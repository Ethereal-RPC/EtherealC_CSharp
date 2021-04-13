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
            Request request = EtherealC.RPCRequest.RequestCore.Register<Request>("ServerService", ip, port, types);
            EtherealC.NativeClient.ClientCore.Register(ip, port).Start();
            Console.WriteLine(request.Hello("小妹" + "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小" +
                "妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹" +
                "小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹小妹"));
            for (int i = 0; i <= 10; i++)
            {
                Console.WriteLine(request.Hello("小妹") + $"{i}");
            }
            return true;
        }
    }
}
