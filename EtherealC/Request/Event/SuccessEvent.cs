using EtherealC.Core.Event.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    public class SuccessEvent : EventSender
    {
        public SuccessEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {
        }
    }
}
