using EtherealC.Core.Manager.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Event
{
    public class SuccessEventContext : RequestContext
    {
        public object RemoteResult { get; set; }
        public SuccessEventContext(Dictionary<string, object> parameters, MethodInfo method, object remoteResult) : base(parameters, method)
        {
            RemoteResult = remoteResult;
        }
    }
    public class SuccessEvent : EventSenderAttribute
    {
        public SuccessEvent(string function) : base(function)
        {
        }
    }
}
