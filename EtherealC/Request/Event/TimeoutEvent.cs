using EtherealC.Core.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

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
        public TimeoutEvent(string function) : base(function)
        {

        }
    }
}
