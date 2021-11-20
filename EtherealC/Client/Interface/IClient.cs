using EtherealC.Core.Interface;
using EtherealC.Core.Model;

namespace EtherealC.Client.Interface
{

    public interface IClient:ILogEvent,IExceptionEvent
    {
        public void Connect();
        public void DisConnect();
    }
}
