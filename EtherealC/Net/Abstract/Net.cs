using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EtherealC.Core;
using EtherealC.Core.Model;
using EtherealC.Net.Interface;

namespace EtherealC.Net.Abstract
{
    public abstract class Net:INet
    {
        public enum NetType { WebSocket }
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
        protected ConcurrentDictionary<string, Service.Abstract.Service> services = new ConcurrentDictionary<string, Service.Abstract.Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected Dictionary<string, Request.Abstract.Request> requests = new Dictionary<string, Request.Abstract.Request>();
        protected NetType type = NetType.WebSocket;
        protected NetConfig config;
        #endregion

        #region --属性--
        public ConcurrentDictionary<string, Service.Abstract.Service> Services { get => services; set => services = value; }
        public Dictionary<string, Request.Abstract.Request> Requests { get => requests; set => requests = value; }
        public string Name { get => name; set => name = value; }
        public NetType Type { get => type; set => type = value; }
        public NetConfig Config { get => config; set => config = value; }

        #endregion

        #region --方法--

        public Net(string name)
        {
            this.name = name;
        }
        public abstract bool Publish();

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            e.Net = this;
            exceptionEvent?.Invoke(e);
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }

        public void OnLog(TrackLog log)
        {
            logEvent?.Invoke(log);
        }

        public void ServerRequestReceiveProcess(ServerRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service.Abstract.Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string[] param_id = request.MethodId.Split('-');
                    for (int i = 1, j = 0; i < param_id.Length; i++, j++)
                    {
                        if (service.Types.TypesByName.TryGetValue(param_id[i], out AbstractType type))
                        {
                            request.Params[j] = type.Deserialize((string)request.Params[j]);
                        }
                        else throw new TrackException(TrackException.ErrorCode.Runtime,$"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                    }
                    method.Invoke(service, request.Params);
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{request.Service}-{request.MethodId}未找到!");
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{request.Service} 未找到!");
        }
        public void ClientResponseReceiveProcess(ClientResponseModel response)
        {
            if (int.TryParse(response.Id, out int id) && Requests.TryGetValue(response.Service, out Request.Abstract.Request request))
            {
                if (request.GetTask(id, out ClientRequestModel model))
                {
                    model.Set(response);
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{response.Service}-{id}返回的请求ID未找到!");
            }
            else
            {
                if(response.Error != null)
                {
                    throw new TrackException(TrackException.ErrorCode.Runtime, $"Server:\n{response.Error}");
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{response.Service}未找到!");
            }
        }
        #endregion
    }
}
