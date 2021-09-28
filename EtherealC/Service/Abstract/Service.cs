using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
        protected string serviceName;
        protected string netName;
        protected ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public ServiceConfig Config { get => config; set => config = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        public string NetName { get => netName; set => netName = value; }

        public static void Register<T>(T instance, string netName, string servicename, ServiceConfig config)where T:Service
        {
            instance.config = config;
            instance.netName = netName;
            instance.serviceName = servicename;
            //遍历所有字段
            foreach (FieldInfo field in instance.GetType().GetFields())
            {
                Attribute.ServiceConfig rpcAttribute = field.GetCustomAttribute<Attribute.ServiceConfig>();
                if (rpcAttribute != null)
                {
                    //将config赋值入该Service
                    field.SetValue(instance, config);
                }
            }

            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.Service rpcAttribute = method.GetCustomAttribute<Attribute.Service>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        if (rpcAttribute.Paramters == null)
                        {
                            foreach (ParameterInfo param in parameters)
                            {
                                if (config.Types.TypesByType.TryGetValue(param.ParameterType, out AbstractType type))
                                {
                                    methodid.Append("-" + type.Name);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Core, $"C#中的{param.ParameterType}类型参数尚未注册");
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if (parameters.Length == types_name.Length)
                            {
                                foreach (string type_name in types_name)
                                {
                                    if (config.Types.TypesByName.TryGetValue(type_name, out AbstractType type))
                                    {
                                        methodid.Append("-").Append(type.Name);
                                    }
                                    else throw new TrackException(TrackException.ErrorCode.Core, $"C#对应的{types_name}类型参数尚未注册");
                                }
                            }
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
    }
}
