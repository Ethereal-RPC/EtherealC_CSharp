using EtherealC.Client.Abstract;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request;

namespace EtherealC.Client
{
    public class ClientCore
    {
        public static bool Get(string netName, out Abstract.Client client)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                client = net.Client;
                return true;
            }
            client = null;
            return false;
        }
        public static Abstract.Client Register(Net.Abstract.Net net,Abstract.Client client)
        {
            if (net.Client == null)
            {
                //当连接建立时，请求中的连接成功事件将会发生
                net.Client = client;
                client.Net = net;
                client.LogEvent += net.OnLog;
                client.ExceptionEvent += net.OnException;
                client.ConnectEvent += Client_ConnectEvent;
                return net.Client;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}已注册Client！");
        }

        private static void Client_ConnectEvent(Abstract.Client client)
        {
            foreach(Request.Abstract.Request request in client.Net.Requests.Values)
            {
                request.OnConnectSuccess();
            }
        }

        public static bool UnRegister(Abstract.Client client)
        {
            client.LogEvent -= client.Net.OnLog;
            client.ExceptionEvent -= client.Net.OnException;
            client.Net.Client = null;
            client.DisConnect();
            return true;
        }
    }
}
