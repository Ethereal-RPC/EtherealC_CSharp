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
        //参数级 方法级 服务级
        [RPCRequest]
        public int Add(int a, int b);
    }
}
