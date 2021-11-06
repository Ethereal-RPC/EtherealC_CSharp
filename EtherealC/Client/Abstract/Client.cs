using EtherealC.Client.Interface;
using EtherealC.Core;
using EtherealC.Core.Model;

namespace EtherealC.Client.Abstract
{

    public abstract class Client:IClient
    {
        #region --委托--
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectSuccessDelegate(Client client);
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectFailDelegate(Client client);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(Client client);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }
        }
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectSuccessDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ConnectFailDelegate ConnectFailEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--
        protected Net.Abstract.Net net;
        protected ClientConfig config;
        protected string prefixes;
        #endregion

        #region --属性--
        public ClientConfig Config { get => config; set => config = value; }
        public string Prefixes { get => prefixes; set => prefixes = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        #endregion

        public Client(string prefixes)
        {
            this.prefixes = prefixes;
        }
        public abstract void Connect();
        public abstract void DisConnect();
        public abstract void SendClientRequestModel(ClientRequestModel request);
        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            e.Client = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }
        public void OnLog(TrackLog log)
        {
            log.Client = this;
            logEvent?.Invoke(log);
        }

        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        protected void OnConnectSuccess()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        protected void OnConnnectFail()
        {
            ConnectFailEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        protected void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }

    }
}
