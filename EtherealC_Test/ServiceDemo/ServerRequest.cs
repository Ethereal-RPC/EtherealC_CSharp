using EtherealC.Core.Event.Attribute;
using EtherealC.Request.Attribute;
using EtherealC.Request.WebSocket;
using EtherealC_Test.Model;
using EtherealC_Test.ServiceDemo;
using System;

namespace EtherealS_Test.RequestDemo
{
    public class ServerRequest : WebSocketRequest
    {
        public ServerRequest()
        {
            Types.Add<int>("Int");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            Types.Add<User>("User");
        }
        public override void Initialize()
        {
            object instance = new EventClass();
            RegisterIoc("instance", instance);
        }

        [RequestMapping(Mapping: "SendSay", InvokeType = RequestMapping.InvokeTypeFlags.Remote)]
        public virtual bool SendSay(long listener_id, string message)
        {
            Console.WriteLine("Add");
            return false;
        }
        [RequestMapping(Mapping: "SendSay1", InvokeType = RequestMapping.InvokeTypeFlags.Remote)]
        public virtual bool SendSay(string listener_id, string message)
        {
            Console.WriteLine("Add");
            return false;
        }
        [AfterEvent("instance.after(ddd:d,s:s)")]
        [RequestMapping(Mapping: "test", InvokeType = RequestMapping.InvokeTypeFlags.Local | RequestMapping.InvokeTypeFlags.ReturnLocal)]
        public virtual bool test(int d, string s)
        {
            Console.WriteLine("Add");
            return false;
        }


        public override void UnInitialize()
        {

        }
    }
}
