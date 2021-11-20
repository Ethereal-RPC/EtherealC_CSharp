using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Event.Attribute
{
    public class ExceptionEventContext : Model.EventContext
    {
        public Exception Exception { get; set; }
        public ExceptionEventContext(Dictionary<string, object> parameters, MethodInfo method,Exception exception) : base(parameters, method)
        {
            Exception = exception;
        }
    }
    public class ExceptionEvent : EventSender
    {
        public Exception Exception { get; set; }
        public bool IsThrow { get; set; }
        public ExceptionEvent(string instance, string mapping,string paramsMapping, bool isThrow = false) : base(instance, mapping, paramsMapping)
        {
            IsThrow = isThrow;
        }
    }
}
