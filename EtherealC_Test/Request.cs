using EtherealC.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealC_Test
{
    public interface Request
    {
        [RPCRequest]
        public string Hello(string str);
    }
}
