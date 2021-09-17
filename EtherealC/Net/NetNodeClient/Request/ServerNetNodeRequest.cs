using EtherealC.Net.NetNodeClient.Model;

namespace EtherealC.Net.NetNodeClient.Request
{
    public interface ServerNetNodeRequest
    {
        [EtherealC.Request.Attribute.Request]
        public NetNode GetNetNode(string servicename);


    }
}
