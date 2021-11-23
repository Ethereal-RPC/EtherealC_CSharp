using EtherealC.Core.Manager.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Service.Event
{
    public class ServiceContext : EventContext
    {
        public ServiceContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
}
