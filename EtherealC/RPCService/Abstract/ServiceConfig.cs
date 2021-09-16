using EtherealC.Core.Model;
using System;

namespace EtherealC.RPCService
{
    public abstract class ServiceConfig: IServiceConfig
    {

        #region --字段--
        protected RPCTypeConfig types;
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
