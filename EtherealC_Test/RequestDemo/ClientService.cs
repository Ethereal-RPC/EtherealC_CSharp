using EtherealC_Test.Model;
using System;
using EtherealC.Core.Model;
using EtherealC.Service.Attribute;
using EtherealC.Service.WebSocket;

namespace EtherealS_Test.RequestDemo
{
    public class ClientService:WebSocketService
    {
        public ClientService()
        {
            Types.Add<int>("Int");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            Types.Add<User>("User");
        }
        [ServiceMapping(Mapping:"Say")]
        public void Say(User sender,string message)
        {
            Console.WriteLine(sender.Username + ":" + message);
        }
        public override void Initialize()
        {

        }

        public override void UnInitialize()
        {

        }
    }
}
