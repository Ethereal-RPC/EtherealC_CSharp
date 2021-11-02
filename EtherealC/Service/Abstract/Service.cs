﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Net.Extension.Plugins;
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
        protected string name;
        protected string netName;
        protected ServiceConfig config;
        protected AbstractTypes types = new AbstractTypes();
        protected PluginDomain pluginDomain;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public ServiceConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public string NetName { get => netName; set => netName = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public PluginDomain PluginDomain { get => pluginDomain; set => pluginDomain = value; }

        public static void Register<T>(T instance)where T:Service
        {
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.ServiceMethod rpcAttribute = method.GetCustomAttribute<Attribute.ServiceMethod>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameterInfos = method.GetParameters();
                        foreach (ParameterInfo parameterInfo in parameterInfos)
                        {
                            if (instance.Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out AbstractType type)
                                || instance.Types.TypesByName.TryGetValue(parameterInfo.GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                            {
                                methodid.Append("-" + type.Name);
                            }
                            else throw new TrackException($"{method.Name}方法中的{parameterInfo.ParameterType}类型参数尚未注册");
                        }
                        instance.methods.TryAdd(methodid.ToString(), method);
                        methodid.Length = 0;
                    }
                }
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
    }
}
