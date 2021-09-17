using System;
using EtherealC.Core.Model;

namespace EtherealC.Core.Delegates
{
    public delegate void OnExceptionDelegate(Exception exception);
    public delegate void OnLogDelegate(RPCLog log);
}
