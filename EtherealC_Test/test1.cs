using EtherealC.Model;
using EtherealC.RPCService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EtherealC_Test
{
    public class test1
    {
        Random random = new Random();
        public bool test()
        {
            RPCType types = new RPCType();
            types.Add<string>("String");
            string ip = "127.0.0.1";
            string port = "28015";
            EtherealC.RPCNet.NetCore.Register(ip, port);
            Request request = EtherealC.RPCRequest.RequestCore.Register<Request>(ip, port, "ServerService", types);
            ServiceCore.Register(new UserService(), ip, port, "UserService", new ServiceConfig(types));
            EtherealC.NativeClient.ClientCore.Register(ip, port).Start();
            while (true)
            {

            }
        }
    }
}
