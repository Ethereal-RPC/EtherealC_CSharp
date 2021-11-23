using EtherealC.Client.Interface;
using EtherealC.Core.BaseCore;
using EtherealC.Core.Model;

namespace EtherealC.Client.Abstract
{

    public abstract class Client : BaseCore,IClient
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
        private Request.Abstract.Request request;
        protected ClientConfig config;
        protected string prefixes;
        #endregion

        #region --属性--
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
        /// 连接时激活连接事件
        /// </summary>
        internal void OnConnectSuccess()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        internal void OnConnnectFail()
        {
            ConnectFailEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        internal void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }

    }
}
