using EtherealC.Core.Interface;

namespace EtherealC.Service.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Initialize();
        public void UnInitialize();
    }
}
