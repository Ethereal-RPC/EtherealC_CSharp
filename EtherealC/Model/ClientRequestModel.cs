using System.Threading;
using Newtonsoft.Json;

namespace EtherealC.Model
{
    public class ClientRequestModel
    {
        [JsonIgnore]
        private ClientResponseModel result;
        private string jsonRpc;
        private string methodId;
        private object[] @params;
        private string id;
        private string service;
        private AutoResetEvent sign = new AutoResetEvent(false);
        public ClientResponseModel Result { get => result; set => result = value; } 
        public string JsonRpc { get => jsonRpc; set => jsonRpc = value; }
        public string MethodId { get => methodId; set => methodId = value; }
        public object[] Params { get => @params; set => @params = value; }
        public string Id { get => id; set => id = value; }
        public string Service { get => service; set => service = value; }
        public AutoResetEvent Sign { get => sign; set => sign = value; }

        public ClientRequestModel(string jsonRpc,string service,string methodId, object[] @params)
        {
            JsonRpc = jsonRpc;
            MethodId = methodId;
            Params = @params;
            Service = service;
        }

        public void Set(ClientResponseModel result)
        {
            Result = result;
            Sign.Set();
        }
        public ClientResponseModel Get(int timeout)
        {
            //暂停当前进程，等待返回.
            while (Result == null)
            {
                Sign.WaitOne(timeout);
            }
            return Result;
        }
        public override string ToString()
        {
            return "Jsonrpc:" + JsonRpc + "\n"
                + "Service:" + Service + "\n"
                + "Methodid:" + MethodId + "\n"
                + "Params:" + JsonConvert.SerializeObject(Params);
        }
    }
}
