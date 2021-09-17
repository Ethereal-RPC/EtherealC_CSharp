using System;

namespace EtherealC.Request.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Request : System.Attribute
    {
        private string[] paramters = null;
        private int timeout = -1;
        public string[] Paramters { get => paramters; set => paramters = value; }
        public int Timeout { get => timeout; set => timeout = value; }
    }
}
