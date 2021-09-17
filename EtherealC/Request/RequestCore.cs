using EtherealC.Client;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request.WebSocket;

namespace EtherealC.Request
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get(string netName, string requestName, out Request.Abstract.Request reqeust)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, requestName, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }
        public static bool Get(Net.Abstract.Net net, string requestName, out Request.Abstract.Request reqeust)
        {
            return net.Requests.TryGetValue(requestName, out reqeust);
        }

        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(Net.Abstract.Net net, string requestname,RPCTypeConfig type) where T : class
        {
            if(net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register<T>(net, requestname, new WebSocketRequestConfig(type));
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.Type}的Request-Register处理");
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(Net.Abstract.Net net, string servicename, WebSocketRequestConfig config) where T : class
        {
            net.Requests.TryGetValue(servicename, out Request.Abstract.Request request);
            if (request == null)
            {
                if(net.Type == Net.Abstract.Net.NetType.WebSocket)
                {
                    request = WebSocketRequest.Register<T>(net.Name, servicename, config);
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.Type}的Request-Register处理");
                net.Requests[servicename] = request;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
            }
            return (T)(request as object);
        }

        public static bool UnRegister(string netName, string serviceName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if(net != null)
            {
                net.Requests.Remove(serviceName, out Request.Abstract.Request request);
                request.LogEvent -= net.OnLog;
                request.ExceptionEvent -= net.OnException;
                ClientCore.UnRegister(request);
            }
            return true;
        }
        #endregion
    }
}
