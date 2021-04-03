using System;

namespace EtherealC.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCService : System.Attribute
    {
        private string[] paramters = null;

        public string[] Paramters { get => paramters; set => paramters = value; }
    }
}
