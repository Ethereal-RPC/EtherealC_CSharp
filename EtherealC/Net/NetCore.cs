    using EtherealC.Core.Model;
using EtherealC.Request;
using System.Collections.Generic;

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
            if (!net.IsRegister)
            {
                net.isRegister = true;
                nets.Add(net.Name, net);
                return net;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name} Net已经注册");
        }
        public static bool UnRegister(Abstract.Net net)
        {
            if (net.isRegister)
            {
                net.Requests.Clear();
                nets.Remove(net.Name);
                net.isRegister = false;
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}并未注册，无需UnRegister");
        }
    }
}
