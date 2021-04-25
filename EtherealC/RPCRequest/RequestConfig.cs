using EtherealC.Model;
using EtherealS.Model;

namespace EtherealC.RPCRequest
{
    public class RequestConfig
    {
        #region --字段--
        private bool tokenEnable = true;
        private RPCTypeConfig types;
        private int timeout = -1;

        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
        public RPCTypeConfig Types { get => types; set => types = value; }
        public int Timeout { get => timeout; set => timeout = value; }
        #endregion


        #region --方法--
        public RequestConfig(RPCTypeConfig types)
        {
            this.types = types;
        }
        #endregion
    }
}
