using System;
using System.Collections.Concurrent;
using System.Reflection;
using EtherealC.Core.Delegates;
using EtherealC.Core.Model;
using EtherealC.NativeClient.Abstract;
using EtherealC.RPCRequest.WebSocket;

namespace EtherealC.RPCRequest.Abstract
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
        protected Client client;
        protected string name;
        protected string netName;
        protected RequestConfig config;
        protected ConcurrentDictionary<int, ClientRequestModel> tasks = new ConcurrentDictionary<int, ClientRequestModel>();
        #endregion

        #region --属性--
        public RequestConfig Config { get => config; set => config = value; }
        public Client Client { get => client; set => client = value; }
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
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(RPCException e)
        {
            e.Request = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
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
        