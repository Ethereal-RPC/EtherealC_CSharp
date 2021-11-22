using EtherealC.Client;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Service;

namespace EtherealC.Request
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get<T>(string netName, string serviceName, out T reqeust) where T : Abstract.Request
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, serviceName, out reqeust);
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
        public static R Register<R>(Net.Abstract.Net net, string serviceName = null) where R : Abstract.Request
        {
            R request = Abstract.Request.Register<R>();
            request.Initialize();
            if (serviceName != null) request.Name = serviceName;
            if (!net.Requests.ContainsKey(request.Name))
            {
                request.Net = net;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
                net.Requests[request.Name] = request;
                request.Register();
                return request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{serviceName}已注册，无法重复注册！");
        }

        public static bool UnRegister(Abstract.Request request)
        {
            request.UnRegister();
            ClientCore.UnRegister(request.Client);
            foreach (Service.Abstract.Service service in request.Services.Values)
            {
                ServiceCore.UnRegister(service);
            }
            request.Net.Requests.Remove(request.Name, out request);
            request.LogEvent -= request.Net.OnLog;
            request.ExceptionEvent -= request.Net.OnException;
            request.Net = null;
            request.UnInitialize();
            return true;
        }
        #endregion
    }
}
