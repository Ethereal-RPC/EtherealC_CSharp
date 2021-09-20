﻿using System.Collections.Concurrent;
using System.Reflection;
using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Request.WebSocket;

namespace EtherealC.Request.Abstract
{
    public abstract class Request : DispatchProxy
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
        protected string netName;
        protected RequestConfig config;
        protected ConcurrentDictionary<int, ClientRequestModel> tasks = new ConcurrentDictionary<int, ClientRequestModel>();
        #endregion

        #region --属性--
        public RequestConfig Config { get => config; set => config = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        public string NetName { get => netName; set => netName = value; }
        public string Name { get => name; set => name = value; }
        #endregion

        #region --方法--

        public bool GetTask(int id, out ClientRequestModel model)
        {
            return tasks.TryGetValue(id, out model);
        }

        public static Request Register<T>(string netName, string servicename, WebSocketRequestConfig config)
        {
            Request proxy = (Request)(Create<T, Request>() as object);
            proxy.NetName = netName;
            proxy.Name = servicename;
            proxy.Config = config;
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
        #endregion
    }
}
        