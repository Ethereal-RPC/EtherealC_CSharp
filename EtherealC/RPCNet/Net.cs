using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet.Client.Model;
using EtherealC.RPCNet.Client.Request;
using EtherealC.RPCNet.Client.Service;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace EtherealC.RPCNet
{
    public class Net
    {
        #region --委托--
        public delegate void ServerRequestReceiveDelegate(ServerRequestModel request);
        public delegate void ClientResponseReceiveDelegate(ClientResponseModel respond);
        public delegate bool ClientRequestSendDelegate(ClientRequestModel request);
        public delegate void OnLogDelegate(RPCLog log, Net net);
        public delegate void OnExceptionDelegate(Exception exception, Net net);
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
        #endregion

        #region --字段--
        private ServerRequestReceiveDelegate serverRequestReceive;
        private ClientResponseReceiveDelegate clientResponseReceive;
        private NetConfig config;
        /// <summary>
        /// Net网关名
        /// </summary>
        private string name;
        /// <summary>
        /// Service映射表
        /// </summary>
        private ConcurrentDictionary<string, Service> services = new ConcurrentDictionary<string, Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        private Dictionary<string, Request> requests = new Dictionary<string, Request>();

        private AutoResetEvent connectSign = new AutoResetEvent(false);
        #endregion

        #region --属性--
        public ServerRequestReceiveDelegate ServerRequestReceive { get => serverRequestReceive; set => serverRequestReceive = value; }
        public ClientResponseReceiveDelegate ClientResponseReceive { get => clientResponseReceive; set => clientResponseReceive = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; set => services = value; }
        public Dictionary<string, Request> Requests { get => requests; set => requests = value; }
        public NetConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }

        #endregion
        public Net()
        {
            serverRequestReceive = ServerRequestReceiveProcess;
            clientResponseReceive = ClientResponseReceiveProcess;
        }
        #region --方法--

        public bool Publish()
        {
            //开启分布式模式
            if (config.NetNodeMode)
            {
                //注册数据类型
                RPCTypeConfig types = new RPCTypeConfig();
                types.Add<int>("Int");
                types.Add<long>("Long");
                types.Add<string>("String");
                types.Add<bool>("Bool");
                types.Add<NetNode>("NetNode");
                //注册网关
                Net net = NetCore.Register($"NetNode-{name}");//防止重名
                net.LogEvent += NetNode_LogEvent;
                net.ExceptionEvent += NetNode_ExceptionEvent;
                //向网关注册服务
                Service netNodeService = ServiceCore.Register<ClientNetNodeService>(net, "ClientNetNodeService", types);
                //向网关注册请求
                ServerNetNodeRequest netNodeRequest = RequestCore.Register<ServerNetNodeRequest>(net, "ServerNetNodeService", types);
                new Thread(() =>
                {
                    while (true)
                    {
                        NetNodeSearch();
                        Thread.Sleep(config.NetNodeHeartInterval);
                    }
                }).Start();
            }
            else
            {
                foreach (Request request in requests.Values)
                {
                    request.Client.Start();
                }
            }
            return true;
        }

        private void NetNode_ExceptionEvent(Exception exception, Net net)
        {
            OnException(exception);
        }

        private void NetNode_LogEvent(RPCLog log, Net net)
        {
            OnLog(log);
        }

        public void NetNodeSearch()
        {
            lock (connectSign)
            {
                bool flag = false;
                foreach (Request request in requests.Values)
                {
                    if (request.Client == null)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    SocketClient client = null;
                    if (!NetCore.Get($"NetNode-{name}", out Net net)) OnException(RPCException.ErrorCode.Runtime, $"NetNode-{name} 未找到");
                    //搜寻正常启动的注册中心
                    foreach (Tuple<string, string,ClientConfig> item in config.NetNodeIps)
                    {
                        string ip = item.Item1;
                        string port = item.Item2;
                        ClientConfig config = item.Item3;
                        //向网关注册连接
                        client = ClientCore.Register(net, "ServerNetNodeService", ip, port,config);
                        //关闭分布式模式
                        net.config.NetNodeMode = false;
                        client.ConnectSuccessEvent += SignConnectSuccessEvent;
                        client.ConnectFailEvent += SignConnectFailEvent;
                        //启动连接
                        net.Publish();
                        connectSign.WaitOne();
                        client.ConnectSuccessEvent -= SignConnectSuccessEvent;
                        client.ConnectFailEvent -= SignConnectFailEvent;
                        //连接成功
                        if (client?.DataToken?.SocketArgs?.AcceptSocket?.Connected == true)
                        {
                            break;
                        }
                        else
                        {
                            ClientCore.UnRegister(net, "ServerNetNodeService");
                        }
                    }

                    if (client?.DataToken?.SocketArgs?.AcceptSocket?.Connected == true)
                    {
                        if(RequestCore.Get(net, "ServerNetNodeService", out Request netNodeRequest))
                        {
                            foreach (Request request in requests.Values)
                            {
                                if (request.Client == null)
                                {
                                    //获取服务节点
                                    NetNode node = (netNodeRequest as ServerNetNodeRequest).GetNetNode(request.Name);
                                    if (node != null)
                                    {
                                        //注册连接并启动连接
                                        SocketClient requestClient = ClientCore.Register(request, node.Ip, node.Port);
                                        requestClient.ConnectFailEvent += ClientConnectFailEvent;
                                        requestClient.Start();
                                    }
                                    else OnException(RPCException.ErrorCode.Runtime,$"{name}-{request.Name}-在NetNode分布式中未找到节点");
                                }
                            }
                        }
                    }
                    ClientCore.UnRegister(net, "ServerNodeService");
                }
            }
        }
        private  void ClientConnectFailEvent(SocketClient client)
        {
            client.ConnectFailEvent -= ClientConnectFailEvent;
            ClientCore.UnRegister(client.NetName, client.ServiceName);
            NetNodeSearch();
        }
        private void SignConnectFailEvent(SocketClient client)
        {
            connectSign.Set();
        }

        private void SignConnectSuccessEvent(SocketClient client)
        {
            connectSign.Set();
        }

        internal void OnRequestException(Exception exception, Request request)
        {
            OnException(exception);
        }

        internal void OnRequestLog(RPCLog log, Request request)
        {
            OnLog(log);
        }

        internal void OnServiceException(Exception exception, Service service)
        {
            OnException(exception);
        }

        internal void OnServiceLog(RPCLog log, Service service)
        {
            OnLog(log);
        }
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e, this);
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
                logEvent.Invoke(log,this);
            }
        }

        private void ServerRequestReceiveProcess(ServerRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string log = "";
                    log += "---------------------------------------------------------\n";
                    log += $"{DateTime.Now}::{name}::[服-指令]\n{request}\n";
                    log += "---------------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime,log);
                    string[] param_id = request.MethodId.Split('-');
                    for (int i = 1, j = 0; i < param_id.Length; i++, j++)
                    {
                        if (service.Config.Types.TypesByName.TryGetValue(param_id[i], out RPCType type))
                        {
                            request.Params[j] = type.Deserialize((string)request.Params[j]);
                        }
                        else service.OnException(RPCException.ErrorCode.Runtime,$"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                    }
                    method.Invoke(service, request.Params);
                }
                else service.OnException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service}-{request.MethodId}未找到!");
            }
            else OnException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service} 未找到!");
        }
        private void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            string log = "";
            log += "---------------------------------------------------------\n";
            log += $"{DateTime.Now}::{name}::[服-返回]\n{response}\n";
            log += "---------------------------------------------------------\n";
            OnLog(RPCLog.LogCode.Runtime, log);
            if (int.TryParse(response.Id, out int id) && Requests.TryGetValue(response.Service, out Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
                else OnException(RPCException.ErrorCode.Runtime, $"{name}-{response.Service}-{id}返回的请求ID未找到!");
            }
            else OnException(RPCException.ErrorCode.Runtime, $"{name}-{response.Service}未找到!");
        }
        #endregion
    }
}
