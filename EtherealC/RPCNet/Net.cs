using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EtherealC.RPCNet
{
    public class Net
    {
        #region --委托--
        public delegate void ServerRequestReceiveDelegate(ServerRequestModel request);
        public delegate void ClientResponseReceiveDelegate(ClientResponseModel respond);
        public delegate void ClientRequestSendDelegate(ClientRequestModel request);
        #endregion

        #region --字段--
        private ServerRequestReceiveDelegate serverRequestReceive;
        private ClientResponseReceiveDelegate clientResponseReceive;
        private ClientRequestSendDelegate clientRequestSend;
        private NetConfig config;
        /// <summary>
        /// Server
        /// </summary>
        private SocketClient client;
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
        #endregion

        #region --属性--
        public ServerRequestReceiveDelegate ServerRequestReceive { get => serverRequestReceive; set => serverRequestReceive = value; }
        public ClientRequestSendDelegate ClientRequestSend { get => clientRequestSend; set => clientRequestSend = value; }
        public ClientResponseReceiveDelegate ClientResponseReceive { get => clientResponseReceive; set => clientResponseReceive = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; set => services = value; }
        public Dictionary<string, Request> Requests { get => requests; set => requests = value; }
        public NetConfig Config { get => config; set => config = value; }
        public SocketClient Client { get => client; set => client = value; }
        public string Name { get => name; set => name = value; }

        #endregion
        public Net()
        {
            serverRequestReceive = ServerRequestReceiveProcess;
            clientResponseReceive = ClientResponseReceiveProcess;
        }
        #region --方法--
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
                    config.OnLog(RPCLog.LogCode.Runtime,log,this);
                    string[] param_id = request.MethodId.Split('-');
                    for (int i = 1, j = 0; i < param_id.Length; i++, j++)
                    {
                        if (service.Config.Types.TypesByName.TryGetValue(param_id[i], out RPCType type))
                        {
                            request.Params[j] = type.Deserialize((string)request.Params[j]);
                        }
                        else service.Config.OnException(RPCException.ErrorCode.Runtime,$"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册",service);
                    }
                    method.Invoke(service.Instance, request.Params);
                }
                else service.Config.OnException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service}-{request.MethodId}未找到!",service);
            }
            else config.OnException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service} 未找到!",this);
        }
        private void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            string log = "";
            log += "---------------------------------------------------------\n";
            log += $"{DateTime.Now}::{name}::[服-返回]\n{response}\n";
            log += "---------------------------------------------------------\n";
            config.OnLog(RPCLog.LogCode.Runtime, log,this);
            if (int.TryParse(response.Id, out int id) && Requests.TryGetValue(response.Service, out Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
            }
            config.OnException(RPCException.ErrorCode.Runtime, $"{name}-{response.Service}未找到!",this);
        }
        #endregion
    }
}
