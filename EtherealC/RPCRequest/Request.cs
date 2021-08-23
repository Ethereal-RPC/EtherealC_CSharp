using EtherealC.Model;
using EtherealC.NativeClient;
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
        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception, Request request);
        public delegate void OnLogDelegate(RPCLog log, Request request);
        public delegate void OnConnnectSuccessDelegate(Request request);
        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        public event OnConnnectSuccessDelegate ConnectSuccessEvent;
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

        #region --字段--
        private SocketClient client;
        private string name;
        private string netName;
        private RequestConfig config;
        private ConcurrentDictionary<int, ClientRequestModel> tasks = new ConcurrentDictionary<int, ClientRequestModel>();
        private Random random = new Random();
        #endregion

        #region --属性--
        public RequestConfig Config { get => config; set => config = value; }
        public SocketClient Client { get => client; set => client = value; }
        public string NetName { get => netName; set => netName = value; }
        public string Name { get => name; set => name = value; }
        #endregion

        #region --方法--

        public bool GetTask(int id, out ClientRequestModel model)
        {
            return tasks.TryGetValue(id, out model);
        }

        public static Request Register<T>(string netName, string servicename, RequestConfig config)
        {
            Request proxy = (Request)(Create<T, Request>() as object);
            proxy.NetName = netName;
            proxy.Name = servicename;
            proxy.Config = config;
            return proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Attribute.RPCRequest rpcAttribute = targetMethod.GetCustomAttribute<Attribute.RPCRequest>();
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
                        if (Config.Types.TypesByType.TryGetValue(parameters[i].ParameterType, out RPCType type))
                        {
                            methodid.Append("-" + type.Name);
                            obj[j] = type.Serialize(args[i]);
                        }
                        else OnException(RPCException.ErrorCode.Runtime, $"C#中的{args[i].GetType()}类型参数尚未注册");
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
                                OnException(RPCException.ErrorCode.Runtime, $"C#中的{args[i].GetType()}类型参数尚未注册");
                            }
                        }
                    }
                    else OnException(RPCException.ErrorCode.Runtime, $"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个");
                }
                ClientRequestModel request = new ClientRequestModel("2.0", Name, methodid.ToString(), obj);
                if (targetMethod.ReturnType == typeof(void))
                {
                    client.Send(request);
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
                        if (client.Send(request))
                        {
                            ClientResponseModel result = request.Get(timeout);
                            if (result != null)
                            {
                                if (result.Error != null)
                                {
                                    OnException(RPCException.ErrorCode.Runtime, $"ErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                                }
                                else if (Config.Types.TypesByName.TryGetValue(result.ResultType, out RPCType type))
                                {
                                    return type.Deserialize((string)result.Result);
                                }
                                else OnException(RPCException.ErrorCode.Runtime, $"C#中的{result.ResultType}类型转换器尚未注册");
                            }
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
        internal void OnClientException(Exception exception, SocketClient client)
        {
            OnException(exception);
        }

        internal void OnClientLog(RPCLog log, SocketClient client)
        {
            OnLog(log);
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
                logEvent(log, this);
            }
        }
        public void OnConnectSuccess()
        {
            ConnectSuccessEvent?.Invoke(this);
        }
        #endregion
    }
}
        