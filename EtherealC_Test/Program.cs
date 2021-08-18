using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using EtherealC_Test.Model;
using EtherealC_Test.ServiceDemo;
using EtherealS_Test.RequestDemo;
using System;

namespace EtherealC_Test
{
    public class Program
    {
        public static void Main()
        {
            //注册数据类型
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<int>("int");
            types.Add<User>("user");
            types.Add<long>("long");
            types.Add<string>("string");
            types.Add<bool>("bool");

            //建立网关
            Net net = NetCore.Register("demo");
            //向网关注册服务
            Service service = ServiceCore.Register<ClientService>(net, "Client", types);
            //向网关注册请求
            ServerRequest request = RequestCore.Register<ServerRequest>(net, "Server", types);
            //向网关注册连接(提供一个生成User的方法)
            SocketClient client = ClientCore.Register(net, "192.168.0.112", "28015");
            //启动连接
            client.Start();
            request.Register("CSharpClient", 0);
            //经目标网络发送至目标客户端消息
            if (request.SendSay(1, "Hello C#"))
            {
                Console.WriteLine("消息发送成功");
            }
            else
            {
                Console.WriteLine("消息发送失败");
            }
        }
    }
}
