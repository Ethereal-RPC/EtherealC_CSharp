using EtherealC.Model;
using EtherealC.RPCNet;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtherealC.NativeClient
{

    public class Client
    {

        #region --委托--

        public delegate void OnExceptionDelegate(Exception exception, Client client);

        public delegate void OnLogDelegate(RPCLog log, Client client);

        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(Client client);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(Client client);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }

        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--
        private string netName;
        private string serviceName;
        private string prefixes;
        private ClientConfig config;
        private ClientWebSocket accept;
        private CancellationToken cancellationToken = CancellationToken.None;

        #endregion

        #region --属性--

        public string NetName { get => netName; set => netName = value; }
        public string Prefixes { get => prefixes; set => prefixes = value; }
        public ClientWebSocket Accept { get => accept; set => accept = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        #endregion
        public Client(string netName,string serviceName,string prefixes, ClientConfig config)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(RPCLog.LogCode.Runtime,"Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            if (prefixes == null)
                throw new ArgumentException(" ");
            this.Prefixes = prefixes;
            this.NetName = netName;
            this.ServiceName = serviceName;
            this.config = config;
            // Create a listener.
            Accept = new ClientWebSocket();
            // Add the prefixes.
            Accept.Options.KeepAliveInterval = config.KeepAliveInterval;
        }

        public async void Start()
        {
            try
            {
                await Accept.ConnectAsync(new Uri("ws://" + Prefixes), cancellationToken);
                if (Accept.State == WebSocketState.Open)
                {
                    ReceiveAsync();
                    OnConnect();
                }
                else
                {
                    OnDisConnect();
                }
            }
            catch (Exception e)
            {
                OnException(e);
                OnDisConnect();
            }
        }
        public async void Close(WebSocketCloseStatus code,string description)
        {
            try
            {
                if(cancellationToken.CanBeCanceled == true)
                {
                    await Accept.CloseAsync(code, description, cancellationToken);
                    OnDisConnect();
                }
            }
            catch(Exception e)
            {
                OnException(e);
            }
        }
        public async void ReceiveAsync()
        {   
            byte[] receiveBuffer = null;
            int offset = 0;
            int free = config.BufferSize;
            // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
            while (Accept.State == WebSocketState.Open)
            {
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[config.BufferSize];
                }
                try
                {
                    WebSocketReceiveResult receiveResult = await Accept.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, offset, free), cancellationToken);

                    offset += receiveResult.Count;
                    free -= receiveResult.Count;
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        OnDisConnect();
                        await Accept.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                        continue;
                    }

                    if (receiveResult.EndOfMessage)
                    {
                        string data = config.Encoding.GetString(receiveBuffer);
                        JObject token = JObject.Parse(data);
                        offset = 0;
                        free = config.BufferSize;
                        if(token.TryGetValue("Type",out JToken value))
                        {
                            if (value.ToString() == "ER-1.0-ClientResponse")
                            {
                                ClientResponseModel response = config.ClientResponseModelDeserialize(data);
                                if (!NetCore.Get(netName, out Net net))
                                {
                                    throw new RPCException(RPCException.ErrorCode.Runtime, $"查询{netName} Net时 不存在");
                                }
                                net.ClientResponseReceive(response);
                            }
                            else if(value.ToString() == "ER-1.0-ServerRequest")
                            {
                                ServerRequestModel request = config.ServerRequestModelDeserialize(data);
                                if (!NetCore.Get(netName, out Net net))
                                {
                                    throw new RPCException(RPCException.ErrorCode.Runtime, $"查询{netName} Net时 不存在");
                                }
                                net.ServerRequestReceive(request);
                            }
                        }
                    }
                    else if (free == 0)
                    {
                        var newSize = receiveBuffer.Length + config.BufferSize;
                        if (newSize > config.MaxBufferSize)
                        {
                            Close(WebSocketCloseStatus.MessageTooBig, $"缓冲区:{newSize}-超过最大字节数:{config.MaxBufferSize}，已断开连接！");
                            return;
                        }
                        byte[] new_bytes = new byte[newSize];
                        Array.Copy(receiveBuffer, 0, new_bytes, 0, offset);
                        receiveBuffer = new_bytes;
                        free = receiveBuffer.Length - offset;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Close(WebSocketCloseStatus.NormalClosure, $"{e.Message}");
                }
            }
        }
        public async Task<bool> SendAsync(ClientRequestModel request)
        {
            if (accept.State == WebSocketState.Open)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{netName}::[客-请求]\n{request}\n" +
                            "--------------------------------------------------\n";
                OnLog(RPCLog.LogCode.Runtime, log);
                await accept.SendAsync(config.Encoding.GetBytes(config.ClientRequestModelSerialize(request)), WebSocketMessageType.Text, true, cancellationToken);
                return true;
            }
            else return false;
        }
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e,this);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                logEvent.Invoke(log, this);
            }
        }

        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        public void OnConnect()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        public void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }
    }
}
