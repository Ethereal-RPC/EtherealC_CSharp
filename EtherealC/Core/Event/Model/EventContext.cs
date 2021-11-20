using EtherealC.Core.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Event.Model
{
    public class EventContext
    {
        public EventContext(Dictionary<string, object> parameters, MethodInfo method)
        {
            Method = method;
            Parameters = parameters;
        }

        public MethodInfo Method { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
