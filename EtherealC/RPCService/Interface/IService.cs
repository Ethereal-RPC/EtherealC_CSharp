using EtherealC.Core.Interface;
using EtherealC.RPCService.Abstract;

namespace EtherealC.RPCService.Interface
{
    public interface IService:ILogEvent,IExceptionEvent
    {
        public void Register<T>(T instance, string netName, string servicename, ServiceConfig config);
    }
}
