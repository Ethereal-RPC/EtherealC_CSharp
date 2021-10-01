using EtherealC.Client.Abstract;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request;

namespace EtherealC.Client
{
    public class ClientCore
    {
        public static bool Get(string netName,string serviceName, out Abstract.Client client)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, serviceName, out client);
            }
            else
            {
                client = null;
                return false;
            }
        }
        public static bool Get(Net.Abstract.Net net,string serviceName, out Abstract.Client client)
        {
            if(RequestCore.Get(net, serviceName, out Request.Abstract.Request request))
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
        public static Abstract.Client Register(Net.Abstract.Net net, string serviceName, Abstract.Client client)
        {
            if (RequestCore.Get(net, serviceName, out Request.Abstract.Request request))
            {
                return Register(request, client);
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{serviceName} 未找到");
        }
        public static Abstract.Client Register(Request.Abstract.Request request,Abstract.Client client)
        {
            //当连接建立时，请求中的连接成功事件将会发生
            request.Client = client;
            client.ServiceName = request.Name;
            client.NetName = request.NetName;
            request.Client.LogEvent += request.OnLog;
            request.Client.ExceptionEvent += request.OnException;
            request.Client.ConnectEvent += Client_ConnectEvent;
            return request.Client;
        }

        private static void Client_ConnectEvent(Abstract.Client client)
        {
            if(RequestCore.Get(client.NetName,client.ServiceName,out Request.Abstract.Request request))
            {
                request.OnConnectSuccess();
            }
        }

        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net,serviceName);
            }
            else return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if (net != null && RequestCore.Get(net, serviceName, out Request.Abstract.Request request))
            {
                return UnRegister(request);
            }
            return true;
        }
        public static bool UnRegister(Request.Abstract.Request request)
        {
            if (request?.Client != null)
            {
                request.Client.LogEvent -= request.OnLog;
                request.Client.ExceptionEvent -= request.OnException;
                var temp = request.Client;
                request.Client = null;
                temp?.DisConnect();
            }
            return true;
        }
    }
}
