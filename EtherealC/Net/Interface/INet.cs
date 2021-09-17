using EtherealC.Core.Model;

namespace EtherealC.Net.Interface
{
    public interface INet
    {
        #region --方法--
        public bool Publish();
        public void ServerRequestReceiveProcess(ServerRequestModel request);
        public void ClientResponseReceiveProcess(ClientResponseModel response);
        #endregion
    }
}
