using EtherealC.Core.BaseCore;
using EtherealC.Net.Interface;
using System.Collections.Generic;

namespace EtherealC.Net.Abstract
{
    public abstract class Net : BaseCore,INet
    {
        public enum NetType { WebSocket }
        #region --事件字段--

        #endregion

        #region --事件属性--

        #endregion

        #region --字段--
        internal string name;
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected Dictionary<string, Request.Abstract.Request> requests = new Dictionary<string, Request.Abstract.Request>();
        protected NetType type = NetType.WebSocket;
        protected NetConfig config;
        #endregion

        #region --属性--
        public Dictionary<string, Request.Abstract.Request> Requests { get => requests; set => requests = value; }
        public NetType Type { get => type; set => type = value; }
        public NetConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }

        #endregion

        #region --方法--

        public Net(string name)
        {
            this.Name = name;
        }
        #endregion
    }
}
