using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Net.Interface;
using System.Collections.Generic;

namespace EtherealC.Net.Abstract
{
    public abstract class Net : INet
    {
        public enum NetType { WebSocket }
        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }
        #endregion

        #region --字段--
        /// <summary>
        /// Net网关名
        /// </summary>
        protected string name;
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected Dictionary<string, Request.Abstract.Request> requests = new Dictionary<string, Request.Abstract.Request>();
        protected NetType type = NetType.WebSocket;
        protected NetConfig config;
        #endregion

        #region --属性--
        public Dictionary<string, Request.Abstract.Request> Requests { get => requests; set => requests = value; }
        public string Name { get => name; set => name = value; }
        public NetType Type { get => type; set => type = value; }
        public NetConfig Config { get => config; set => config = value; }

        #endregion

        #region --方法--

        public Net(string name)
        {
            this.name = name;
        }

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            e.Net = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }

        public void OnLog(TrackLog log)
        {
            logEvent?.Invoke(log);
        }
        #endregion
    }
}
