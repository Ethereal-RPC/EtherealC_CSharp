using System;
using System.Collections.Generic;
using EtherealC.Client.Abstract;
using EtherealC.Net.Interface;

namespace EtherealC.Net.Abstract
{
    public abstract class NetConfig: INetConfig
    {
        #region --字段--
        /// <summary>
        /// 服务注册心跳间隔
        /// </summary>
        private int netNodeHeartInterval = 6000;
        #endregion

        #region --属性--
        public int NetNodeHeartInterval { get => netNodeHeartInterval; set => netNodeHeartInterval = value; }

        #endregion

        #region --方法--

        #endregion
    }
}
