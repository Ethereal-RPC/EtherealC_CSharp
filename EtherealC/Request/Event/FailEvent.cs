using EtherealC.Core.EventManage.Attribute;
using EtherealC.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Request.Event
{
    public class FailEventContext : RequestContext
    {
        public Error Error { get; set; }
        public FailEventContext(Dictionary<string, object> parameters, MethodInfo method,Error error) : base(parameters, method)
        {
            Error = error;
        }
    }
    public class FailEvent : EventSender
    {
        public FailEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {

        }
    }
}
