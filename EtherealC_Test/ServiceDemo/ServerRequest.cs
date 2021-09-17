using EtherealC.Request.Attribute;
using EtherealC_Test.Model;

namespace EtherealC_Test.ServiceDemo
{
    public interface ServerRequest
    {
        [Request]
        public bool Register(string username,long id);

        [Request]
        public bool SendSay(long listener_id, string message);
        //参数级 方法级 服务级
        [Request]
        public int Add(int a, int b);
    }
}
