using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Core.Event.Attribute
{
    public class AfterEventContext : EventContext
    {
        public object Result { get; set; }
        public AfterEventContext(Dictionary<string, object> parameters, MethodInfo method, object result) : base(parameters, method)
        {
            Result = result;
        }
    }
    public class AfterEvent : EventSender
    {
        public AfterEvent(string function) : base(function)
        {
        }
    }
}
