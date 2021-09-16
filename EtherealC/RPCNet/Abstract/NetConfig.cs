using EtherealC.NativeClient;
using System;
using System.Collections.Generic;

namespace EtherealC.RPCNet
{
    public abstract class NetConfig: IBaseNetConfig
    {
        #region --字段--
        /// <summary>
        /// 分布式模式是否开启
        /// </summary>
        private bool netNodeMode = false;
        /// <summary>
        /// 分布式IP组
        /// </summary>
        private List<Tuple<string, ClientConfig>> netNodeIps;
        /// <summary>
        /// 服务注册心跳间隔
        /// </summary>
        private int netNodeHeartInterval = 6000;
        #endregion

        #region --属性--
        public bool NetNodeMode { get => netNodeMode; set => netNodeMode = value; }
        public List<Tuple<string, ClientConfig>> NetNodeIps { get => netNodeIps; set => netNodeIps = value; }
        public int NetNodeHeartInterval { get => netNodeHeartInterval; set => netNodeHeartInterval = value; }

        #endregion

        #region --方法--

        #endregion
    }
}
