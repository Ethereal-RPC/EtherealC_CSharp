using EtherealC.Core.Model;
using EtherealC.RPCRequest.Abstract;

namespace EtherealC.RPCRequest.WebSocket
{
    public class WebSocketRequestConfig : RequestConfig
    {
        public WebSocketRequestConfig(RPCTypeConfig types) : base(types)
        {
        }
    }
}
