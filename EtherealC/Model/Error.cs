namespace EtherealC.Model
{
    public class Error
    {
        public enum ErrorCode { Intercepted, NotFoundService, NotFoundMethod }
        public int Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }   

    }
}
