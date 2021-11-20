using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Param : System.Attribute
    {
        public string Name{ get; set; }
    }
}
