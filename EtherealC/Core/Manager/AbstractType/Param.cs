using EtherealC.Core.Attribute;
using System;

namespace EtherealC.Core.Manager.AbstractType
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Param : BaseParam
    {
        public string Type { get; set; }
        public Param(string Type)
        {
            this.Type = Type;
        }
    }
}
