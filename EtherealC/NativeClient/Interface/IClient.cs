using EtherealC.Core.Interface;
using EtherealC.Core.Model;

namespace EtherealC.NativeClient
{

    public interface IClient:ILogEvent,IExceptionEvent
    {
        public void Connect();
        public void DisConnect();
        public void SendClientRequestModel(ClientRequestModel request);
    }
}
