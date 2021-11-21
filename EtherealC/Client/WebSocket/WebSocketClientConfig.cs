using EtherealC.Client.Abstract;
using System;

namespace EtherealC.Client.WebSocket
{
    public class WebSocketClientConfig : ClientConfig
    {
        #region --委托--

        #endregion

        #region --字段--
        private int bufferSize = 1024;
        private int maxBufferSize = 10240;
        /// <summary>
        /// 心跳周期
        /// </summary>
        private TimeSpan keepAliveInterval = TimeSpan.FromSeconds(60);
        #endregion

        #region --属性--
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public TimeSpan KeepAliveInterval { get => keepAliveInterval; set => keepAliveInterval = value; }
        #endregion

        #region --方法--

        #endregion


    }
}
