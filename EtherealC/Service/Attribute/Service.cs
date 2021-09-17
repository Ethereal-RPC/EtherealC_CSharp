using System;

namespace EtherealC.Service.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Service : System.Attribute
    {
        private string[] paramters = null;
        private bool tokenEnable = true;

        public string[] Paramters { get => paramters; set => paramters = value; }
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
    }
}
