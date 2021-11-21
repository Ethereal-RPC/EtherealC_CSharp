using EtherealC.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EtherealC.Client.WebSocket
{

    public class WebSocketClient : Abstract.Client
    {
        #region --字段--
        private ClientWebSocket accept;
        private CancellationToken cancellationToken = CancellationToken.None;
        #endregion

        #region --属性--

        public ClientWebSocket Accept { get => accept; set => accept = value; }
        public new WebSocketClientConfig Config { get => (WebSocketClientConfig)config; set => config = value; }

        #endregion

        public WebSocketClient(string prefixes) : base(prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                OnLog(TrackLog.LogCode.Runtime, "Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            if (prefixes == null)
                throw new ArgumentException(" ");
            this.Prefixes = prefixes;
            // Create a listener.
            Accept = new ClientWebSocket();
            this.config = new WebSocketClientConfig();
            // Add the prefixes.
            Accept.Options.KeepAliveInterval = Config.KeepAliveInterval;
        }

        public async override void Connect()
        {
            try
            {
                if (Prefixes.LastIndexOf('/') != Prefixes.Length - 1) Prefixes += $"/{Request.Name}";
                else Prefixes += $"{Request.Name}";
                await Accept.ConnectAsync(new Uri(Prefixes.Replace("ethereal://", "ws://")), cancellationToken);
                if (Accept.State == WebSocketState.Open)
                {
                    ReceiveAsync();
                    OnConnectSuccess();
                }
                else
                {
                    OnConnnectFail();
                }
            }
            catch (Exception e)
            {
                OnException(new TrackException(e));
                OnConnnectFail();
            }
        }

        public async Task ConnectSync()
        {
            try
            {
                await Accept.ConnectAsync(new Uri(Prefixes.Replace("ethereal://", "ws://")), cancellationToken);
                if (Accept.State == WebSocketState.Open)
                {
                    ReceiveAsync();
                    OnConnectSuccess();
                }
                else
                {
                    OnConnnectFail();
                }
            }
            catch (Exception e)
            {
                OnException(new TrackException(e));
                OnConnnectFail();
            }
        }
        public override void DisConnect()
        {
            DisConnect(WebSocketCloseStatus.NormalClosure, "正常关闭");
        }
        public async void DisConnect(WebSocketCloseStatus status, string message)
        {
            try
            {
                if (Accept?.State == WebSocketState.Open)
                {
                    await Accept?.CloseAsync(status, message, cancellationToken);
                }
                else
                {
                    Accept?.Abort();
                }
            }
            catch
            {

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
                        if (config.Debug) OnLog(TrackLog.LogCode.Runtime, data);
                        JObject token = JObject.Parse(data);
                        offset = 0;
                        free = Config.BufferSize;
                        if (token.TryGetValue("Type", out JToken value))
                        {
                            if (value.ToString() == "ER-1.0-ClientResponse")
                            {
                                ClientResponseModel response = Config.ClientResponseModelDeserialize(data);
                                Request.ClientResponseReceiveProcess(response);
                            }
                            else if (value.ToString() == "ER-1.0-ServerRequest")
                            {
                                ServerRequestModel request = config.ServerRequestModelDeserialize(data);
                                if (Request.Services.TryGetValue(request.Service, out Service.Abstract.Service service))
                                {
                                    service.ServerRequestReceiveProcess(request);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{Request.Name}-{request.Service} 未找到!");
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
                    OnException(TrackException.ErrorCode.Runtime, e.Message);
                }
            }
        }
        internal override async void SendClientRequestModel(ClientRequestModel request)
        {
            if (Accept?.State == WebSocketState.Open)
            {
                string data = config.ClientRequestModelSerialize(request);
                await Accept?.SendAsync(config.Encoding.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}
