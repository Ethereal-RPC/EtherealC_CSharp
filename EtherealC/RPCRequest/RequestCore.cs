using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using System;
using System.Collections.Generic;

namespace EtherealC.RPCRequest
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get(string netName, string requestName, out Request reqeust)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, requestName, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }
        public static bool Get(Net net, string requestName, out Request reqeust)
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
        public static T Register<T>(Net net, string requestname,RPCTypeConfig type) where T : class
        {
            return Register<T>(net, requestname, new RequestConfig(type));
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(Net net, string servicename, RequestConfig config) where T : class
        {
            net.Requests.TryGetValue(servicename, out Request request);
            if (request == null)
            {
                request = Request.Register<T>(net.Name, servicename,config);
                net.Requests[servicename] = request;
                request.LogEvent += net.OnRequestLog;
                request.ExceptionEvent += net.OnRequestException;
            }
            return (T)(request as object);
        }

        public static bool UnRegister(string netName, string serviceName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            if(net != null)
            {
                net.Requests.Remove(serviceName, out Request request);
                request.LogEvent -= net.OnRequestLog;
                request.ExceptionEvent -= net.OnRequestException;
                ClientCore.UnRegister(request);
            }
            return true;
        }
        #endregion
    }
}
