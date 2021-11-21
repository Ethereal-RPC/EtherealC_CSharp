using EtherealC.Core.EventManage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Request.Event
{
    public class RequestContext : EventContext
    {
        public RequestContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {
            
        }
    }
}
