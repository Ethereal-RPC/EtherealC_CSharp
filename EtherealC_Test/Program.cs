﻿using EtherealC.Client;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Net.Abstract;
using EtherealC.Net.WebSocket;
using EtherealC.Request;
using EtherealC.Request.Abstract;
using EtherealC.Service;
using EtherealC.Service.Abstract;
using EtherealC_Test.Model;
using EtherealS_Test.RequestDemo;
using System;

namespace EtherealC_Test
{
    public class Program
    {
        public static void Single(string ip, string port, string netName)
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
            //向网关注册请求
            ServerRequest request = RequestCore.Register<ServerRequest>(net, "Server");
            //向网关注册服务
            Service service = ServiceCore.Register(request, new ClientService(), "Client");
            request.ConnectSuccessEvent += Request_ConnectSuccessEvent;
            //注册连接
            ClientCore.Register(request, new WebSocketClient($"ethereal://{ip}:{port}/NetDemo"));
        }

        private static void Net_LogEvent(TrackLog log)
        {
            Console.WriteLine($"---------------------------------\n{log.Message}\n---------------------------------\n");
        }

        private static void Request_ConnectSuccessEvent(Request request)
        {
            (request as ServerRequest).test(4, "你好");
        }

        private static void Config_ExceptionEvent(TrackException exception)
        {
            Console.WriteLine($"---------------------------------\n{exception.Exception.Message}\n{exception.Exception.StackTrace}---------------------------------\n");
            throw exception.Exception;
        }

        public static void Main()
        {
            Single("127.0.0.1", "28015", "demo");
            Console.ReadKey();
        }
    }
}
