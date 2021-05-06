using EtherealC.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EtherealC.RPCService
{
    public class Service
    {

        //原作者的思想是Type调用Invoke，这里是在注册的时候就预存方法，1e6情况下调用速度的话是快了4-5倍左右，比正常调用慢10倍
        //string连接的时候使用引用要比tuple慢很多
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private object instance;
        private string servicename;
        private Tuple<string, string> clientKey;
        ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public object Instance { get => instance; set => instance = value; }
        public ServiceConfig Config { get => config; set => config = value; }

        public void Register<T>(T instance, Tuple<string, string> clientKey, string servicename, ServiceConfig config)
        {
            this.instance = instance;
            this.Config = config;
            this.clientKey = clientKey;
            this.servicename = servicename;
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
                                else throw new RPCException($"C#中的{param.ParameterType}类型参数尚未注册");
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
                                    else throw new RPCException($"C#对应的{types_name}类型参数尚未注册");
                                }
                            }
                        }
                        Methods.TryAdd(methodid.ToString(), method);
                        methodid.Length = 0;
                    }
                }
            }
        }
    }
}
