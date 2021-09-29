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
                //向网关注册服务
                Service.Abstract.Service netNodeService = ServiceCore.Register<ClientNetNodeService>(this, "ClientNetNodeService", types);
                //向网关注册请求
                ServerNetNodeRequest netNodeRequest = RequestCore.Register<ServerNetNodeRequest,IServerNetNodeRequest>(this, "ServerNetNodeService", types);
                new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            NetNodeSearch();
                            connectSign.WaitOne(Config.NetNodeHeartInterval);
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
        public async void NetNodeSearch()
        {
            bool flag = false;
            foreach (Request.Abstract.Request request in Requests.Values)
            {
                if (request.Client == null && request.ServiceName != "ServerNetNodeService")
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                //搜寻正常启动的注册中心
                foreach (Tuple<string, ClientConfig> item in Config.NetNodeIps)
                {
                    string prefixe = item.Item1;
                    ClientConfig config = item.Item2;
                    //向网关注册连接
                    WebSocketClient client = (WebSocketClient)ClientCore.Register(this, "ServerNetNodeService", prefixe, config);
                    try
                    {
                        await client.ConnectSync();
                        //连接成功
                        if (client?.Accept?.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            if (RequestCore.Get(this, "ServerNetNodeService",
                                out Request.Abstract.Request netNodeRequest))
                            {
                                foreach (Request.Abstract.Request request in Requests.Values)
                                {
                                    if (request.Client == null)
                                    {
                                        //获取服务节点
                                        NetNode node =
                                            (netNodeRequest as ServerNetNodeRequest).GetNetNode(request.ServiceName);
                                        if (node != null)
                                        {
                                            //注册连接并启动连接
                                            Client.Abstract.Client requestClient =
                                                ClientCore.Register(request, node.Prefixes[0]);
                                            requestClient.ConnectFailEvent += ClientConnectFailEvent;
                                            requestClient.DisConnectEvent += ClientDisConnectEvent; ;
                                            requestClient.Connect();
                                        }
                                        else
                                            throw new TrackException(TrackException.ErrorCode.Runtime,
                                                $"{net_name}-{request.ServiceName}-在NetNode分布式中未找到节点");
                                    }
                                }
                                return;
                            }
                            throw new TrackException(TrackException.ErrorCode.Runtime, $"无法找到{net_name}-ServerNetNodeService");
                        }
                    }
                    finally
                    {
                        ClientCore.UnRegister(this, "ServerNetNodeService");
                    }
                }
            }
        }

        private void ClientDisConnectEvent(Client.Abstract.Client client)
        {
            ClientCore.UnRegister(client.NetName, client.ServiceName);
            connectSign.Set();
        }

        private void ClientConnectFailEvent(Client.Abstract.Client client)
        {
            ClientCore.UnRegister(client.NetName, client.ServiceName);
        }
        #endregion
    }
}
