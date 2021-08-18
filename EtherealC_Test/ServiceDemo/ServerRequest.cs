using EtherealC.Attribute;
using EtherealC_Test.Model;

namespace EtherealC_Test.ServiceDemo
{
    public interface ServerRequest
    {
        [RPCRequest]
        public bool Register(string username,long id);

        [RPCRequest]
        public bool SendSay(long listener_id, string message);

        [RPCRequest]
        public int Add(int a, int b);
    }
}
