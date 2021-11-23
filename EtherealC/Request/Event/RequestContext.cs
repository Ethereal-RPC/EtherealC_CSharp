using EtherealC.Core.Manager.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Event
{
    public class RequestContext : EventContext
    {
        public RequestContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
}
