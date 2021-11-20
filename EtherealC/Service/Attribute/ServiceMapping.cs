using System;

namespace EtherealC.Service.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceMapping : System.Attribute
    {
        private string mapping = null;

        public string Mapping { get => mapping; set => mapping = value; }


        public ServiceMapping(string Mapping)
        {
            this.Mapping = Mapping;
        }
    }
}
