using EtherealC.Core.Interface;
using EtherealC.Core.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.RPCRequest
{
    public interface IRequest : ILogEvent,IExceptionEvent
    {
        #region --方法--
        public bool GetTask(int id, out ClientRequestModel model);
        #endregion
    }
}
        