using EtherealC.Service.Attribute;
using EtherealC.Service.WebSocket;
using EtherealC_Test.Model;
using System;

namespace EtherealS_Test.RequestDemo
{
    public class ClientService : WebSocketService
    {
        public ClientService()
        {
            Name = "Client";
            Types.Add<int>("Int");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            Types.Add<User>("User");
        }
        [ServiceMapping(Mapping: "Say")]
        public void Say(User sender,string message)
        {
            Console.WriteLine(sender.Username + ":" + message);
        }
        protected override void Initialize()
        {

        }

        protected override void Register()
        {

        }

        protected override void UnInitialize()
        {

        }

        protected override void UnRegister()
        {

        }
    }
}
