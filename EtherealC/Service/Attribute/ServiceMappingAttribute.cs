using System;

namespace EtherealC.Service.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceMappingAttribute : System.Attribute
    {
        private string mapping = null;

        public string Mapping { get => mapping; set => mapping = value; }


        public ServiceMappingAttribute(string Mapping)
        {
            this.Mapping = Mapping;
        }
    }
}
