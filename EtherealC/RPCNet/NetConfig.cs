using EtherealC.Model;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;

namespace EtherealC.RPCNet
{
    public class NetConfig
    {
        #region --委托--
        public delegate void OnLogDelegate(RPCLog log);
        public delegate void OnExceptionDelegate(Exception exception);

        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --字段--

        #endregion

        #region --属性--

        #endregion

        #region --方法--
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
