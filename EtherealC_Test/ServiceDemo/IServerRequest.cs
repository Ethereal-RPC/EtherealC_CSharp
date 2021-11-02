using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherealC.Request.Attribute;

namespace EtherealC_Test.ServiceDemo
{
    public interface IServerRequest
    {
        [RequestMethod]
        public bool Register(string username, long id);

        [RequestMethod]
        public bool SendSay(long listener_id, string message);
        //参数级 方法级 服务级
        [RequestMethod]
        public int Add(int a, int b);
    }
}
