using System;

namespace EtherealC.Service.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : System.Attribute
    {
        private bool plugin = true;
        public bool Plugin { get => plugin; set => plugin = value; }
    }
}
