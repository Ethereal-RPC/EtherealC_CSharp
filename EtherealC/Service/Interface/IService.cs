using EtherealC.Core.Interface;

namespace EtherealC.Service.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
    }
}
