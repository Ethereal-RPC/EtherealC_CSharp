using EtherealC.Model;

namespace EtherealC.RPCService
{
    public class ServiceConfig
    {

        #region --字段--
        private RPCType type;
        #endregion

        #region --属性--
        public RPCType Type { get => type; set => type = value; }

        #endregion

        #region --方法--
        public ServiceConfig(RPCType type)
        {
            this.type = type;
        }

        public ServiceConfig(RPCType type, bool tokenEnable)
        {
            this.type = type;
        }

        #endregion
    }
}
