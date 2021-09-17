using System;
using EtherealC.Core.Model;

namespace EtherealC.Core.Delegates
{
    public delegate void OnExceptionDelegate(RPCException exception);
    public delegate void OnLogDelegate(RPCLog log);
}
