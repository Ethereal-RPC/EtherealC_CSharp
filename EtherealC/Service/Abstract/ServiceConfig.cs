using EtherealC.Core.Model;
using EtherealC.Service.Interface;

namespace EtherealC.Service.Abstract
{
    public abstract class ServiceConfig: IServiceConfig
    {

        #region --字段--
        protected AbstractTypes types;
        #endregion

        #region --属性--
        public AbstractTypes Types { get => types; set => types = value; }

        #endregion

        #region --方法--
        public ServiceConfig(AbstractTypes types)
        {
            this.types = types;
        }

        public ServiceConfig(AbstractTypes types, bool tokenEnable)
        {
            this.types = types;
        }

        #endregion
    }
}
