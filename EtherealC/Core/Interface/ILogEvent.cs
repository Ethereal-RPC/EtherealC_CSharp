using EtherealC.Core.Model;

namespace EtherealC.Core.Interface
{
    public interface ILogEvent
    {
        public void OnLog(RPCLog.LogCode code, string message);
        public void OnLog(RPCLog log);
    }
}
