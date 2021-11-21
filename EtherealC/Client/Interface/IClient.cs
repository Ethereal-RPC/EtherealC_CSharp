using EtherealC.Core.Interface;

namespace EtherealC.Client.Interface
{

    public interface IClient : ILogEvent, IExceptionEvent
    {
        public void Connect();
        public void DisConnect();
    }
}
