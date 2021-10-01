using EtherealC.Client;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request.WebSocket;

namespace EtherealC.Request
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get<T>(string netName, string serviceName, out T reqeust)where T:Abstract.Request
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get<T>(net, serviceName, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }
        public static bool Get<T>(Net.Abstract.Net net, string serviceName, out T reqeust) where T : Abstract.Request
        {
            bool result = net.Requests.TryGetValue(serviceName, out Abstract.Request value);
            reqeust = (T)value;
            return result;
        }
        public static R Register<R, T>(Net.Abstract.Net net) where R : Abstract.Request
        {
            return Register<R, T>(net, null, null);
        }
        public static R Register<R, T>(Net.Abstract.Net net, string serviceName, AbstractTypes types) where R : Abstract.Request
        {
            Abstract.Request request = Abstract.Request.Register<R, T>();
            if (serviceName != null) request.Name = serviceName;
            if (types != null) request.Types = types;
            if (!net.Requests.ContainsKey(request.Name))
            {
                request.NetName = net.Name;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
                net.Requests[request.Name] = request;
                return (R)request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{serviceName}已注册，无法重复注册！");
        }

        public static bool UnRegister(string netName, string serviceName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if(net != null)
            {
                net.Requests.Remove(serviceName, out Request.Abstract.Request request);
                request.LogEvent -= net.OnLog;
                request.ExceptionEvent -= net.OnException;
                ClientCore.UnRegister(request);
            }
            return true;
        }
        #endregion
    }
}
