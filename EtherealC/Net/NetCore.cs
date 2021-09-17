using System;
using System.Collections.Generic;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net.Abstract;
using EtherealC.Net.WebSocket;

namespace EtherealC.Net
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class NetCore
    {
        private static Dictionary<string, Net.Abstract.Net> nets { get; } = new Dictionary<string, Net.Abstract.Net>();

        public static bool Get(string name, out Net.Abstract.Net net)
        {
            return nets.TryGetValue(name, out net);
        }
        public static Net.Abstract.Net Register(string name,Net.Abstract.Net.NetType netType)
        {
            if(netType == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(name, new WebSocketNetConfig(), netType);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{netType}的Net-Register处理");
        }
        public static Net.Abstract.Net Register(string name, NetConfig config, Net.Abstract.Net.NetType netType)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(name, out Net.Abstract.Net net))
            {
                if (netType == Net.Abstract.Net.NetType.WebSocket)
                {
                    net = new WebSocketNet();
                    net.Config = config;
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.Type}的Net-Register处理");
                net.Name = name;
                nets.Add(name, net);
                return net;
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{name} Net已经注册");
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Net.Abstract.Net net))
            {
                return UnRegister(net);
            }
            else return true;
        }
        public static bool UnRegister(Net.Abstract.Net net)
        {
            if(net != null)
            {
                //清理请求上的连接
                foreach (Request.Abstract.Request request in net.Requests.Values)
                {
                    if(net.Type == Net.Abstract.Net.NetType.WebSocket)
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
