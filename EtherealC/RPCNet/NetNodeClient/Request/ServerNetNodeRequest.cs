using EtherealC.RPCNet.NetNodeClient.Model;

namespace EtherealC.RPCNet.NetNodeClient.Request
{
    public interface ServerNetNodeRequest
    {
        [Attribute.RPCRequest]
        public NetNode GetNetNode(string servicename);


    }
}
