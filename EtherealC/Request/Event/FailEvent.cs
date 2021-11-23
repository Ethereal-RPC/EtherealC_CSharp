using EtherealC.Core.Manager.Event.Attribute;
using EtherealC.Core.Model;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Event
{
    public class FailEventContext : RequestContext
    {
        public Error Error { get; set; }
        public FailEventContext(Dictionary<string, object> parameters, MethodInfo method, Error error) : base(parameters, method)
        {
            Error = error;
        }
    }
    public class FailEvent : EventSender
    {
        public FailEvent(string function) : base(function)
        {
        }
    }
}
