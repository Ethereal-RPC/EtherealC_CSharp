using EtherealC.Client.Interface;
using EtherealC.Core;
using EtherealC.Core.Model;

namespace EtherealC.Client.Abstract
{

    public abstract class Client:IClient
    {
        #region --ί��--
        /// <summary>
        /// ����ί��
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(Client client);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(Client client);

        #endregion

        #region --�¼��ֶ�--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --�¼�����--
        /// <summary>
        /// ��־����¼�
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
        /// �׳��쳣�¼�
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
        /// �����¼�
        /// </summary>
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --�ֶ�--
        protected string netName;
        protected string serviceName;
        protected ClientConfig config;
        #endregion

        #region --����--
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
        /// ����ʱ���������¼�
        /// </summary>
        protected void OnConnect()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// �Ͽ�����ʱ����Ͽ������¼�
        /// </summary>
        protected void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }

    }
}