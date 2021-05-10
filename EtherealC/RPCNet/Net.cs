using EtherealC.Model;
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
        private Tuple<string, string> clientKey;
        private ServerRequestReceiveDelegate serverRequestReceive;
        private ClientResponseReceiveDelegate clientResponseReceive;
        private ClientRequestSendDelegate clientRequestSend;
        private NetConfig config;
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
        public Tuple<string, string> ClientKey { get => clientKey; set => clientKey = value; }

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
#if DEBUG
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine($"{DateTime.Now}::{clientKey.Item1}:{clientKey.Item2}::[服-指令]\n{request}");
                    Console.WriteLine("---------------------------------------------------------");
#endif
                    string[] param_id = request.MethodId.Split('-');
                    for (int i = 1, j = 0; i < param_id.Length; i++, j++)
                    {
                        if (service.Config.Types.TypesByName.TryGetValue(param_id[i], out RPCType type))
                        {
                            request.Params[j] = type.Deserialize((string)request.Params[j]);
                        }
                        else throw new RPCException($"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                    }
                    method.Invoke(service.Instance, request.Params);
                }
                else throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{clientKey.Item1}:{clientKey.Item2}-{request.Service}-{request.MethodId}未找到!");
            }
            else throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{clientKey.Item1}:{clientKey.Item2}-{request.Service} 未找到!");
        }
        private void ClientResponseReceiveProcess(ClientResponseModel response)
        {
#if DEBUG
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"{DateTime.Now}::{clientKey.Item1}:{clientKey.Item2}::[服-返回]\n{response}");
            Console.WriteLine("---------------------------------------------------------");
#endif
            if (int.TryParse(response.Id, out int id) && Requests.TryGetValue(response.Service, out Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
            }
            else throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{clientKey.Item1}:{clientKey.Item2}-{response.Service}未找到!");
        }
        #endregion
    }
}
