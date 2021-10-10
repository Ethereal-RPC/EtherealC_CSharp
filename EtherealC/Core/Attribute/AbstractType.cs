using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Core.Attribute
{
    /// <summary>
     /// 作为服务器服务方法的标注类
     /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class AbstractType:System.Attribute
    {
        private string abstractName;

        public string AbstractName { get => abstractName; set => abstractName = value; }
    }
}
