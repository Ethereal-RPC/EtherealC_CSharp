using EtherealC.Core.Attribute;
using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Manager.Event.Attribute;
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
            Types.Add<int>("Int1");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            Types.Add<User>("User");
        }
        public override void Initialize()
        {
            object instance = new EventClass();
            IOCManager.Register("instance", instance);
        }

        public override void Register()
        {

        }

        [RequestMapping(Mapping: "SendSay", InvokeType = RequestMapping.InvokeTypeFlags.Remote)]
        public virtual bool SendSay(long listener_id,string message)
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
        [RequestMapping(Mapping: "test", InvokeType = RequestMapping.InvokeTypeFlags.Local | RequestMapping.InvokeTypeFlags.Remote | RequestMapping.InvokeTypeFlags.ReturnRemote)]
        public virtual bool Test([Param("Int1")]int d, string s,int k)
        {
            Console.WriteLine("调用了Test");
            return true;
        }


        public override void UnInitialize()
        {

        }

        public override void UnRegister()
        {

        }
    }
}
