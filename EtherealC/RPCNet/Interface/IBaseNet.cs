using EtherealC.Core.Model;

namespace EtherealC.RPCNet.Interface
{
    public interface IBaseNet
    {
        #region --方法--
        public bool Publish();
        public void ServerRequestReceiveProcess(ServerRequestModel request);
        public void ClientResponseReceiveProcess(ClientResponseModel response);
        #endregion
    }
}
