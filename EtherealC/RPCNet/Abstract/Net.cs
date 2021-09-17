﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EtherealC.Core.Delegates;
using EtherealC.Core.Model;
using EtherealC.RPCNet.Interface;
using EtherealC.RPCRequest;
using EtherealC.RPCRequest.Abstract;
using EtherealC.RPCService;
using EtherealC.RPCService.Abstract;
using EtherealC.Core.Enums;

namespace EtherealC.RPCNet.Abstract
{
    public abstract class Net:INet
    {

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
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
        /// <summary>
        /// Net网关名
        /// </summary>
        protected string name;
        /// <summary>
        /// Service映射表
        /// </summary>
        protected ConcurrentDictionary<string, Service> services = new ConcurrentDictionary<string, Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected Dictionary<string, Request> requests = new Dictionary<string, Request>();
        protected NetType netType = NetType.WebSocket;
        protected NetConfig config;
        #endregion

        #region --属性--
        public ConcurrentDictionary<string, Service> Services { get => services; set => services = value; }
        public Dictionary<string, Request> Requests { get => requests; set => requests = value; }
        public string Name { get => name; set => name = value; }
        public NetType NetType { get => netType; set => netType = value; }
        public NetConfig Config { get => config; set => config = value; }

        #endregion

        #region --方法--

        public abstract bool Publish();

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(RPCException e)
        {
            e.Net = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }

        public void OnLog(RPCLog log)
        {
            logEvent?.Invoke(log);
        }

        public void ServerRequestReceiveProcess(ServerRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string log = "";
                    log += "---------------------------------------------------------\n";
                    log += $"{DateTime.Now}::{name}::[服-指令]\n{request}\n";
                    log += "---------------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime,log);
                    string[] param_id = request.MethodId.Split('-');
                    for (int i = 1, j = 0; i < param_id.Length; i++, j++)
                    {
                        if (service.Config.Types.TypesByName.TryGetValue(param_id[i], out RPCType type))
                        {
                            request.Params[j] = type.Deserialize((string)request.Params[j]);
                        }
                        else throw new RPCException(RPCException.ErrorCode.Runtime,$"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                    }
                    method.Invoke(service, request.Params);
                }
                else throw new RPCException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service}-{request.MethodId}未找到!");
            }
            else throw new RPCException(RPCException.ErrorCode.Runtime, $"{name}-{request.Service} 未找到!");
        }
        public void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            string log = "";
            log += "---------------------------------------------------------\n";
            log += $"{DateTime.Now}::{name}::[服-返回]\n{response}\n";
            log += "---------------------------------------------------------\n";
            OnLog(RPCLog.LogCode.Runtime, log);
            if (int.TryParse(response.Id, out int id) && Requests.TryGetValue(response.Service, out Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
                else throw new RPCException(RPCException.ErrorCode.Runtime, $"{name}-{response.Service}-{id}返回的请求ID未找到!");
            }
            else
            {
                if(response.Error != null)
                {
                    throw new RPCException(RPCException.ErrorCode.Runtime, $"Server:\n{response.Error}");
                }
                else throw new RPCException(RPCException.ErrorCode.Runtime, $"{name}-{response.Service}未找到!");
            }
        }
        #endregion
    }
}
