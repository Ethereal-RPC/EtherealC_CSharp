using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Manager.Ioc;

namespace EtherealC.Core.BaseCore
{
    public class MZCore : BaseCore
    {
        public IocManager IOCManager { get; set; } = new();
        public AbstractTypeManager Types { get; set; } = new();
        
    }
}
