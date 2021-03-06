using Castle.DynamicProxy;
using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Manager.Event.Attribute;
using EtherealC.Core.Model;
using EtherealC.Request.Attribute;
using EtherealC.Request.Event;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Abstract
{
    internal class RequestInterceptor : IInterceptor
    {
        private Random random = new Random();

        public void Intercept(IInvocation invocation)
        {
            MethodInfo method = invocation.Method;
            RequestMappingAttribute attribute = method.GetCustomAttribute<RequestMappingAttribute>();
            if (attribute == null)
            {
                invocation.Proceed();
                return;
            }
            Request instance = invocation.InvocationTarget as Request;
            object remoteResult = null;
            object localResult = null;
            object methodResult = null;
            ClientResponseModel result = null;
            EventSenderAttribute eventSender;
            EventContext eventContext;
            //注入参数
            ParameterInfo[] parameterInfos = method.GetParameters();
            ClientRequestModel request = new ClientRequestModel();
            request.Mapping = attribute.Mapping;
            request.Params = new Dictionary<string, string>(parameterInfos.Length);
            Dictionary<string, object>  @params = new Dictionary<string, object>(parameterInfos.Length);
            object[] args = invocation.Arguments;
            int idx = 0;
            foreach(ParameterInfo parameterInfo in parameterInfos)
            {
                instance.Types.Get(parameterInfo, out AbstractType type);
                request.Params.Add(parameterInfo.Name, type.Serialize(args[idx]));
                @params.Add(parameterInfo.Name, args[idx++]);
            }
            eventSender = method.GetCustomAttribute<BeforeEventAttribute>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
            if (attribute.InvokeType.HasFlag(RequestMappingAttribute.InvokeTypeFlags.Local))
            {
                try
                {
                    invocation.Proceed();
                    localResult = invocation.ReturnValue;
                }
                catch (Exception e)
                {
                    eventSender = method.GetCustomAttribute<ExceptionEventAttribute>();
                    if (eventSender != null)
                    {
                        (eventSender as ExceptionEventAttribute).Exception = e;
                        eventContext = new ExceptionEventContext(@params, method, e);
                        instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                        if ((eventSender as ExceptionEventAttribute).IsThrow) throw;
                    }
                    else throw;
                }
            }
            eventSender = method.GetCustomAttribute<AfterEventAttribute>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, localResult);
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
            if (attribute.InvokeType.HasFlag(RequestMappingAttribute.InvokeTypeFlags.Remote))
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
                                    instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                                }
                                else throw new TrackException(TrackException.ErrorCode.Runtime, $"来自服务器的报错消息:\nErrorCode:{result.Error.Code} Message:{result.Error.Message} Data:{result.Error.Data}");
                            }
                            instance.Types.Get(method.GetCustomAttribute<ParamAttribute>(true)?.Type,method.ReturnType,out AbstractType type);
                            remoteResult = type.Deserialize(result.Result);
                            eventSender = method.GetCustomAttribute<SuccessEvent>();
                            if (eventSender != null)
                            {
                                eventContext = new SuccessEventContext(@params, method, result.Result);
                                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                            }
                        }
                        else
                        {
                            eventSender = method.GetCustomAttribute<TimeoutEvent>();
                            if (eventSender != null)
                            {
                                eventContext = new TimeoutEventContext(@params, method);
                                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                            }
                        }
                    }
                    finally
                    {
                        instance.Tasks.Remove(id, out ClientRequestModel value);
                    }
                }
            }
            if ((attribute.InvokeType & RequestMappingAttribute.InvokeTypeFlags.ReturnRemote) != 0)
            {
                methodResult = remoteResult;
            }
            if ((attribute.InvokeType & RequestMappingAttribute.InvokeTypeFlags.ReturnLocal) != 0)
            {
                methodResult = localResult;
            }
            invocation.ReturnValue = methodResult;
        }
    }
}
