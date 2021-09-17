namespace EtherealC.Core.Model
{
    public class TrackLog
    {
        public enum LogCode { Core, Runtime }

        #region --字段--
        private string message;
        private LogCode code;

        private Net.Abstract.Net net;
        private Client.Abstract.Client client;
        private Service.Abstract.Service service;
        private Request.Abstract.Request request;
        #endregion



        #region --属性--
        public string Message { get => message; set => message = value; }
        public LogCode Code { get => code; set => code = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        #endregion

        public TrackLog(LogCode code,string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
