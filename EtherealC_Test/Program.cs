using EtherealC_Test.Model;
using EtherealC_Test.ServiceDemo;
using EtherealS_Test.RequestDemo;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtherealC.Client;
using EtherealC.Client.Abstract;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Net.Abstract;
using EtherealC.Net.WebSocket;
using EtherealC.Request;
using EtherealC.Request.Abstract;
using EtherealC.Service;
using EtherealC.Service.Abstract;

namespace EtherealC_Test
{
    public class Program
    {

        public static void Single(string ip,string port,string netName)
        {   
            //注册数据类型
            AbstractTypes types = new AbstractTypes();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register(new WebSocketNet(netName));
            net.ExceptionEvent += Config_ExceptionEvent;
            net.LogEvent += Net_LogEvent;
            //向网关注册服务
            Service service = ServiceCore.Register(net, new ClientService(), "Client", types);
            //向网关注册请求
            ServerRequest request = RequestCore.Register<ServerRequest,IServerRequest>(net, "Server", types);
            request.ConnectSuccessEvent += Request_ConnectSuccessEvent;
            //注册连接
            ClientCore.Register(net, new WebSocketClient($"ethereal://{ip}:{port}/NetDemo"));
            //启动连接
            net.Publish();
        }

        private static void Net_LogEvent(TrackLog log)
        {
            Console.WriteLine($"---------------------------------\n{log.Message}\n---------------------------------\n");
        }

        private static void Request_ConnectSuccessEvent(Request request)
        {
            Console.WriteLine(((request) as ServerRequest).Add(2, 3));
        }

        private static void Config_ExceptionEvent(TrackException exception)
        {
            Console.WriteLine($"---------------------------------\n{exception.Exception.Message}\n---------------------------------\n");
            throw exception.Exception;
        }

        public static void Main()
        {
            Single("127.0.0.1", "28015","demo");
            Console.ReadKey();
        }
    }
}
