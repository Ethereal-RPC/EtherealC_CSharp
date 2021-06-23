using EtherealC.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealC.NativeClient
{
    public class ClientConfig
    {
        #region --委托--
        public delegate string ClientRequestModelSerializeDelegate(ClientRequestModel requestModel);
        public delegate ServerRequestModel ServerRequestModelDeserializeDelegate(string json);
        public delegate ClientResponseModel ClientResponseModelDeserializeDelegate (string json);
        public delegate void OnExceptionDelegate(Exception exception,SocketClient client);

        public delegate void OnLogDelegate(RPCLog log, SocketClient client);
        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --字段--
        private int bufferSize = 1024;
        private int maxBufferSize = 10240;
        private Encoding encoding = Encoding.UTF8;
        private int dynamicAdjustBufferCount = 1;
        private ClientRequestModelSerializeDelegate clientRequestModelSerialize;
        private ServerRequestModelDeserializeDelegate serverRequestModelDeserialize;
        private ClientResponseModelDeserializeDelegate clientResponseModelDeserialize;
        #endregion

        #region --属性--
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public Encoding Encoding { get => encoding; set => encoding = value; }
        public int DynamicAdjustBufferCount { get => dynamicAdjustBufferCount; set => dynamicAdjustBufferCount = value; }
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

        internal void OnException(RPCException.ErrorCode code, string message, SocketClient client)
        {
            OnException(new RPCException(code, message),client);
        }
        internal void OnException(Exception e, SocketClient client)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e,client);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, SocketClient client)
        {
            OnLog(new RPCLog(code, message), client);
        }
        internal void OnLog(RPCLog log, SocketClient client)
        {
            if (LogEvent != null)
            {
                LogEvent(log,client);
            }
        }
        #endregion


    }
}
