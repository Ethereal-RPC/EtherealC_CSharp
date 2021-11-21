using System;

namespace EtherealC.Core.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class TrackException : Exception
    {
        #region --字段--
        public enum ErrorCode { Core, Runtime, NotEthereal }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;
        private Net.Abstract.Net net;
        private Client.Abstract.Client client;
        private Service.Abstract.Service service;
        private Request.Abstract.Request request;
        private Exception exception;
        #endregion

        #region --属性--
        public ErrorCode Error { get => errorCode; set => errorCode = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }
        public Exception Exception { get => exception; set => exception = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        #endregion

        public TrackException(string message) : base(message)
        {
            exception = this;
        }
        public TrackException(Exception e) : base("外部库发生异常\n" + e.Message)
        {
            exception = e;
            errorCode = ErrorCode.NotEthereal;
        }

        public TrackException(ErrorCode errorCode, string message) : base(message)
        {
            exception = this;
            this.errorCode = errorCode;
        }
    }
}
