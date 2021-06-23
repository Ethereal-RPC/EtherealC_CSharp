using EtherealC.Model;
using System;

namespace EtherealC.RPCRequest
{
    public class RequestConfig
    {

        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception,Request request);
        public delegate void OnLogDelegate(RPCLog log,Request request);
        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --字段--
        private bool tokenEnable = true;
        private RPCTypeConfig types;
        private int timeout = -1;

        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
        public RPCTypeConfig Types { get => types; set => types = value; }
        public int Timeout { get => timeout; set => timeout = value; }
        #endregion


        #region --方法--
        public RequestConfig(RPCTypeConfig types)
        {
            this.types = types;
        }
        internal void OnException(RPCException.ErrorCode code, string message, Request request)
        {
            OnException(new RPCException(code, message), request);
        }
        internal void OnException(Exception e, Request request)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e, request);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, Request request)
        {
            OnLog(new RPCLog(code, message),request);
        }
        internal void OnLog(RPCLog log, Request request)
        {
            if (LogEvent != null)
            {
                LogEvent(log,request);
            }
        }
        #endregion
    }
}
