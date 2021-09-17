using EtherealC.Core.Model;

namespace EtherealC.Core.Interface
{
    public interface ILogEvent
    {
        public void OnLog(TrackLog.LogCode code, string message);
        public void OnLog(TrackLog log);
    }
}
