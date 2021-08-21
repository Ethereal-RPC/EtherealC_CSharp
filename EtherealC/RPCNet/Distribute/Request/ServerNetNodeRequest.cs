using EtherealC.RPCNet.Distribute.Model;

namespace EtherealC.RPCNet.Distribute.Request
{
    public interface ServerNetNodeRequest
    {
        [Attribute.RPCRequest]
        public NetNode GetNetNode(string servicename);


    }
}
