﻿using System.Collections.Concurrent;
using System.Reflection;
using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Request.Interface;
using EtherealC.Request.WebSocket;

namespace EtherealC.Request.Abstract
{
    public abstract class Request : DispatchProxy,IRequest
    {
        #region --委托--
        public delegate void OnConnnectSuccessDelegate(Request request);
        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        public event OnConnnectSuccessDelegate ConnectSuccessEvent;
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
        protected Client.Abstract.Client client;
        protected string name;
        protected Net.Abstract.Net net;
        protected RequestConfig config;
        protected ConcurrentDictionary<int, ClientRequestModel> tasks = new();
        protected AbstractTypes types = new();
        #endregion

        #region --属性--

        public RequestConfig Config { get => config; set => config = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }

        #endregion

        #region --方法--

        public bool GetTask(int id, out ClientRequestModel model)
        {
            return tasks.TryGetValue(id, out model);
        }

        public static R Register<R,T>()where R:Request
        {
            R proxy = Create<T, R>() as R;
            return proxy;
        }

        protected abstract override object Invoke(MethodInfo targetMethod, object[] args);
        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            e.Request = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }
        public void OnLog(TrackLog log)
        {
            log.Request = this;
            logEvent?.Invoke(log);
        }
        public void OnConnectSuccess()
        {
            ConnectSuccessEvent?.Invoke(this);
        }

        public abstract void Initializate();
        public abstract void UnInitialize();
        #endregion
    }
}
        