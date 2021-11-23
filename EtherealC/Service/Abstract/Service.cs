using EtherealC.Core.Attribute;
using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Manager.Event.Attribute;
using EtherealC.Core.Model;
using EtherealC.Net.Extension.Plugins;
using EtherealC.Service.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Service.Abstract
{
    public abstract class Service : Core.BaseCore.MZCore,IService
    {
        #region --事件字段--

        #endregion

        #region --事件属性--

        #endregion
        internal string name;
        protected Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected Request.Abstract.Request request;
        protected ServiceConfig config;
        protected PluginDomain pluginDomain;

        public Dictionary<string, MethodInfo> Methods { get => methods; }
        public ServiceConfig Config { get => config; set => config = value; }
        public PluginDomain PluginDomain { get => pluginDomain; set => pluginDomain = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }
        public string Name { get => name; set => name = value; }

        internal static void Register<T>(T instance) where T : Service
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.ServiceMapping attribute = method.GetCustomAttribute<Attribute.ServiceMapping>();
                if (method.ReturnType != typeof(void) && attribute != null)
                {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        BaseParam paramsAttribute = parameterInfo.GetCustomAttribute<BaseParam>(true);
                        if (paramsAttribute != null)
                        {
                            continue;
                        }
                        Param paramAttribute = parameterInfo.GetCustomAttribute<Param>();
                        if (paramAttribute != null && !instance.Types.Get(paramAttribute.Type, out AbstractType type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{paramAttribute.Type}抽象类型未找到");
                        }
                        else if (!instance.Types.Get(parameterInfo.ParameterType, out type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{parameterInfo.ParameterType}类型映射抽象类型");
                        }
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
            @params = new(parameterInfos.Length);
            object[] localParams = new object[parameterInfos.Length];
            int idx = 0;
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                if (request.Params.TryGetValue(parameterInfo.Name, out string value))
                {
                    Types.Get(parameterInfo, out AbstractType type);
                    localParams[idx] = type.Deserialize(value);
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"来自服务器的{Name}服务请求中未提供{method.Name}方法的{parameterInfo.Name}参数");
                @params.Add(parameterInfo.Name, localParams[idx++]);
            }
            eventSender = method.GetCustomAttribute<BeforeEvent>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
            object result = null;
            try
            {
                result = method.Invoke(this, localParams);
            }
            catch (Exception e)
            {
                eventSender = method.GetCustomAttribute<ExceptionEvent>();
                if (eventSender != null)
                {
                    (eventSender as ExceptionEvent).Exception = e;
                    eventContext = new ExceptionEventContext(@params, method, e);
                    IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                    if ((eventSender as ExceptionEvent).IsThrow) throw;
                }
                else throw;
            }
            eventSender = method.GetCustomAttribute<AfterEvent>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, result);
                IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
        }

        public abstract void Initialize();
        public abstract void UnInitialize();
        public abstract void Register();
        public abstract void UnRegister();
    }
}
