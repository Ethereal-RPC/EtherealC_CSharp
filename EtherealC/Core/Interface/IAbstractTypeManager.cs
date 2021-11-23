using EtherealC.Core.Manager.AbstractType;

namespace EtherealC.Core.Interface
{
    internal interface IAbstractTypeManager
    {
        public void RegisterAbstract<T>(string typeName);
        public void RegisterAbstract<T>(string typeName, AbstractType.SerializeDelegage serializDelegage, AbstractType.DeserializeDelegage deserializeDelegage);
        public void RegisterAbstract(AbstractType type);

    }
}
