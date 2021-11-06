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
        public static Abstract.Net Register(Abstract.Net net)
        {
            if (!nets.ContainsKey(net.Name))
            {
                nets.Add(net.Name, net);
                return net;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name} Net已经注册");
        }
        public static bool UnRegister(Abstract.Net net)
        {
            if(net != null)
            {
                if (net.Type == Net.Abstract.Net.NetType.WebSocket)
                {
                    (net.Client as WebSocketClient).DisConnect(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "UnRegister");
                }
                net.Requests.Clear();
                net.Services.Clear();
                nets.Remove(net.Name);
            }
            return true;
        }
    }
}
