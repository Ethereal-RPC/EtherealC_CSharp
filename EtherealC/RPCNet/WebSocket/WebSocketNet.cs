using System;
using System.Threading;
using EtherealC.Core.Model;
using EtherealC.NativeClient;
using EtherealC.NativeClient.Abstract;
using EtherealC.NativeClient.WebSocket;
using EtherealC.RPCNet.Abstract;
using EtherealC.RPCNet.NetNodeClient.Model;
using EtherealC.RPCNet.NetNodeClient.Request;
using EtherealC.RPCNet.NetNodeClient.Service;
using EtherealC.RPCRequest;
using EtherealC.RPCRequest.Abstract;
using EtherealC.RPCService;
using EtherealC.RPCService.Abstract;

namespace EtherealC.RPCNet.WebSocket
{
    public class WebSocketNet:Net
    {
        #region --字段--
        private AutoResetEvent connectSign = new AutoResetEvent(false);

        #endregion

        #region --属性--
        public new WebSocketNetConfig Config { get => (WebSocketNetConfig)config; set => config = value; }

        #endregion

        #region --方法--
        public override bool Publish()
        {
            //开启分布式模式
            if (Config.NetNodeMode)
            {
                //注册数据类型
                RPCTypeConfig types = new RPCTypeConfig();
                types.Add<int>("Int");
                types.Add<long>("Long");
                types.Add<string>("String");
                types.Add<bool>("Bool");
                types.Add<NetNode>("NetNode");
                //注册网关
                Net net = NetCore.Register($"NetNode-{name}",Core.Enums.NetType.WebSocket);//防止重名
                net.LogEvent += OnLog;
                net.ExceptionEvent += OnException;
                //向网关注册服务
                Service netNodeService = ServiceCore.Register<ClientNetNodeService>(net, "ClientNetNodeService", types);
                //向网关注册请求
                ServerNetNodeRequest netNodeRequest = RequestCore.Register<ServerNetNodeRequest>(net, "ServerNetNodeService", types);
                new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            NetNodeSearch();
                            Thread.Sleep(Config.NetNodeHeartInterval);
                        }
                    }
                    catch(Exception e)
                    {
                        OnException(e);
                    }
                }).Start();
            }
            else
            {
                try
                {
                    foreach (Request request in Requests.Values)
                    {
                        request.Client.Connect();
                    }
                }
                catch(Exception e)
                {
                    OnException(e);
                }
            }
            return true;
        }
        public void NetNodeSearch()
        {
            lock (connectSign)
            {
                bool flag = false;
                foreach (Request request in Requests.Values)
                {
                    if (request.Client == null)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    WebSocketClient client = null;
                    if (!NetCore.Get($"NetNode-{name}", out Net net)) throw new RPCException(RPCException.ErrorCode.Runtime, $"NetNode-{name} 未找到");
                    //搜寻正常启动的注册中心
                    foreach (Tuple<string,ClientConfig> item in Config.NetNodeIps)
                    {
                        string prefixe = item.Item1;
                        ClientConfig config = item.Item2;
                        //向网关注册连接
                        client = (WebSocketClient)ClientCore.Register(net, "ServerNetNodeService",prefixe,config);
                        //关闭分布式模式
                        net.Config.NetNodeMode = false;
                        client.ConnectEvent += SignConnectSuccessEvent;
                        client.DisConnectEvent += SignConnectFailEvent;
                        //启动连接
                        net.Publish();
                        connectSign.WaitOne();
                        client.ConnectEvent -= SignConnectSuccessEvent;
                        client.DisConnectEvent -= SignConnectFailEvent;
                        //连接成功
                        if (client?.Accept?.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            break;
                        }
                        else
                        {
                            ClientCore.UnRegister(net, "ServerNetNodeService");
                        }
                    }

                    if (client?.Accept?.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        if(RequestCore.Get(net, "ServerNetNodeService", out Request netNodeRequest))
                        {
                            foreach (Request request in Requests.Values)
                            {
                                if (request.Client == null)
                                {
                                    //获取服务节点
                                    NetNode node = (netNodeRequest as ServerNetNodeRequest).GetNetNode(request.Name);
                                    if (node != null)
                                    {
                                        //注册连接并启动连接
                                        Client requestClient = ClientCore.Register(request, node.Prefixes[0]);
                                        requestClient.DisConnectEvent += ClientConnectFailEvent;
                                        requestClient.Connect();
                                    }
                                    else throw new RPCException(RPCException.ErrorCode.Runtime,$"{name}-{request.Name}-在NetNode分布式中未找到节点");
                                }
                            }
                        }
                    }
                    ClientCore.UnRegister(net, "ServerNodeService");
                }
            }
        }
        private  void ClientConnectFailEvent(Client client)
        {
            client.DisConnectEvent -= ClientConnectFailEvent;
            ClientCore.UnRegister(client.NetName, client.ServiceName);
            NetNodeSearch();
        }
        private void SignConnectFailEvent(Client client)
        {
            connectSign.Set();
        }

        private void SignConnectSuccessEvent(Client client)
        {
            connectSign.Set();
        }
        #endregion
    }
}
