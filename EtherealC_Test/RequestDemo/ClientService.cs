using EtherealC_Test.Model;
using System;
using EtherealC.Service.Attribute;
using EtherealC.Service.WebSocket;

namespace EtherealS_Test.RequestDemo
{
    public class ClientService:WebSocketService
    {
        [Service]
        public void Say(User sender,string message)
        {
            Console.WriteLine(sender.Username + ":" + message);
        }

    }
}
