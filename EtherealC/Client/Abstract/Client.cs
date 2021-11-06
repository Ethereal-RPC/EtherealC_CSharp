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
        public delegate void ConnectSuccessDelegate(Client client);
        /// <summary>
        /// ����ί��
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectFailDelegate(Client client);
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
        public event ConnectSuccessDelegate ConnectEvent;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        public event ConnectFailDelegate ConnectFailEvent;
        /// <summary>
        /// �Ͽ������¼�
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --�ֶ�--
        protected Net.Abstract.Net net;
        protected ClientConfig config;
        protected string prefixes;
        #endregion

        #region --����--
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
        /// ����ʱ���������¼�
        /// </summary>
        protected void OnConnectSuccess()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// ����ʱ���������¼�
        /// </summary>
        protected void OnConnnectFail()
        {
            ConnectFailEvent?.Invoke(this);
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
