using EtherealC.Attribute;
using EtherealC_Test.Model;
using System;

namespace EtherealS_Test.RequestDemo
{
    public class ClientService
    {
        [RPCService]
        public void Say(User sender,string message)
        {
            Console.WriteLine(sender.Username + ":" + message);
        }

    }
}
