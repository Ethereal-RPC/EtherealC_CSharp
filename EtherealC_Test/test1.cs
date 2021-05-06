﻿using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCService;
using System;

namespace EtherealC_Test
{
    public class test1
    {
        Random random = new Random();
        public bool test()
        {
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<string>("String");
            string ip = "127.0.0.1";
            string port = "28014";
            EtherealC.RPCNet.NetCore.Register(ip, port);
            Request request = EtherealC.RPCRequest.RequestCore.Register<Request>(ip, port, "ServerService", types);
            ServiceCore.Register(new UserService(), ip, port, "UserService", new ServiceConfig(types));
            ClientConfig config = new ClientConfig();
            ClientCore.Register(ip, port,config).Start();
            Console.WriteLine(request.Hello("s"));
            return true;
        }
    }
}