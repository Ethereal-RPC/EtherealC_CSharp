using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using EtherealC.Core.Model;
using EtherealC.NativeClient;
using EtherealC.NativeClient.WebSocket;
using EtherealC.RPCNet.Abstract;
using EtherealC.RPCNet.WebSocket;
using EtherealC.RPCRequest;
using EtherealC.RPCRequest.Abstract;
using EtherealC.RPCService;
using EtherealC.Core.Enums;

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
        public static Net Register(string name,NetType netType)
        {
            if(netType == NetType.WebSocket)
            {
                return Register(name, new WebSocketNetConfig(), netType);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{netType}的Net-Register处理");
        }
        public static Net Register(string name, NetConfig config, NetType netType)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(name, out Net net))
            {
                if (netType == NetType.WebSocket)
                {
                    net = new WebSocketNet();
                    net.Config = config;
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.NetType}的Net-Register处理");
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
            if(net != null)
            {
                //清理请求上的连接
                foreach (Request request in net.Requests.Values)
                {
                    if(net.NetType == NetType.WebSocket)
                    {
                        (request.Client as WebSocketClient).DisConnect(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "UnRegister");
                    }
                    request.Client = null;
                }
                net.Requests.Clear();
                net.Services.Clear();
                nets.Remove(net.Name);
            }
            return true;
        }
    }
}
