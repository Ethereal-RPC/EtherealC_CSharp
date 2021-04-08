using EtherealC.Model;
using EtherealC.RPCRequest;
using EtherealC.RPCService;

namespace EtherealC.RPCNet
{
    public class NetConfig
    {
        #region --委托--
        public delegate void ServerRequestReceiveDelegate(string ip, string port,NetConfig config, ServerRequestModel request);
        public delegate void ClientResponseReceiveDelegate(string ip, string port,NetConfig config, ClientResponseModel respond);
        public delegate void ClientRequestSendDelegate(ClientRequestModel request);

        #endregion

        #region --字段--
        private ServerRequestReceiveDelegate serverRequestReceive = ServiceCore.ServerRequestReceive;
        private ClientResponseReceiveDelegate clientResponseReceive = RequestCore.ClientResponseProcess;
        private ClientRequestSendDelegate clientRequestSend;
        private bool exceptionThrow = false;
        #endregion

        #region --属性--
        public ServerRequestReceiveDelegate ServerRequestReceive { get => serverRequestReceive; set => serverRequestReceive = value; }
        public ClientRequestSendDelegate ClientRequestSend { get => clientRequestSend; set => clientRequestSend = value; }
        public bool ExceptionThrow { get => exceptionThrow; set => exceptionThrow = value; }
        public ClientResponseReceiveDelegate ClientResponseReceive { get => clientResponseReceive; set => clientResponseReceive = value; }
        #endregion

        #region --方法--


        #endregion
    }
}
