﻿using System.Reflection;
using System.Text;
using EtherealC.Core.Model;
using EtherealC.Service.Abstract;

namespace EtherealC.Service.WebSocket
{

    public class WebSocketService:Abstract.Service
    {
        #region --属性--
        public new WebSocketServiceConfig Config { get => (WebSocketServiceConfig)config; set => config = value; }

        #endregion
        public override void Register<T>(T instance, string netName, string servicename, ServiceConfig config)
        {
            this.instance = instance;
            this.config = config;
            this.netName = netName;
            this.name = servicename;
            //遍历所有字段
            foreach(FieldInfo field in instance.GetType().GetFields())
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
                                if(config.Types.TypesByType.TryGetValue(param.ParameterType,out AbstractType type))
                                {
                                    methodid.Append("-" + type.Name);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Core, $"C#中的{param.ParameterType}类型参数尚未注册");
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if(parameters.Length == types_name.Length)
                            {
                                foreach (string type_name in types_name)
                                {
                                    if (config.Types.TypesByName.TryGetValue(type_name, out AbstractType type))
                                    {
                                        methodid.Append("-").Append(type.Name);
                                    }
                                    else throw new TrackException(TrackException.ErrorCode.Core,$"C#对应的{types_name}类型参数尚未注册");
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
