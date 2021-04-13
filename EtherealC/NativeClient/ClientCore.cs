using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using EtherealC.Model;
using EtherealC.RPCNet;

namespace EtherealC.NativeClient
{
    public class ClientCore
    {
        private static Dictionary<Tuple<string, string>, SocketClient> SocketClients { get; } = new Dictionary<Tuple<string, string>, SocketClient>();

        public static SocketClient Register(string ip, string port)
        {
            return Register(ip, port, new ClientConfig(), null);
        }
        public static SocketClient Register(string ip, string port, ClientConfig config)
        {
            return Register(ip, port, config, null);
        }
        public static SocketClient Register(string ip, string port, SocketClient client)
        {
            return Register(ip, port, new ClientConfig(), client);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static SocketClient Register(string ip, string port, ClientConfig config, SocketClient socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (!SocketClients.TryGetValue(key, out socketserver))
            {
                if (socketserver == null) socketserver = new SocketClient(key, config);
                SocketClients[key] = socketserver;
            }
            return socketserver;
        }
        public static SocketClient Get(string ip, string port)
        {
            return Get(new Tuple<string, string>(ip, port));
        }


        public static SocketClient Get(Tuple<string, string> key)
        {
            SocketClients.TryGetValue(key, out SocketClient socketserver);
            return socketserver;
        }
    }
}
