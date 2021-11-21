using EtherealC.Core.EventManage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Service.Event
{
    public class ServiceContext : EventContext
    {
        public ServiceContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
}
