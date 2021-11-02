using System;

namespace EtherealC.Request.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestMethod : System.Attribute
    {
        [Flags]
        public enum InvokeTypeFlags
        {
            Local = 0x1,
            Remote = 0x2,
            Fail = 0x4,
            Success = 0x8,
            ReturnLocal = 0x10,
            ReturnRemote = 0x20,
            Timeout = 0x40,
            All = 0x80,
        }

        private InvokeTypeFlags invokeType = InvokeTypeFlags.Remote | InvokeTypeFlags.ReturnRemote;
        private int timeout = -1;


        public InvokeTypeFlags InvokeType { get => invokeType; set => invokeType = value; }
        public int Timeout { get => timeout; set => timeout = value; }
    }
}
