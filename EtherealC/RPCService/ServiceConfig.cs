using EtherealC.Model;
using System;

namespace EtherealC.RPCService
{
    public class ServiceConfig
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
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }

        #endregion

        #region --方法--
        public ServiceConfig(RPCTypeConfig types)
        {
            this.types = types;
        }

        public ServiceConfig(RPCTypeConfig types, bool tokenEnable)
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
