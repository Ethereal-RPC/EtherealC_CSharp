﻿using EtherealC.Core.Attribute;
using EtherealC.Core.Manager.AbstractType;
using EtherealC.Core.Model;
using EtherealC.Request.Interface;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealC.Request.Abstract
{
    public abstract class Request : Core.BaseCore.MZCore, IRequest
    {
        #region --委托--
        public delegate void OnConnnectSuccessDelegate(Request request);
        #endregion

        #region --事件字段--

        #endregion

        #region --事件属性--

        public event OnConnnectSuccessDelegate ConnectSuccessEvent;

        #endregion

        #region --字段--

        private Net.Abstract.Net net;
        protected RequestConfig config;
        private Dictionary<int, ClientRequestModel> tasks = new();
        private Client.Abstract.Client client;
        private Dictionary<string, Service.Abstract.Service> services = new Dictionary<string, Service.Abstract.Service>();

        #endregion

        #region --属性--

        public RequestConfig Config { get => config; set => config = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Client.Abstract.Client Client { get => client; set => client = value; }
        public Dictionary<string, Service.Abstract.Service> Services { get => services; set => services = value; }
        internal Dictionary<int, ClientRequestModel> Tasks { get => tasks; set => tasks = value; }

        #endregion

        #region -- 接口方法 --

        #endregion

        #region  -- 抽象接口 --
        public abstract void Initialize();
        public abstract void UnInitialize();
        public abstract void Register();
        public abstract void UnRegister();

        #endregion

        #region -- 普通方法 --
        internal static void Register(Request instance)
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.RequestMapping attribute = method.GetCustomAttribute<Attribute.RequestMapping>();
                if (attribute != null)
                {
                    if (method.ReturnType != typeof(void) && !instance.Types.Get(method.GetCustomAttribute<Param>()?.Type, method.ReturnType,out AbstractType type))
                    {
                        throw new TrackException(TrackException.ErrorCode.Core, $"{method.Name} 返回值未提供抽象类型方案");
                    }
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        BaseParam paramsAttribute = parameterInfo.GetCustomAttribute<BaseParam>(true);
                        if (paramsAttribute != null)
                        {
                            continue;
                        }
                        Param paramAttribute = parameterInfo.GetCustomAttribute<Param>();
                        if (paramAttribute != null && !instance.Types.Get(paramAttribute.Type, out type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{paramAttribute.Type}抽象类型未找到");
                        }
                        else if (!instance.Types.Get(parameterInfo.ParameterType, out type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{parameterInfo.ParameterType}类型映射抽象类型");
                        }
                    }
                }
            }
        }
        internal void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            if (Tasks.TryGetValue(int.Parse(response.Id), out ClientRequestModel model))
            {
                model.Set(response);
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}-{response.Id}返回的请求ID未找到!");
        }
        public void OnConnectSuccess()
        {
            ConnectSuccessEvent?.Invoke(this);
        }
        #endregion
    }
}
