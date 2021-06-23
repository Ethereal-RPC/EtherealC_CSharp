using EtherealC.Model;
using EtherealC.RPCNet;
using System;
using System.Collections.Generic;

namespace EtherealC.RPCRequest
{
    public class RequestCore
    {
        #region --方法--

        public static bool Get(string netName, string servicename, out Request reqeust)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, servicename, out reqeust);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{netName}Net未找到");
        }
        public static bool Get(Net net, string servicename, out Request reqeust)
        {
            return net.Requests.TryGetValue(servicename, out reqeust);
        }

        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string netName, string requestname,RPCTypeConfig type) where T : class
        {
            return Register<T>(netName, requestname, new RequestConfig(type));
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string netName, string servicename, RequestConfig config) where T : class
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Core, $"{netName} Net未找到");
            }
            net.Requests.TryGetValue(servicename, out Request request);
            if (request == null)
            {
                request = Request.Register<T>(netName, servicename,config);
                net.Requests[servicename] = request;
            }
            return (T)(request as object);
        }

        public static bool UnRegister(string netName, string serviceName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net, serviceName);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{netName}Net未找到");
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            return net.Requests.Remove(serviceName, out Request value);
        }
        #endregion
    }
}
