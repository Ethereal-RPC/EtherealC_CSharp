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
        public delegate void ConnectDelegate(Client client);
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
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--
        protected string netName;
        protected string serviceName;
        protected ClientConfig config;
        #endregion

        #region --属性--
        public string NetName { get => netName; set => netName = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        public ClientConfig Config { get => config; set => config = value; }
        #endregion

        public Client(string netName, string serviceName)
        {
            this.NetName = netName;
            this.ServiceName = serviceName;
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
        protected void OnConnect()
        {
            ConnectEvent?.Invoke(this);
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
