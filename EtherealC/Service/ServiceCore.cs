using EtherealC.Core.Model;
using EtherealC.Request;

namespace EtherealC.Service
{
    public class ServiceCore
    {
        public static bool Get<R>(string net_name, string request_name, string service_name, out R service) where R : Abstract.Service
        {
            if (RequestCore.Get(net_name, request_name, out Request.Abstract.Request request))
            {
                return Get<R>(request, service_name, out service);
            }
            service = null;
            return false;
        }
        public static bool Get<R>(Request.Abstract.Request request, string service_name, out R service) where R : Abstract.Service
        {
            if (request.Services.TryGetValue(service_name, out Abstract.Service value))
            {
                service = value as R;
                return true;
            }
            service = null;
            return false;
        }
        public static T Register<T>(Request.Abstract.Request request, T service, string serviceName = null) where T : Abstract.Service
        {
            service.Initialize();
            if (serviceName != null) service.name = serviceName;
            if (!service.IsRegister)
            {
                service.isRegister = true;
                Abstract.Service.Register(service);
                service.Request = request;
                service.LogEvent += request.OnLog;
                service.ExceptionEvent += request.OnException;
                request.Services[service.Name] = service;
                service.Register();
                service.isRegister = true;
                return service;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{request.Net.Name}-{request.Name}-{service.Name}已注册！");
        }
        public static bool UnRegister(Abstract.Service service)
        {
            if (service.IsRegister)
            {
                service.UnRegister();
                if (service.Request != null)
                {
                    service.Request.Services.Remove(service.Name, out service);
                    service.LogEvent -= service.Request.OnLog;
                    service.ExceptionEvent -= service.Request.OnException;
                    service.Request = null;
                }
                service.isRegister = false;
                service.Initialize();
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{service.Name}并未注册，无需UnRegister");
        }
    }
}
