using Castle.DynamicProxy;
using EtherealC.Core;
using EtherealC.Core.Event;
using EtherealC.Core.Interface;
using EtherealC.Core.Model;
using EtherealC.Request.Attribute;
using EtherealC.Request.Interface;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Abstract
{
    public abstract class Request : IRequest, IBaseIoc
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
        private string name;
        private Net.Abstract.Net net;
        protected RequestConfig config;
        private Dictionary<int, ClientRequestModel> tasks = new();
        private AbstractTypes types = new();
        private Client.Abstract.Client client;
        private Dictionary<string, Service.Abstract.Service> services = new Dictionary<string, Service.Abstract.Service>();
        #endregion

        #region --属性--

        public RequestConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        public Dictionary<string, Service.Abstract.Service> Services { get => services; set => services = value; }
        internal protected Dictionary<string, object> IocContainer { get; set; } = new();
        internal Dictionary<int, ClientRequestModel> Tasks { get => tasks; set => tasks = value; }

        public EventManager EventManager { get; set; } = new EventManager();

        #endregion

        #region --方法--
        internal static T Register<T>() where T : Request
        {
            ProxyGenerator generator = new ProxyGenerator();
            RequestInterceptor interceptor = new RequestInterceptor();
            T request = generator.CreateClassProxy<T>(interceptor);
            return request;
        }
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

        public abstract void Initialize();
        public abstract void UnInitialize();

        internal void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            if (Tasks.TryGetValue(int.Parse(response.Id), out ClientRequestModel model))
            {
                model.Set(response);
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}-{response.Id}返回的请求ID未找到!");
        }

        public void RegisterIoc(string name, object instance)
        {
            if (IocContainer.ContainsKey(name))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}请求中的{name}实例已注册");
            }
            IocContainer.Add(name, instance);
            EventManager.RegisterEventMethod(name, instance);
        }
        public void UnRegisterIoc(string name)
        {
            if (IocContainer.TryGetValue(name, out object instance))
            {
                IocContainer.Remove(name);
                EventManager.UnRegisterEventMethod(name, instance);
            }
        }
        public bool GetIocObject(string name, out object instance)
        {
            return IocContainer.TryGetValue(name, out instance);
        }

        public abstract void Register();
        public abstract void UnRegister();
        #endregion
    }
}
