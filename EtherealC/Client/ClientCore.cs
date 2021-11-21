using EtherealC.Core.Model;
using EtherealC.Request;

namespace EtherealC.Client
{
    public class ClientCore
    {
        public static bool Get(string net_name, string request_name, out Abstract.Client client)
        {
            if (RequestCore.Get(net_name, request_name, out Request.Abstract.Request request))
            {
                client = request.Client;
                return true;
            }
            client = null;
            return false;
        }
        public static Abstract.Client Register(Request.Abstract.Request request, Abstract.Client client, bool startClient = true)
        {
            if (request.Client == null)
            {
                //当连接建立时，请求中的连接成功事件将会发生
                request.Client = client;
                client.Request = request;
                client.LogEvent += request.OnLog;
                client.ExceptionEvent += request.OnException;
                client.ConnectEvent += Client_ConnectEvent;
                if (startClient)
                {
                    client.Connect();
                }
                return request.Client;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{request.Name}已注册Client！");
        }

        private static void Client_ConnectEvent(Abstract.Client client)
        {
            client.Request.OnConnectSuccess();
        }

        public static bool UnRegister(Abstract.Client client)
        {
            client.LogEvent -= client.Request.OnLog;
            client.ExceptionEvent -= client.Request.OnException;
            client.Request.Client = null;
            client.Request = null;
            client.DisConnect();
            return true;
        }
    }
}
