using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealC.Core.Model;
using Newtonsoft.Json;

namespace EtherealC.Request.WebSocket
{
    public class WebSocketRequest:Abstract.Request
    {
        #region --字段--

        private Random random = new Random();

        #endregion


        #region --属性--

        public new WebSocketRequestConfig Config { get => (WebSocketRequestConfig)config; set => config = value; }

        #endregion


        #region --方法--

        public WebSocketRequest()
        {
            config = new WebSocketRequestConfig();
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Attribute.Request rpcAttribute = targetMethod.GetCustomAttribute<Attribute.Request>();
            if (rpcAttribute == null)
            {
                return targetMethod.Invoke(this, args);
            }
            object localResult = null;
            object remoteResult = null;
            if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.Local) == 0)
            {
                //这里要连接字符串，发现StringBuilder效率高一些.
                StringBuilder methodid = new StringBuilder(targetMethod.Name);
                ParameterInfo[] parameterInfos = targetMethod.GetParameters();
                //理想状态下为抛出Token的参数数量，但后期可能会存在不只是一个特殊类的问题，所以改为了动态数组。
                List<string> @params = new List<string>(parameterInfos.Length - 1);
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    try
                    {
                        if (Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out AbstractType type)
                        || Types.TypesByName.TryGetValue(parameterInfos[i].GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                        {
                            methodid.Append("-" + type.Name);
                            @params.Add(type.Serialize(args[i]));
                        }
                        else throw new TrackException($"{targetMethod.Name}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
                    }
                    catch (ArgumentNullException)
                    {
                        throw new TrackException($"{targetMethod.Name}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
                    }
                }
                ClientRequestModel request = new ClientRequestModel(Name, methodid.ToString(), @params.ToArray());
                if (targetMethod.ReturnType == typeof(void))
                {
                    client.SendClientRequestModel(request);
                }
                else
                {
                    int id = random.Next();
                    while (tasks.TryGetValue(id, out ClientRequestModel value))
                    {
                        id = random.Next();
                    }
                    tasks.TryAdd(id, request);
                    try{
                        request.Id = id.ToString();
                        int timeout = Config.Timeout;
                        if (rpcAttribute.Timeout != -1) timeout = rpcAttribute.Timeout;
                        client.SendClientRequestModel(request);
                        ClientResponseModel result = request.Get(timeout);
                        if (result != null)
                        {
                            if (result.Error != null)
                            {
                                if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.Fail) != 0)
                                {
                                    localResult = targetMethod.Invoke(this, args);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Runtime, $"ErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                            }
                            if (Types.TypesByType.TryGetValue(targetMethod.ReturnType, out AbstractType type)
                            || Types.TypesByName.TryGetValue(targetMethod.GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                            {
                                remoteResult = type.Deserialize(result.Result);
                                if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.Success) != 0
                                    || (rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.All) != 0)
                                {
                                    localResult = targetMethod.Invoke(this, args);
                                }
                            }
                            else throw new TrackException($"{targetMethod.Name}方法中的{targetMethod.ReturnType}类型参数尚未注册");
                        }
                        else if((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.Timeout) != 0)
                        { 
                            localResult = targetMethod.Invoke(this, args);
                        }
                    }
                    finally
                    {
                        tasks.TryRemove(id,out ClientRequestModel value);
                    }
                }
            }
            else
            {
                localResult = targetMethod.Invoke(this, args);
            }

            if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.ReturnRemote) != 0)
            {
                return remoteResult;
            }   
            if((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.ReturnLocal) != 0)
            {
                return localResult;
            }
            return remoteResult;
        }
        #endregion
    }
}
        