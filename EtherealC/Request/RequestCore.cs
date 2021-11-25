using Castle.DynamicProxy;
using EtherealC.Client;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request.Abstract;
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
        public static T Register<T>(Net.Abstract.Net net, string serviceName = null) where T : Abstract.Request
        {
            ProxyGenerator generator = new ProxyGenerator();
            RequestInterceptor interceptor = new RequestInterceptor();
            T request = generator.CreateClassProxy<T>(interceptor);
            request.Initialize();
            if (serviceName != null) request.name = serviceName;
            if (!net.IsRegister)
            {
                request.isRegister = true;
                Abstract.Request.Register(request);
                request.Net = net;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
                net.Requests[request.Name] = request;
                request.Register();
                request.isRegister = true;
                return request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{serviceName}已注册，无法重复注册！");
        }

        public static bool UnRegister(Abstract.Request request)
        {
            if (request.IsRegister)
            {
                request.UnRegister();
                if(request.Net != null)
                {
                    request.Net.Requests.Remove(request.Name, out request);
                    request.LogEvent -= request.Net.OnLog;
                    request.ExceptionEvent -= request.Net.OnException;
                    request.Net = null;
                }
                request.UnInitialize();
                request.isRegister = false;
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{request.Name}并未注册，无需UnRegister");
        }
        #endregion
    }
}
