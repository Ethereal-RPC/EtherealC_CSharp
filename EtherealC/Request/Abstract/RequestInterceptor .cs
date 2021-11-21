using Castle.DynamicProxy;
using EtherealC.Core.Attribute;
using EtherealC.Core.Event.Attribute;
using EtherealC.Core.Model;
using EtherealC.Request.Attribute;
using EtherealC.Request.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealC.Request.Abstract
{
    internal class RequestInterceptor : IInterceptor
    {
        private Random random = new Random();
        public void Intercept(IInvocation invocation)
        {
            MethodInfo method = invocation.Method;
            RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
            if (attribute == null)
            {
                invocation.Proceed();
                return;
            }
            Request instance = invocation.InvocationTarget as Request;
            object remoteResult = null;
            object localResult = null;
            object methodResult = null;
            ClientRequestModel request = null;
            Dictionary<string, object> @params = null;
            ClientResponseModel result = null;
            EventSender eventSender;
            EventContext eventContext;
            //注入参数
            ParameterInfo[] parameterInfos = method.GetParameters();
            request = new ClientRequestModel();
            request.Mapping = attribute.Mapping;
            request.Params = new string[parameterInfos.Length];
            @params = new(parameterInfos.Length);
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                Param paramAttribute = parameterInfos[i].GetCustomAttribute<Param>(true);
                if ((paramAttribute != null && instance.Types.TypesByName.TryGetValue(paramAttribute.Name, out AbstractType type)) || instance.Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out type))
                {
                    request.Params[i] = type.Serialize(invocation.Arguments[i]);
                    @params.Add(parameterInfos[i].Name, invocation.Arguments[i]);
                }
                else throw new TrackException($"{method.Name}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
            }
            eventSender = method.GetCustomAttribute<BeforeEvent>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
            if (attribute.InvokeType.HasFlag(RequestMapping.InvokeTypeFlags.Local))
            {
                try
                {
                    invocation.Proceed();
                    localResult = invocation.ReturnValue;
                }
                catch (Exception e)
                {
                    eventSender = method.GetCustomAttribute<ExceptionEvent>();
                    if (eventSender != null)
                    {
                        (eventSender as ExceptionEvent).Exception = e;
                        eventContext = new ExceptionEventContext(@params, method, e);
                        instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                        if ((eventSender as ExceptionEvent).IsThrow) throw;
                    }
                    else throw;
                }
            }
            if (attribute.InvokeType.HasFlag(RequestMapping.InvokeTypeFlags.Remote))
            {
                if (method.ReturnType == typeof(void))
                {
                    instance.Client.SendClientRequestModel(request);
                }
                else
                {
                    int id = random.Next();
                    while (instance.Tasks.TryGetValue(id, out ClientRequestModel value))
                    {
                        id = random.Next();
                    }
                    instance.Tasks.TryAdd(id, request);
                    try
                    {
                        request.Id = id.ToString();
                        int timeout = instance.Config.Timeout;
                        if (attribute.Timeout != -1) timeout = attribute.Timeout;
                        instance.Client.SendClientRequestModel(request);
                        result = request.Get(timeout);
                        if (result != null)
                        {
                            if (result.Error != null)
                            {
                                eventSender = method.GetCustomAttribute<FailEvent>();
                                if (eventSender != null)
                                {
                                    eventContext = new FailEventContext(@params, method, result.Error);
                                    instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Runtime, $"来自服务器的报错消息:\nErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                            }
                            Param abstractTypeAttribute = method.GetCustomAttribute<Param>(true);
                            if ((abstractTypeAttribute != null && instance.Types.TypesByName.TryGetValue(abstractTypeAttribute.Name, out AbstractType type))
                                || instance.Types.TypesByType.TryGetValue(method.ReturnType, out type))
                            {
                                remoteResult = type.Deserialize(result.Result);
                                eventSender = method.GetCustomAttribute<SuccessEvent>();
                                if (eventSender != null)
                                {
                                    eventContext = new SuccessEventContext(@params, method, result.Result);
                                    instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                                }
                            }
                            else throw new TrackException($"{method.Name}方法中的{method.ReturnType}类型参数尚未注册");
                        }
                        else
                        {
                            eventSender = method.GetCustomAttribute<TimeoutEvent>();
                            if (eventSender != null)
                            {
                                eventContext = new TimeoutEventContext(@params, method);
                                instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                            }
                        }
                    }
                    finally
                    {
                        instance.Tasks.Remove(id, out ClientRequestModel value);
                    }
                }
            }
            if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.ReturnRemote) != 0)
            {
                methodResult = remoteResult;
            }
            if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.ReturnLocal) != 0)
            {
                methodResult = localResult;
            }
            eventSender = method.GetCustomAttribute<AfterEvent>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, methodResult);
                instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
            invocation.ReturnValue = methodResult;
        }
    }
}
