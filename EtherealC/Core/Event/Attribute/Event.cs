using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Event.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Event : System.Attribute
    {
        public Event(string Mapping)
        {
            this.Mapping = Mapping;
        }
        public string Mapping { get; set; }
    }
}
