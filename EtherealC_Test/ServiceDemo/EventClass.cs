using EtherealC.Core.Attribute;
using EtherealC.Core.EventManage.Attribute;
using EtherealC.Core.EventManage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC_Test.ServiceDemo
{
    internal class EventClass
    {
        [Event("after")]
        public void After(int ddd,[EventContextParam] EventContext context,string s)
        {
            Console.WriteLine(ddd);
            Console.WriteLine(s);
            Console.WriteLine("结果值是" + ((AfterEventContext)context).Result);
        }
    }
}
