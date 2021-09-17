using EtherealC_Test.Model;
using System;
using EtherealC.Service.Attribute;

namespace EtherealS_Test.RequestDemo
{
    public class ClientService
    {
        [Service]
        public void Say(User sender,string message)
        {
            Console.WriteLine(sender.Username + ":" + message);
        }

    }
}
