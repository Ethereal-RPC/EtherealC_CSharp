using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Manager.IOC;

namespace EtherealC.Core.BaseCore
{
    public class MZCore : BaseCore
    {
        public IOCManager IOCManager { get; set; } = new();
        public AbstractTypeManager Types { get; set; } = new();
        
    }
}
