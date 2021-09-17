using EtherealC.Core.Interface;
using EtherealC.Core.Model;

namespace EtherealC.RPCRequest.Interface
{
    public interface IRequest : ILogEvent,IExceptionEvent
    {
        #region --方法--
        public bool GetTask(int id, out ClientRequestModel model);
        #endregion
    }
}
        