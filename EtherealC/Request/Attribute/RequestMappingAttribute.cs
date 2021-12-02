using System;

namespace EtherealC.Request.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestMappingAttribute : System.Attribute
    {
        [Flags]
        public enum InvokeTypeFlags
        {
            Local = 0x1,
            Remote = 0x2,
            ReturnLocal = 0x10,
            ReturnRemote = 0x20
        }

        private InvokeTypeFlags invokeType = InvokeTypeFlags.Remote | InvokeTypeFlags.ReturnRemote;
        private int timeout = -1;
        private string mapping = null;

        public InvokeTypeFlags InvokeType { get => invokeType; set => invokeType = value; }
        public int Timeout { get => timeout; set => timeout = value; }
        public string Mapping { get => mapping; set => mapping = value; }

        public RequestMappingAttribute(string Mapping)
        {
            this.Mapping = Mapping;
        }
    }
}
