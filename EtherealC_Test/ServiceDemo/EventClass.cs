using EtherealC.Core.Event.Attribute;
using System;

namespace EtherealC_Test.ServiceDemo
{
    internal class EventClass
    {
        [Event("after")]
        public void After(int ddd, [EventContextParam] EventContext context, string s)
        {
            Console.WriteLine(ddd);
            Console.WriteLine(s);
            Console.WriteLine("结果值是" + ((AfterEventContext)context).Result);
        }
    }
}
