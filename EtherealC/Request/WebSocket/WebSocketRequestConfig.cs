using EtherealC.Core.Model;
using EtherealC.Request.Abstract;

namespace EtherealC.Request.WebSocket
{
    public class WebSocketRequestConfig : RequestConfig
    {
        public WebSocketRequestConfig(RPCTypeConfig types) : base(types)
        {
        }
    }
}
