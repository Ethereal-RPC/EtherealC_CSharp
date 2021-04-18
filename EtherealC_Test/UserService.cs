using EtherealC.Attribute;
using System;

namespace EtherealC_Test
{
    public class UserService
    {
        [RPCService]
        public void Hello(string name)
        {
            Console.WriteLine("Nihao" + name);
        }
    }
}
