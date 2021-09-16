using EtherealC.Core.Interface;

namespace EtherealC.RPCService
{
    public interface IService:ILogEvent,IExceptionEvent
    {
        public void Register<T>(T instance, string netName, string servicename, ServiceConfig config);
    }
}
