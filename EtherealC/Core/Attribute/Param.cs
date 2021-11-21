using System;

namespace EtherealC.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Param : System.Attribute
    {
        public string Name { get; set; }
    }
}
