using EtherealC.Model;

namespace EtherealC.RPCNet
{
    public class NetConfig
    {
        #region --委托--
        public delegate void ServerRequestReceiveDelegate(string ip, string port,NetConfig config, ServerRequestModel request);
        public delegate void ClientResponseReceiveDelegate(string ip, string port,NetConfig config, ClientResponseModel respond);
        public delegate void CiientRequestSendDelegate(ClientRequestModel request);
        #endregion

        #region --字段--
        private ServerRequestReceiveDelegate serverRequestReceive;
        private ClientResponseReceiveDelegate clientResponseReceive;
        private CiientRequestSendDelegate clientRequestSend;
        #endregion

        #region --属性--
        public ServerRequestReceiveDelegate ServerRequestReceive { get => serverRequestReceive; set => serverRequestReceive = value; }
        public ClientResponseReceiveDelegate ClientResponseReceive { get => clientResponseReceive; set => clientResponseReceive = value; }
        public CiientRequestSendDelegate ClientRequestSend { get => clientRequestSend; set => clientRequestSend = value; }

        #endregion

        #region --方法--


        #endregion
    }
}
