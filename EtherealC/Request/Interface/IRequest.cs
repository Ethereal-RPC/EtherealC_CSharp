using EtherealC.Core.Interface;

namespace EtherealC.Request.Interface
{
    public interface IRequest : ILogEvent, IExceptionEvent
    {
        #region --方法--
        public void Initialize();
        public void UnInitialize();
        #endregion
    }
}
