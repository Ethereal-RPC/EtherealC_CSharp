using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using EtherealC.Model;
using EtherealC.RPCNet;
using EtherealC.RPCRequest;
using EtherealC.RPCService;

namespace EtherealC.NativeClient
{
    public class ClientCore
    {
        public static bool Get(string netName,string serviceName, out Client client)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, serviceName, out client);
            }
            else
            {
                client = null;
                return false;
            }
        }
        public static bool Get(Net net,string serviceName, out Client client)
        {
            if(RequestCore.Get(net, serviceName, out Request request))
            {
                client = request.Client;
                return true;
            }
            else
            {
                client = null;
                return false;
            }
        }

        public static Client Register(Net net,string serviceName, string prefixes)
        {
            return Register(net,serviceName, prefixes,new ClientConfig());
        }

        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static Client Register(Net net, string serviceName, string prefixes,ClientConfig config)
        {
            if(RequestCore.Get(net, serviceName, out Request request))
            {
                return Register(request,prefixes, config);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{net.Name}-{serviceName} 未找到");
        }
        public static Client Register(object request, string prefixes)
        {
            return Register(request, prefixes,new ClientConfig());
        }
        public static Client Register(object request, string prefixes, ClientConfig config)
        {
            if (request is not Request) throw new RPCException(RPCException.ErrorCode.Core, "ClientCore执行Register函数时request参数非Request类型");
            //已经有连接了，禁止重复注册
            Request _request = request as Request;
            if (_request.Client != null) return _request.Client;
            _request.Client = new Client(_request.NetName, _request.Name, prefixes, config);
            //当连接建立时，请求中的连接成功事件将会发生
            _request.Client.ConnectEvent += Client_ConnectSuccessEvent;
            _request.Client.LogEvent += _request.OnClientLog;
            _request.Client.ExceptionEvent += _request.OnClientException;
            return _request.Client;
        }

        private static void Client_ConnectSuccessEvent(Client client)
        {
            if(RequestCore.Get(client.NetName, client.ServiceName, out Request request))
            {
                request.OnConnectSuccess();
            }
        }

        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net net) && RequestCore.Get(net,serviceName,out Request request))
            {
                return UnRegister(net,serviceName);
            }
            else return true;
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            if (RequestCore.Get(net, serviceName, out Request request))
            {
                if(request.Client != null)
                {
                    request.Client.LogEvent -= request.OnClientLog;
                    request.Client.ExceptionEvent -= request.OnClientException;
                    request.Client.Close(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "UnRegister");
                    request.Client = null;
                }
                return true;
            }
            else return true;
        }
    }
}
