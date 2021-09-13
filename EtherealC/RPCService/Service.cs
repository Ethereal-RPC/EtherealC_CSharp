using EtherealC.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EtherealC.RPCService
{
    public class Service
    {

        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception, Service service);
        public delegate void OnLogDelegate(RPCLog log, Service service);
        #endregion

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
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private object instance;
        private string name;
        private string netName;
        ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public object Instance { get => instance; set => instance = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string Name { get => name; set => name = value; }
        public string NetName { get => netName; set => netName = value; }

        public void Register<T>(T instance, string netName, string servicename, ServiceConfig config)
        {
            this.instance = instance;
            this.Config = config;
            this.NetName = netName;
            this.Name = servicename;
            //遍历所有字段
            foreach(FieldInfo field in instance.GetType().GetFields())
            {
                Attribute.Service.ServiceConfig rpcAttribute = field.GetCustomAttribute<Attribute.Service.ServiceConfig>();
                if (rpcAttribute != null)
                {
                    //将config赋值入该Service
                    field.SetValue(instance, config);
                }
            }
            
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.RPCService rpcAttribute = method.GetCustomAttribute<Attribute.RPCService>();
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
                                if(config.Types.TypesByType.TryGetValue(param.ParameterType,out RPCType type))
                                {
                                    methodid.Append("-" + type.Name);
                                }
                                else throw new RPCException(RPCException.ErrorCode.Core, $"C#中的{param.ParameterType}类型参数尚未注册");
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if(parameters.Length == types_name.Length)
                            {
                                foreach (string type_name in types_name)
                                {
                                    if (config.Types.TypesByName.TryGetValue(type_name, out RPCType type))
                                    {
                                        methodid.Append("-").Append(type.Name);
                                    }
                                    else throw new RPCException(RPCException.ErrorCode.Core,$"C#对应的{types_name}类型参数尚未注册");
                                }
                            }
                        }
                        Methods.TryAdd(methodid.ToString(), method);
                        methodid.Length = 0;
                    }
                }
            }
        }
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e, this);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                logEvent.Invoke(log, this);
            }
        }
    }
}
