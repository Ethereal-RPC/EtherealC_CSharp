using EtherealC.Core.Event.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Request.Event
{
    public class TimeoutEventContext : RequestContext
    {
        public TimeoutEventContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
    public class TimeoutEvent : EventSender
    {
        public TimeoutEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {

        }
    }
}
