using EtherealC.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealC_Test
{
    public class test1
    {
        public bool test()
        {
            RPCType types = new RPCType();
            types.Add<string>("String");
            string ip = "127.0.0.1";
            string port = "28015";
            EtherealC.RPCNet.NetCore.Register(ip, port);
            Request request = EtherealC.RPCRequest.RequestCore.Register<Request>(ip, port, "ServerService", types);
            EtherealC.NativeClient.ClientCore.Register(ip, port).Start();
                        for (int i = 0; i <= 10; i++)
            {
                Console.WriteLine(request.Hello("小妹") + $"{i}");
            }
            return true;
        }
    }
}
