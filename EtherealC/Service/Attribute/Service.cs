using System;

namespace EtherealC.Service.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Service : System.Attribute
    {
        private bool tokenEnable = true;

        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
    }
}
