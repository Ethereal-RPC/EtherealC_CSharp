using EtherealC.Core.Interface;
using EtherealC.Service.Abstract;

namespace EtherealC.Service.Interface
{
    public interface IService:ILogEvent,IExceptionEvent
    {
        public void Register<T>(T instance, string netName, string servicename, ServiceConfig config);
    }
}
