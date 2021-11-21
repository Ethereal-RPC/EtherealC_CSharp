namespace EtherealC.Core.Model
{
    public class Error
    {
        public enum ErrorCode { Intercepted, NotFoundService, NotFoundMethod, Common }
        public ErrorCode Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }

    }
}
