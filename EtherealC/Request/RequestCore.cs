using EtherealC.Client;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Request.WebSocket;

namespace EtherealC.Request
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get<T>(string netName, string serviceName, out T reqeust)where T:Abstract.Request
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get<T>(net, serviceName, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }
        public static bool Get<T>(Net.Abstract.Net net, string serviceName, out T reqeust) where T : Abstract.Request
        {
            bool result = net.Requests.TryGetValue(serviceName, out Abstract.Request value);
            reqeust = (T)value;
            return result;
        }

        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static R Register<R,T>(Net.Abstract.Net net, string serviceName, AbstractTypes type) where R : Abstract.Request
        {
            if(net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register<R,T>(net, serviceName, new WebSocketRequestConfig(type));
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Request-Register处理");
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static R Register<R,T>(Net.Abstract.Net net, string servicename, WebSocketRequestConfig config) where R : Abstract.Request
        {
            net.Requests.TryGetValue(servicename, out Abstract.Request request);
            if (request == null)
            {
                request = Abstract.Request.Register<R, T>(net.Name, servicename, config);
                net.Requests[servicename] = request;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
            }
            return (R)request;
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
