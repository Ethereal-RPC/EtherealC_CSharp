using EtherealC.Core;
using EtherealC.Core.Event;
using EtherealC.Core.Event.Attribute;
using EtherealC.Core.Interface;
using EtherealC.Core.Model;
using EtherealC.Net.Extension.Plugins;
using EtherealC.Service.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Service.Abstract
{
    public abstract class Service : IService, IBaseIoc
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
        protected string name;
        protected Request.Abstract.Request request;
        protected ServiceConfig config;
        protected AbstractTypes types = new AbstractTypes();
        protected PluginDomain pluginDomain;

        public Dictionary<string, MethodInfo> Methods { get => methods; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public PluginDomain PluginDomain { get => pluginDomain; set => pluginDomain = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }
        internal protected Dictionary<string, object> IocContainer { get; set; }
        public EventManager EventManager { get; set; } = new EventManager();

        internal static void Register<T>(T instance) where T : Service
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.ServiceMapping attribute = method.GetCustomAttribute<Attribute.ServiceMapping>();
                if (attribute != null)
                {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        Core.Attribute.Param ParamsAttribute = parameterInfo.GetCustomAttribute<Core.Attribute.Param>(true);
                        if ((ParamsAttribute != null && instance.Types.TypesByName.TryGetValue(ParamsAttribute.Name, out AbstractType type))
                            || instance.Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out type))
                        {
                            continue;
                        }
                        else throw new TrackException($"{method.Name}方法中的{parameterInfo.ParameterType}类型参数尚未注册");
                    }
                    instance.methods.TryAdd(attribute.Mapping, method);
                }
            }
        }
        internal void ServerRequestReceiveProcess(ServerRequestModel request)
        {
            EventContext eventContext;
            EventSender eventSender;
            Dictionary<string, object> @params = null;
            if (!Methods.TryGetValue(request.Mapping, out MethodInfo method))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}-{request.Service}-{request.Mapping}未找到!");
            }
            ParameterInfo[] parameterInfos = method.GetParameters();
            int i = 0;
            @params = new(parameterInfos.Length);
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                Core.Attribute.Param abstractTypeAttribute = parameterInfo.GetCustomAttribute<Core.Attribute.Param>(true);
                if ((abstractTypeAttribute != null && Types.TypesByName.TryGetValue(abstractTypeAttribute.Name, out Core.Model.AbstractType type))
                    || Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out type))
                {
                    request.Params[i] = type.Deserialize((string)request.Params[i]);
                    @params.Add(parameterInfo.Name, request.Params[i++]);
                }
                else throw new TrackException($"RPC中的{request.Params[i]}类型中尚未被注册");
            }
            eventSender = method.GetCustomAttribute<BeforeEvent>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
            object result = null;
            try
            {
                result = method.Invoke(this, request.Params);
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
            eventSender = method.GetCustomAttribute<AfterEvent>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, result);
                EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
        }

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

        public abstract void Initialize();
        public abstract void UnInitialize();

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
    }
}
