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
        public static bool Get(string netName, out SocketClient client)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, out client);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{netName}Net未找到");
        }
        public static bool Get(Net net, out SocketClient client)
        {
            client = net.Client;
            if (net.Client != null) return true;
            else return false;
        }

        public static SocketClient Register(string ip, Net net, string port)
        {
            return Register(ip, port,net, new ClientConfig(), null);
        }
        public static SocketClient Register(string ip, Net net, string port, ClientConfig config)
        {
            return Register(ip, port,net, config, null);
        }
        public static SocketClient Register(string ip, Net net, string port, SocketClient client)
        {
            return Register(ip, port,net, new ClientConfig(), client);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static SocketClient Register(string ip, string port,Net net, ClientConfig config, SocketClient socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (net.Client == null)
            {
                if (socketserver == null) socketserver = new SocketClient(net,key, config);
                net.Client = socketserver;
            }
            return socketserver;
        }

        public static bool UnRegister(string netName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{netName}Net未找到");
        }
        public static bool UnRegister(Net net)
        {
            net.Client.Dispose();
            net.Client = null;
            net.ClientRequestSend = null;
            return true;
        }
    }
}
