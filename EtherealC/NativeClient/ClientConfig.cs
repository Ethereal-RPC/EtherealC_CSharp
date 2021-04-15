using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealC.NativeClient
{
    public class ClientConfig
    {
        #region --字段--
        private int bufferSize = 1024;
        private int maxBufferSize = 10240;
        private Encoding encoding = Encoding.UTF8;
        private int dynamicAdjustBufferCount = 1;
        #endregion

        #region --属性--
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public Encoding Encoding { get => encoding; set => encoding = value; }
        public int DynamicAdjustBufferCount { get => dynamicAdjustBufferCount; set => dynamicAdjustBufferCount = value; }
        #endregion
    }
}
