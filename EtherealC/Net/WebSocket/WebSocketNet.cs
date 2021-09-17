using System;
using System.Threading;
using EtherealC.Client;
using EtherealC.Client.Abstract;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net.NetNodeClient.Model;
using EtherealC.Net.NetNodeClient.Request;
using EtherealC.Net.NetNodeClient.Service;
using EtherealC.Request;
using EtherealC.Service;

namespace EtherealC.Net.WebSocket
{
    public class WebSocketNet:Abstract.Net
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
                AbstractTypes types = new AbstractTypes();
                types.Add<int>("Int");
                types.Add<long>("Long");
                types.Add<string>("String");
                types.Add<bool>("Bool");
                types.Add<NetNode>("NetNode");
                //注册网关
                Abstract.Net net = NetCore.Register($"NetNode-{name}",NetType.WebSocket);//防止重名
                net.LogEvent += OnLog;
                net.ExceptionEvent += OnException;
                //向网关注册服务
                Service.Abstract.Service netNodeService = ServiceCore.Register<ClientNetNodeService>(net, "ClientNetNodeService", types);
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
                        OnException(new TrackException(e));
                    }
                }).Start();
            }
            else
            {
                try
                {
                    foreach (Request.Abstract.Request request in Requests.Values)
                    {
                        request.Client.Connect();
                    }
                }
                catch(Exception e)
                {
                    OnException(new TrackException(e));
                }
            }
            return true;
        }
        public void NetNodeSearch()
        {
            lock (connectSign)
            {
                bool flag = false;
                foreach (Request.Abstract.Request request in Requests.Values)
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
                    if (!NetCore.Get($"NetNode-{name}", out Abstract.Net net)) throw new TrackException(TrackException.ErrorCode.Runtime, $"NetNode-{name} 未找到");
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
                        if(RequestCore.Get(net, "ServerNetNodeService", out Request.Abstract.Request netNodeRequest))
                        {
                            foreach (Request.Abstract.Request request in Requests.Values)
                            {
                                if (request.Client == null)
                                {
                                    //获取服务节点
                                    NetNode node = (netNodeRequest as ServerNetNodeRequest).GetNetNode(request.Name);
                                    if (node != null)
                                    {
                                        //注册连接并启动连接
                                        Client.Abstract.Client requestClient = ClientCore.Register(request, node.Prefixes[0]);
                                        requestClient.DisConnectEvent += ClientConnectFailEvent;
                                        requestClient.Connect();
                                    }
                                    else throw new TrackException(TrackException.ErrorCode.Runtime,$"{name}-{request.Name}-在NetNode分布式中未找到节点");
                                }
                            }
                        }
                    }
                    ClientCore.UnRegister(net, "ServerNodeService");
                }
            }
        }
        private  void ClientConnectFailEvent(Client.Abstract.Client client)
        {
            client.DisConnectEvent -= ClientConnectFailEvent;
            ClientCore.UnRegister(client.NetName, client.ServiceName);
            NetNodeSearch();
        }
        private void SignConnectFailEvent(Client.Abstract.Client client)
        {
            connectSign.Set();
        }

        private void SignConnectSuccessEvent(Client.Abstract.Client client)
        {
            connectSign.Set();
        }
        #endregion
    }
}
