using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using Newtonsoft.Json;

namespace EtherealC.RPCNet
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class NetCore
    {
        private static Dictionary<Tuple<string, string>, NetConfig> configs { get; } = new Dictionary<Tuple<string, string>, NetConfig>();
        public static bool Get(Tuple<string, string> key, out NetConfig config)
        {
            return configs.TryGetValue(key, out config);
        }
        public static void Register(string ip, string port)
        {
            Register(ip, port, new NetConfig());
        }
        public static void Register(string ip, string port, NetConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!configs.TryGetValue(new Tuple<string, string>(ip, port), out NetConfig value))
            {
                if (config.ServerRequestReceive == null) config.ServerRequestReceive = ServerRequestReceive;
                if (config.ClientResponseReceive == null) config.ClientResponseReceive = ClientResponseReceive;
                configs.Add(new Tuple<string, string>(ip, port), config);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}服务的NetConfig已经注册");
        }
        public static bool UnRegister(string ip, string port)
        {
            return configs.Remove(new Tuple<string, string>(ip, port));
        }
        private static void ServerRequestReceive(string ip, string port, NetConfig config, ServerRequestModel request)
        {
            if (ServiceCore.Get(new Tuple<string, string, string>(ip, port,request.Service), out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
#if DEBUG
                    Console.WriteLine("---------------------------------------------------------");
                    Console.WriteLine($"{DateTime.Now}::{ip}:{port}::[服-指令]\n{request}");
                    Console.WriteLine("---------------------------------------------------------");
#endif
                    service.ConvertParams(request.MethodId, request.Params);
                    method.Invoke(service.Instance, request.Params);
                }
#if DEBUG
                else throw new RPCException(RPCException.ErrorCode.RuntimeError, $" {ip}-{port}-{request.Service}-{request.MethodId}未找到!");
#endif
            }
#if DEBUG
            else throw new RPCException(RPCException.ErrorCode.RuntimeError, $" {ip}-{port}-{request.Service} 未找到!");
#endif
        }
        private static void ClientResponseReceive(string ip, string port, NetConfig netConfig, ClientResponseModel response)
        {
#if DEBUG
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"{DateTime.Now}::{ip}:{port}::[服-返回]\n{response}");
            Console.WriteLine("---------------------------------------------------------");
#endif
            if (int.TryParse(response.Id, out int id) && RequestCore.Get(new Tuple<string, string, string>(ip, port, response.Service), out Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
            }
#if DEBUG
            else throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{ip}-{port}-{response.Service}未找到!");
#endif
        }
    }
}
