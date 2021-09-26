using System.Collections.Generic;
using System.Reflection;
using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Service.Interface;

namespace EtherealC.Service.Abstract
{
    public abstract class Service:IService
    {
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
    
        //原作者的思想是Type调用Invoke，这里是在注册的时候就预存方法，1e6情况下调用速度的话是快了4-5倍左右，比正常调用慢10倍
        //string连接的时候使用引用要比tuple慢很多
        protected Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected object instance;
        protected string serviceName;
        protected string netName;
        protected ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public object Instance { get => instance; set => instance = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        public string NetName { get => netName; set => netName = value; }

        public abstract void Register<T>(T instance, string netName, string servicename, ServiceConfig config);
        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            e.Service = this;
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
    }
}
