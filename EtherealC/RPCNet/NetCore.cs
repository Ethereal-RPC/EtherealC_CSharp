using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using EtherealC.Model;
using EtherealC.RPCRequest;
using EtherealC.RPCService;

namespace EtherealC.RPCNet
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class NetCore
    {
        private static Dictionary<string, Net> nets { get; } = new Dictionary<string, Net>();

        public static bool Get(string name, out Net net)
        {
            return nets.TryGetValue(name, out net);
        }

        public static Net Register(string name)
        {
            return Register(name, new NetConfig());
        }
        public static Net Register(string name, NetConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(name, out Net net))
            {
                net = new Net();
                net.Config = config;
                net.Name = name;
                nets.Add(name, net);
                return net;
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{name} Net已经注册");
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Net net))
            {
                return UnRegister(net);
            }
            else return true;
        }
        public static bool UnRegister(Net net)
        {
            //清理请求上的连接
            foreach(Request request in net.Requests.Values)
            {
                request.Client.Disconnect();
                request.Client = null;
            }
            net.Requests.Clear();
            net.Services.Clear();
            return nets.Remove(net.Name);
        }
    }
}
