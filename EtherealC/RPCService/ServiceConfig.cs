using EtherealC.Model;
using EtherealS.Model;

namespace EtherealC.RPCService
{
    public class ServiceConfig
    {

        #region --字段--
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }

        #endregion

        #region --方法--
        public ServiceConfig(RPCTypeConfig types)
        {
            this.types = types;
        }

        public ServiceConfig(RPCTypeConfig types, bool tokenEnable)
        {
            this.types = types;
        }

        #endregion
    }
}
