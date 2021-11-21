using EtherealC.Core;
using EtherealC.Core.Attribute;
using EtherealC.Core.EventManage;
using EtherealC.Core.EventManage.Attribute;
using EtherealC.Core.Interface;
using EtherealC.Core.Model;
using EtherealC.Request.Attribute;
using EtherealC.Request.Event;
using EtherealC.Request.Interface;
using EtherealC.Utils;
using System;
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
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private Dictionary<string, Service.Abstract.Service> services = new Dictionary<string, Service.Abstract.Service>();
        private Random random = new Random();
        #endregion

        #region --属性--

        public RequestConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        public Dictionary<string, Service.Abstract.Service> Services { get => services; set => services = value; }
        internal Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        internal protected Dictionary<string, object> IocContainer { get; set; } = new();
        internal Dictionary<int, ClientRequestModel> Tasks { get => tasks; set => tasks = value; }

        public EventManager EventManager { get; set; } = new EventManager();

        #endregion

        #region --方法--
        internal static T Register<T>() where T : Request
        {
            T request = DynamicProxy.CreateRequestProxy<T>();
            foreach (MethodInfo method in typeof(T).GetMethods())
            {
                RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
                if (attribute != null)
                {
                    request.Methods.Add(attribute.Mapping, method);
                }
            }
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

        public virtual object Invoke(string mapping, object[] args, object localResult)
        {
            MethodInfo method = null;
            RequestMapping attribute = null;
            object remoteResult = null;
            object methodResult = null;
            ClientRequestModel request = null;
            Dictionary<string, object> @params = null;
            ClientResponseModel result = null;
            EventSender eventSender;
            EventContext eventContext;
            try
            {
                //方法信息获取
                Methods.TryGetValue(mapping, out method);
                attribute = method.GetCustomAttribute<RequestMapping>();
                //注入参数
                ParameterInfo[] parameterInfos = method.GetParameters();
                request = new ClientRequestModel();
                request.Mapping = attribute.Mapping;
                request.Params = new string[parameterInfos.Length];
                @params = new(parameterInfos.Length);
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    Param paramAttribute = parameterInfos[i].GetCustomAttribute<Param>(true);
                    if (paramAttribute != null && Types.TypesByName.TryGetValue(paramAttribute.Name, out AbstractType type) || Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out type))
                    {
                        request.Params[i] = type.Serialize(args[i]);
                        @params.Add(parameterInfos[i].Name, args[i]);
                    }
                    else throw new TrackException($"{method.Name}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
                }
                eventSender = method.GetCustomAttribute<BeforeEvent>();
                if (eventSender != null)
                {
                    eventContext = new BeforeEventContext(@params, method);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                }
                if (attribute.InvokeType.HasFlag(RequestMapping.InvokeTypeFlags.Remote))
                {
                    if (method.ReturnType == typeof(void))
                    {
                        Client.SendClientRequestModel(request);
                    }
                    else
                    {
                        int id = random.Next();
                        while (Tasks.TryGetValue(id, out ClientRequestModel value))
                        {
                            id = random.Next();
                        }
                        Tasks.TryAdd(id, request);
                        try
                        {
                            request.Id = id.ToString();
                            int timeout = Config.Timeout;
                            if (attribute.Timeout != -1) timeout = attribute.Timeout;
                            Client.SendClientRequestModel(request);
                            result = request.Get(timeout);
                            if (result != null)
                            {
                                if (result.Error != null)
                                {
                                    eventSender = method.GetCustomAttribute<FailEvent>();
                                    if (eventSender != null)
                                    {
                                        eventContext = new FailEventContext(@params, method, result.Error);
                                        EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                                    }
                                    else throw new TrackException(TrackException.ErrorCode.Runtime, $"来自服务器的报错消息:\nErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                                }
                                Param abstractTypeAttribute = method.GetCustomAttribute<Param>(true);
                                if ((abstractTypeAttribute != null && Types.TypesByName.TryGetValue(abstractTypeAttribute.Name, out AbstractType type))
                                    || Types.TypesByType.TryGetValue(method.ReturnType, out type))
                                {
                                    remoteResult = type.Deserialize(result.Result);
                                    eventSender = method.GetCustomAttribute<SuccessEvent>();
                                    if (eventSender != null)
                                    {
                                        eventContext = new SuccessEventContext(@params, method, result.Result);
                                        EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                                    }
                                }
                                else throw new TrackException($"{method.Name}方法中的{method.ReturnType}类型参数尚未注册");
                            }
                            else
                            {
                                eventSender = method.GetCustomAttribute<TimeoutEvent>();
                                if (eventSender != null)
                                {
                                    eventContext = new TimeoutEventContext(@params, method);
                                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                                }
                            }
                        }
                        finally
                        {
                            Tasks.Remove(id, out ClientRequestModel value);
                        }
                    }
                }
                if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.ReturnRemote) != 0)
                {
                    methodResult = remoteResult;
                }
                if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.ReturnLocal) != 0)
                {
                    methodResult = localResult;
                }
                eventSender = method.GetCustomAttribute<AfterEvent>();
                if (eventSender != null)
                {
                    eventContext = new AfterEventContext(@params, method, methodResult);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                }
            }
            catch (Exception e)
            {
                eventSender = method.GetCustomAttribute<ExceptionEvent>();
                if (eventSender != null)
                {
                    (eventSender as ExceptionEvent).Exception = e;
                    eventContext = new ExceptionEventContext(@params, method, e);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                    if ((eventSender as ExceptionEvent).IsThrow) throw;
                }
                else throw;
            }
            return methodResult;
        }
        public void RegisterIoc(string name, object instance)
        {
            if (IocContainer.ContainsKey(name))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}请求中的{name}实例已注册");
            }
            IocContainer.Add(name, instance);
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
        #endregion
    }
}
