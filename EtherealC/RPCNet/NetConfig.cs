using EtherealC.Model;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;

namespace EtherealC.RPCNet
{
    public class NetConfig
    {
        #region --委托--
        public delegate void OnLogDelegate(RPCLog log,Net net);
        public delegate void OnExceptionDelegate(Exception exception,Net net);

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
        internal void OnException(RPCException.ErrorCode code, string message, Net net)
        {
            OnException(new RPCException(code, message), net);
        }
        internal void OnException(Exception e, Net net)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e, net);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, Net net)
        {
            OnLog(new RPCLog(code, message), net);
        }
        internal void OnLog(RPCLog log, Net net)
        {
            if (LogEvent != null)
            {
                LogEvent(log, net);
            }
        }

        #endregion
    }
}
