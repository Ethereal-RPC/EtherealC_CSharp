using EtherealC.Model;

namespace EtherealC.RPCRequest
{
    public class RequestConfig
    {
        #region --字段--
        private bool tokenEnable = true;
        private RPCType type;
        private int timeout = -1;

        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
        public RPCType Type { get => type; set => type = value; }
        public int Timeout { get => timeout; set => timeout = value; }
        #endregion


        #region --方法--
        public RequestConfig(RPCType type)
        {
            this.type = type;
        }
        #endregion
    }
}
