using EtherealC.Core.EventManage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.EventManage.Attribute
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
        public AfterEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {

        }
    }
}
