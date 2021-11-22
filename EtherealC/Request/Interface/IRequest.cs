using EtherealC.Core.Interface;

namespace EtherealC.Request.Interface
{
    public interface IRequest : ILogEvent, IExceptionEvent
    {
        #region --方法--
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
        #endregion
    }
}
