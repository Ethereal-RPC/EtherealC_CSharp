using EtherealC.Core.Model;
using System;

namespace EtherealC.Core.Interface
{
    public interface IExceptionEvent
    {
        public void OnException(RPCException.ErrorCode code, string message);
        public void OnException(RPCException e);
    }
}
