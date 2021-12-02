using EtherealC.Core.Attribute;
using System;

namespace EtherealC.Core.Manager.AbstractType
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParamAttribute : BaseParam
    {
        public string Type { get; set; }
        public ParamAttribute(string Type)
        {
            this.Type = Type;
        }
    }
}
