using System;
using System.Collections.Generic;
using System.Reflection;
using EtherealC.Core.Delegates;
using EtherealC.Core.Model;
using EtherealC.RPCService.Interface;

namespace EtherealC.RPCService.Abstract
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
        protected string name;
        protected string netName;
        protected ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public object Instance { get => instance; set => instance = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public string NetName { get => netName; set => netName = value; }

        public abstract void Register<T>(T instance, string netName, string servicename, ServiceConfig config);
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {

            if (e is not RPCException)
            {
                e = new RPCException(e);
            }
            (e as RPCException).Service = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            logEvent?.Invoke(log);
        }
    }
}
