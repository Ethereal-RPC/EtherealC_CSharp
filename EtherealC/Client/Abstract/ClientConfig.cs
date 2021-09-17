using System.Text;
using EtherealC.Core.Model;
using Newtonsoft.Json;

namespace EtherealC.Client.Abstract
{
    public abstract class ClientConfig
    {
        #region --委托--
        public delegate string ClientRequestModelSerializeDelegate(ClientRequestModel requestModel);
        public delegate ServerRequestModel ServerRequestModelDeserializeDelegate(string json);
        public delegate ClientResponseModel ClientResponseModelDeserializeDelegate (string json);
        #endregion

        #region --字段--
        private Encoding encoding = Encoding.UTF8;
        private ClientRequestModelSerializeDelegate clientRequestModelSerialize;
        private ServerRequestModelDeserializeDelegate serverRequestModelDeserialize;
        private ClientResponseModelDeserializeDelegate clientResponseModelDeserialize;
        #endregion

        #region --属性--
        public Encoding Encoding { get => encoding; set => encoding = value; }
        public ClientRequestModelSerializeDelegate ClientRequestModelSerialize { get => clientRequestModelSerialize; set => clientRequestModelSerialize = value; }
        public ServerRequestModelDeserializeDelegate ServerRequestModelDeserialize { get => serverRequestModelDeserialize; set => serverRequestModelDeserialize = value; }
        public ClientResponseModelDeserializeDelegate ClientResponseModelDeserialize { get => clientResponseModelDeserialize; set => clientResponseModelDeserialize = value; }
        #endregion

        #region --方法--
        public ClientConfig()
        {
            clientRequestModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            serverRequestModelDeserialize = (obj) => JsonConvert.DeserializeObject<ServerRequestModel>(obj);
            clientResponseModelDeserialize = (obj) => JsonConvert.DeserializeObject<ClientResponseModel>(obj);
        }


        #endregion


    }
}
