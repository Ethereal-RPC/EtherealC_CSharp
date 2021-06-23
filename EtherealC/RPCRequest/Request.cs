using EtherealC.Model;
using EtherealC.RPCNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace EtherealC.RPCRequest
{
    public class Request : DispatchProxy
    {
        private string servicename;
        private string netName;
        private RequestConfig config;
        private ConcurrentDictionary<int, ClientRequestModel> tasks = new ConcurrentDictionary<int, ClientRequestModel>();
        private Random random = new Random();

        public bool GetTask(int id, out ClientRequestModel model)
        {
            return tasks.TryGetValue(id, out model);
        }

        public static Request Register<T>(string netName,string servicename, RequestConfig config)
        {
            Request proxy = (Request)(Create<T, Request>() as object);
            proxy.netName = netName;
            proxy.servicename = servicename; 
            proxy.config = config;
            return proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Attribute.RPCRequest rpcAttribute = targetMethod.GetCustomAttribute<Attribute.RPCRequest>();
            if(rpcAttribute != null)
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
                        if(config.Types.TypesByType.TryGetValue(parameters[i].ParameterType,out RPCType type))
                        {
                            methodid.Append("-" + type.Name);
                            obj[j] = type.Serialize(args[i]);
                        }
                        else config.OnException(RPCException.ErrorCode.Runtime,$"C#中的{args[i].GetType()}类型参数尚未注册",this);
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
                                config.OnException(RPCException.ErrorCode.Runtime, $"C#中的{args[i].GetType()}类型参数尚未注册", this);
                            }
                        }
                    }
                    else config.OnException(RPCException.ErrorCode.Runtime,$"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个", this);
                }
                ClientRequestModel request = new ClientRequestModel("2.0", servicename, methodid.ToString(), obj);
                if (!NetCore.Get(netName, out Net net))
                {
                    config.OnException(RPCException.ErrorCode.Runtime, "未找到NetConfig", this);
                }
                if (targetMethod.ReturnType == typeof(void))
                {
                    net.ClientRequestSend(request);
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
                    request.Id = id.ToString();
                    int timeout = config.Timeout;
                    if (rpcAttribute.Timeout != -1) timeout = rpcAttribute.Timeout;
                    net.ClientRequestSend(request);
                    ClientResponseModel result = request.Get(timeout);
                    if (result != null)
                    {
                        if (result.Error != null)
                        {
                            if (result.Error.Code == 0)
                            {
                                config.OnException(RPCException.ErrorCode.Intercepted, $"ErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}", this);
                            }
                        }
                        else if (config.Types.TypesByName.TryGetValue(result.ResultType, out RPCType type))
                        {
                            return type.Deserialize((string)result.Result);
                        }
                        else config.OnException(RPCException.ErrorCode.Runtime,$"C#中的{result.ResultType}类型转换器尚未注册", this);
                    }
                    return null;
                }
            }
            return null;
        }
    }
}
        