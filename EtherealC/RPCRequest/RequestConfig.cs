using EtherealC.Model;
using System;

namespace EtherealC.RPCRequest
{
    public class RequestConfig
    {

        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception);
        public delegate void OnLogDelegate(RPCLog log);
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
        internal void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        internal void OnException(Exception e)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        internal void OnLog(RPCLog log)
        {
            if (LogEvent != null)
            {
                LogEvent(log);
            }
        }
        #endregion
    }
}
