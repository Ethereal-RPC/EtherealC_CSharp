using System;
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

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Attribute.Request rpcAttribute = targetMethod.GetCustomAttribute<Attribute.Request>();
            if (rpcAttribute != null)
            {
                //这里要连接字符串，发现StringBuilder效率高一些.
                StringBuilder methodid = new StringBuilder(targetMethod.Name);
                int param_count;
                if (args != null) param_count = args.Length;
                else param_count = 0;
                object[] obj = new object[param_count + 1];
                if (rpcAttribute.Paramters == null)
                {
                    ParameterInfo[] parameters = targetMethod.GetParameters();
                    for (int i = 0, j = 1; i < param_count; i++, j++)
                    {
                        if (Config.Types.TypesByType.TryGetValue(parameters[i].ParameterType, out AbstractType type))
                        {
                            methodid.Append("-" + type.Name);
                            obj[j] = type.Serialize(args[i]);
                        }
                        else throw new TrackException(TrackException.ErrorCode.Runtime, $"C#中的{args[i].GetType()}类型参数尚未注册");
                    }
                }
                else
                {
                    string[] types_name = rpcAttribute.Paramters;
                    if (types_name.Length == param_count)
                    {
                        for (int i = 0, j = 1; i < param_count; i++, j++)
                        {
                            try
                            {
                                methodid.Append("-" + types_name[i]);
                                obj[j] = JsonConvert.SerializeObject(args[i]);
                            }
                            catch (Exception)
                            {
                                throw new TrackException(TrackException.ErrorCode.Runtime, $"C#中的{args[i].GetType()}类型参数尚未注册");
                            }
                        }
                    }
                    else throw new TrackException(TrackException.ErrorCode.Runtime, $"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个");
                }
                ClientRequestModel request = new ClientRequestModel(ServiceName, methodid.ToString(), obj);
                if (targetMethod.ReturnType == typeof(void))
                {
                    client.SendClientRequestModel(request);
                    return null;
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
                                throw new TrackException(TrackException.ErrorCode.Runtime, $"ErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                            }
                            else if (Config.Types.TypesByName.TryGetValue(result.ResultType, out AbstractType type))
                            {
                                return type.Deserialize((string)result.Result);
                            }
                            else throw new TrackException(TrackException.ErrorCode.Runtime, $"C#中的{result.ResultType}类型转换器尚未注册");
                        }
                    }
                    finally
                    {
                        tasks.TryRemove(id,out ClientRequestModel value);
                    }
                    return null;
                }
            }
            return null;
        }
        #endregion
    }
}
        