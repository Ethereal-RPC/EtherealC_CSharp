using EtherealC.Net.NetNodeClient.Model;

namespace EtherealC.Net.NetNodeClient.Request
{
    public interface IServerNetNodeRequest
    {
        [EtherealC.Request.Attribute.Request]
        public NetNode GetNetNode(string servicename);


    }
}
