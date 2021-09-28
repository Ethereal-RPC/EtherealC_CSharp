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
            Net net = NetCore.Register(netName, Net.NetType.WebSocket);
            net.ExceptionEvent += Config_ExceptionEvent;
            net.LogEvent += Net_LogEvent;
            //向网关注册服务
            Service service = ServiceCore.Register<ClientService>(net, "Client", types);
            //向网关注册请求
            ServerRequest request = RequestCore.Register<ServerRequest,IServerRequest>(net, "Server", types);
            (request as Request).ConnectSuccessEvent += Request_ConnectSuccessEvent;
            //注册连接
            Client client = ClientCore.Register(request, "127.0.0.1:28015/NetDemo/");
            client.ConnectEvent += Config_ConnectSuccessEvent;
            client.DisConnectEvent += Client_ConnectFailEvent;
            //启动连接
            net.Publish();

        }

        private static void Net_LogEvent(TrackLog log)
        {
            Console.WriteLine(log.Message);
        }

        /// <summary>
        /// 分布式模式Demo
        /// </summary>
        /// <param name="netName">网关名</param>
        /// <param name="ip">本地集群地址</param>
        public static void NetNode(string netName,string ip)
        {
            //注册数据类型
            AbstractTypes types = new AbstractTypes();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register(netName, Net.NetType.WebSocket);
            net.ExceptionEvent += Config_ExceptionEvent;
            //向网关注册服务
            Service service = ServiceCore.Register<ClientService>(net, "Client", types);
            //向网关注册请求
            Request request = RequestCore.Register<ServerRequest,IServerRequest>(net, "Server", types) as Request;

            /*
             * 配置分布式
             */
            //开启分布式模式
            net.Config.NetNodeMode = true;
            //添加分布式地址
            List<Tuple<string,ClientConfig>> ips = new();
            ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28015}/NetDemo/", new WebSocketClientConfig()));
            ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28016}/NetDemo/", new WebSocketClientConfig()));
            ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28017}/NetDemo/", new WebSocketClientConfig()));
            ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28018}/NetDemo/", new WebSocketClientConfig()));
            net.Config.NetNodeIps = ips;
            request.ConnectSuccessEvent += Request_ConnectSuccessEvent;
            net.Publish();
            Console.Read();
        }

        private static void Request_ConnectSuccessEvent(Request request)
        {
            Console.WriteLine(((request) as ServerRequest).Add(2, 3));
        }

        private static void Client_ConnectFailEvent(Client client)
        {
            ClientCore.UnRegister(client.NetName, client.ServiceName);
        }

        private static void Config_ExceptionEvent(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        public static void Main()
        {
            Single("127.0.0.1", "28015","1");
            //NetNode("demo", "127.0.0.1");
            Console.ReadKey();
        }

        private static void Config_ConnectSuccessEvent(Client client)
        {
            Console.WriteLine("启动成功");
        }

    }
}
