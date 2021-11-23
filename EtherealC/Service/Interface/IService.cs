using EtherealC.Core.Interface;

namespace EtherealC.Service.Interface
{
    public interface IService
    {
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
    }
}
