using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using EtherealC.Client.Abstract;
using EtherealC.Core.Model;
using EtherealC.Net;
using Newtonsoft.Json.Linq;

namespace EtherealC.Client.WebSocket
{

    public class WebSocketClient : Abstract.Client
    {
        #region --字段--

        private string prefixes;
        private ClientWebSocket accept;
        private CancellationToken cancellationToken = CancellationToken.None;
        private bool isDisConnect = false;

        #endregion

        #region --属性--

        public ClientWebSocket Accept { get => accept; set => accept = value; }
        public string Prefixes { get => prefixes; set => prefixes = value; }
        public new WebSocketClientConfig Config { get => (WebSocketClientConfig)config; set => config = value; }

        #endregion

        public WebSocketClient(string netName,string serviceName,string prefixes, ClientConfig config):base(netName,serviceName)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(TrackLog.LogCode.Runtime,"Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            if (prefixes == null)
                throw new ArgumentException(" ");
            this.Prefixes = prefixes;
            // Create a listener.
            Accept = new ClientWebSocket();
            this.config = config as WebSocketClientConfig;
            // Add the prefixes.
            Accept.Options.KeepAliveInterval = Config.KeepAliveInterval;
        }

        public async override void Connect()
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
                OnException(new TrackException(e));
                DisConnect();
            }
        }

        public async Task ConnectSync()
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
                OnException(new TrackException(e));
                DisConnect();
            }
        }
        public override void DisConnect()
        {
            DisConnect(WebSocketCloseStatus.NormalClosure, "正常关闭");
        }
        public async void DisConnect(WebSocketCloseStatus status,string message)
        {
            bool temp = isDisConnect;
            try
            {
                if (!isDisConnect)
                {
                    isDisConnect = true;
                    if (Accept?.State == WebSocketState.Open)
                    {
                        await Accept?.CloseAsync(status, message, cancellationToken);
                    }
                    else
                    {
                        Accept?.Abort();
                    }
                }
            }
            catch(Exception e)
            {
                OnException(new TrackException(e));
            }
            finally
            {
                if (!temp)
                {
                    OnDisConnect();
                }
            }
        }
        public async void ReceiveAsync()
        {   
            byte[] receiveBuffer = null;
            int offset = 0;
            int free = Config.BufferSize;
            // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
            while (Accept.State == WebSocketState.Open)
            {
                if (receiveBuffer == null)
                {
                    receiveBuffer = new byte[Config.BufferSize];
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
                        string data = Config.Encoding.GetString(receiveBuffer);
                        JObject token = JObject.Parse(data);
                        offset = 0;
                        free = Config.BufferSize;
                        if(token.TryGetValue("Type",out JToken value))
                        {
                            if (value.ToString() == "ER-1.0-ClientResponse")
                            {
                                ClientResponseModel response = Config.ClientResponseModelDeserialize(data);
                                if (!NetCore.Get(netName, out Net.Abstract.Net net))
                                {
                                    throw new TrackException(TrackException.ErrorCode.Runtime, $"查询{netName} Net时 不存在");
                                }
                                net.ClientResponseReceiveProcess(response);
                            }
                            else if(value.ToString() == "ER-1.0-ServerRequest")
                            {
                                ServerRequestModel request = config.ServerRequestModelDeserialize(data);
                                if (!NetCore.Get(netName, out Net.Abstract.Net net))
                                {
                                    throw new TrackException(TrackException.ErrorCode.Runtime, $"查询{netName} Net时 不存在");
                                }
                                net.ServerRequestReceiveProcess(request);
                            }
                        }
                    }
                    else if (free == 0)
                    {
                        var newSize = receiveBuffer.Length + Config.BufferSize;
                        if (newSize > Config.MaxBufferSize)
                        {
                            DisConnect(WebSocketCloseStatus.MessageTooBig, $"缓冲区:{newSize}-超过最大字节数:{Config.MaxBufferSize}，已断开连接！");
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
                    DisConnect(WebSocketCloseStatus.NormalClosure, $"{e.Message}");
                }
            }
        }
        public override async void SendClientRequestModel(ClientRequestModel request)
        {
            if (Accept.State == WebSocketState.Open)
            {
                string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{netName}::[客-请求]\n{request}\n" +
                            "--------------------------------------------------\n";
                OnLog(TrackLog.LogCode.Runtime, log);
                await Accept.SendAsync(config.Encoding.GetBytes(config.ClientRequestModelSerialize(request)), WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}
