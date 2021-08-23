using EtherealC.RPCNet.Client.Model;

namespace EtherealC.RPCNet.Client.Request
{
    public interface ServerNetNodeRequest
    {
        [Attribute.RPCRequest]
        public NetNode GetNetNode(string servicename);


    }
}
