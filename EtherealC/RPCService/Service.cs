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

        ServiceConfig config;

        public Dictionary<string, MethodInfo> Methods { get => methods;  }
        public object Instance { get => instance; set => instance = value; }

        public void Register<T>(T instance,ServiceConfig config)
        {
            this.Instance = instance;
            this.config = config;
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in typeof(T).GetMethods())
            {
                Attribute.RPCService rpcAttribute = method.GetCustomAttribute<Attribute.RPCService>();
                if (rpcAttribute == null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        if (rpcAttribute.Paramters == null)
                        {
                            foreach (ParameterInfo param in parameters)
                            {
                                try
                                {
                                    methodid.Append("-" + config.Type.AbstractName[param.ParameterType]);
                                }
                                catch (Exception)
                                {
                                    throw new RPCException($"C#中的{param.ParameterType}类型参数尚未注册");
                                }
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if(parameters.Length == types_name.Length)
                            {
                                foreach (string type_name in types_name)
                                {
                                    if (config.Type.AbstractType.ContainsKey(type_name))
                                    {
                                        methodid.Append("-").Append(types_name);
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
        public void ConvertParams(string methodId, object[] parameters)
        {
            string[] param_id = methodId.Split('-');
            if (param_id.Length > 1)
            {
                for (int i = 1,j=0; i < param_id.Length; i++,j++)
                {
                    if (config.Type.TypeConvert.TryGetValue(param_id[i], out RPCType.ConvertDelegage convert))
                    {
                        parameters[j] = convert((string)parameters[j]);
                    }
                    else throw new RPCException($"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                }
            }
        }
    }
}
