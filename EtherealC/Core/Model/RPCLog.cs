using EtherealC.NativeClient;
using EtherealC.RPCNet;
using EtherealC.RPCRequest;
using EtherealC.RPCService;

namespace EtherealC.Core.Model
{
    public class RPCLog
    {
        public enum LogCode { Core, Runtime }

        #region --字段--
        private string message;
        private LogCode code;

        private Net net;
        private Client client;
        private Service service;
        private Request request;
        #endregion



        #region --属性--
        public string Message { get => message; set => message = value; }
        public LogCode Code { get => code; set => code = value; }
        public Net Net { get => net; set => net = value; }
        public Service Service { get => service; set => service = value; }
        public Request Request { get => request; set => request = value; }
        public Client Client { get => client; set => client = value; }
        #endregion

        public RPCLog(LogCode code,string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
