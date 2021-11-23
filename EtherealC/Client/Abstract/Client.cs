using EtherealC.Client.Interface;
using EtherealC.Core.BaseCore;
using EtherealC.Core.Model;

namespace EtherealC.Client.Abstract
{

    public abstract class Client : BaseCore,IClient
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
        private Request.Abstract.Request request;
        protected ClientConfig config;
        protected string prefixes;
        #endregion

        #region --����--
        public ClientConfig Config { get => config; set => config = value; }
        public string Prefixes { get => prefixes; set => prefixes = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }

        #endregion

        public Client(string prefixes)
        {
            this.prefixes = prefixes;
        }
        public abstract void Connect();
        public abstract void DisConnect();
        internal abstract void SendClientRequestModel(ClientRequestModel request);

        /// <summary>
        /// ����ʱ���������¼�
        /// </summary>
        internal void OnConnectSuccess()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// ����ʱ���������¼�
        /// </summary>
        internal void OnConnnectFail()
        {
            ConnectFailEvent?.Invoke(this);
        }
        /// <summary>
        /// �Ͽ�����ʱ����Ͽ������¼�
        /// </summary>
        internal void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }

    }
}
