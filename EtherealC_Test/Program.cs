using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using EtherealC_Test.Model;
using EtherealC_Test.ServiceDemo;
using EtherealS_Test.RequestDemo;
using System;
using System.Collections.Generic;

namespace EtherealC_Test
{
    public class Program
    {

        public static void Single(string ip,string port,string netName)
        {
            //注册数据类型
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register(netName);
            net.ExceptionEvent += Config_ExceptionEvent;
            //向网关注册服务
            Service service = ServiceCore.Register<ClientService>(net, "Client", types);
            //向网关注册请求
            ServerRequest request = RequestCore.Register<ServerRequest>(net, "Server", types);
            //注册连接
            SocketClient client = ClientCore.Register(request, ip, port);
            client.ConnectSuccessEvent += Config_ConnectSuccessEvent;
            client.ConnectFailEvent += Client_ConnectFailEvent;
            //启动连接
            net.Publish();
        }
        /// <summary>
        /// 分布式模式Demo
        /// </summary>
        /// <param name="netName">网关名</param>
        /// <param name="ip">本地集群地址</param>
        public static void NetNode(string netName,string ip)
        {
            //注册数据类型
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register(netName);
            net.ExceptionEvent += Config_ExceptionEvent;
            //向网关注册服务
            Service service = ServiceCore.Register<ClientService>(net, "Client", types);
            //向网关注册请求
            Request request = RequestCore.Register<ServerRequest>(net, "Server", types) as Request;

            /*
             * 配置分布式
             */
            //开启分布式模式
            net.Config.NetNodeMode = true;
            //添加分布式地址
            List<Tuple<string, string, ClientConfig>> ips = new();
            ips.Add(new Tuple<string, string, ClientConfig>(ip, "28015", null));
            ips.Add(new Tuple<string, string, ClientConfig>(ip, "28016", null));
            ips.Add(new Tuple<string, string, ClientConfig>(ip, "28017", null));
            ips.Add(new Tuple<string, string, ClientConfig>(ip, "28018", null));
            net.Config.NetNodeIps = ips;
            net.Publish();
            request.ConnectSuccessEvent += Request_ConnectSuccessEvent;
        }

        private static void Request_ConnectSuccessEvent(Request request)
        {
            if (((request) as ServerRequest).Register("aa", 2))
            {
                Console.WriteLine("调用用户服务成功");
                Console.WriteLine($"调用的目标服务器地址为:{request.Client.ClientKey}");
            }
            else Console.WriteLine("返回是啊比");
        }

        private static void Client_ConnectFailEvent(SocketClient client)
        {
            Console.WriteLine("已断开连接");
        }

        private static void Config_ExceptionEvent(Exception exception, Net net)
        {
            Console.WriteLine(exception.Message);
        }

        public static void Main()
        {
            //Single("127.0.0.1", "28015","1");
            NetNode("demo", "127.0.0.1");
            Console.ReadKey();
        }

        private static void Config_ConnectSuccessEvent(SocketClient client)
        {
            Console.WriteLine("启动成功");
        }

    }
}
