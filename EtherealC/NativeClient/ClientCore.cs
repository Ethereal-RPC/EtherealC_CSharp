using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using EtherealC.Core.Model;
using EtherealC.NativeClient.Abstract;
using EtherealC.NativeClient.WebSocket;
using EtherealC.RPCNet;
using EtherealC.RPCNet.Abstract;
using EtherealC.RPCRequest;
using EtherealC.RPCRequest.Abstract;
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
                return client != null;
            }
            else
            {
                client = null;
                return false;
            }
        }

        public static Client Register(Net net,string serviceName, string prefixes)
        {
            if(net.NetType == Core.Enums.NetType.WebSocket)
            {
                return Register(net, serviceName, prefixes, new WebSocketClientConfig());
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.NetType}的Client-Register处理");
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
            return Register(request, prefixes,null);
        }
        public static Client Register(object request, string prefixes, ClientConfig config)
        {
            if (request is not Request) throw new RPCException(RPCException.ErrorCode.Core, "ClientCore执行Register函数时request参数非Request类型");
            //已经有连接了，禁止重复注册
            Request _request = request as Request;
            if (_request.Client != null) return _request.Client;
            if(NetCore.Get(_request.NetName,out Net net))
            {
                if(net.NetType == Core.Enums.NetType.WebSocket)
                {
                    if(config == null)
                    {
                        config = new WebSocketClientConfig();
                    }
                    _request.Client = new WebSocketClient(_request.NetName, _request.Name, prefixes, config);
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.NetType}的Client-Register处理");
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"找不到Net:{_request.NetName}");
            //当连接建立时，请求中的连接成功事件将会发生
            _request.Client.LogEvent += _request.OnLog;
            _request.Client.ExceptionEvent += _request.OnException;
            _request.Client.ConnectEvent += Client_ConnectEvent;
            return _request.Client;
        }

        private static void Client_ConnectEvent(Client client)
        {
            if(RequestCore.Get(client.NetName,client.ServiceName,out Request request))
            {
                request.OnConnectSuccess();
            }
        }

        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net,serviceName);
            }
            else return true;
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            if (net != null && RequestCore.Get(net, serviceName, out Request request))
            {
                return UnRegister(request);
            }
            return true;
        }
        public static bool UnRegister(Request request)
        {
            if (request?.Client != null)
            {
                request.Client.LogEvent -= request.OnLog;
                request.Client.ExceptionEvent -= request.OnException;
                var temp = request.Client;
                request.Client = null;
                temp.DisConnect();
            }
            return true;
        }
    }
}
