using EtherealC.Model;
using System;

namespace EtherealC.RPCService
{
    public class ServiceConfig
    {


        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception,Service service);
        public delegate void OnLogDelegate(RPCLog log,Service service);
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
        internal void OnException(RPCException.ErrorCode code, string message, Service service)
        {
            OnException(new RPCException(code, message),service);
        }
        internal void OnException(Exception e, Service service)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e, service);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, Service service)
        {
            OnLog(new RPCLog(code, message), service);
        }
        internal void OnLog(RPCLog log, Service service)
        {
            if (LogEvent != null)
            {
                LogEvent(log, service);
            }
        }
        #endregion
    }
}
