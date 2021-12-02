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
            Name = "Server";
            Types.Add<int>("Int");
            Types.Add<int>("Int1");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            Types.Add<User>("User");
        }
        [RequestMappingAttribute(Mapping: "SendSay", InvokeType = RequestMappingAttribute.InvokeTypeFlags.Remote)]
        public virtual bool SendSay(long listener_id,string message)
        {
            Console.WriteLine("Add");
            return false;
        }
        [RequestMappingAttribute(Mapping: "SendSay1", InvokeType = RequestMappingAttribute.InvokeTypeFlags.Remote)]
        public virtual bool SendSay(string listener_id, string message)
        {
            Console.WriteLine("Add");
            return false;
        }

        [AfterEvent("instance.after(ddd:d,s:s)")]
        [RequestMappingAttribute(Mapping: "test", InvokeType = RequestMappingAttribute.InvokeTypeFlags.Local | RequestMappingAttribute.InvokeTypeFlags.Remote | RequestMappingAttribute.InvokeTypeFlags.ReturnRemote)]
        public virtual bool Test([ParamAttribute("Int1")]int d, string s,int k)
        {
            Console.WriteLine("调用了Test");
            return true;
        }

        protected override void Initialize()
        {

        }

        protected override void Register()
        {
            object instance = new EventClass();
            IOCManager.Register("instance", instance);
        }

        protected override void UnInitialize()
        {

        }

        protected override void UnRegister()
        {

        }
    }
}
