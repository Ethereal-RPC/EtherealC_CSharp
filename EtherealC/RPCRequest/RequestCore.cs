using EtherealC.Model;
using EtherealC.RPCNet;
using System;
using System.Collections.Generic;

namespace EtherealC.RPCRequest
{
    public class RequestCore
    {
        #region --方法--
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string hostname, string port, string requestname,RPCTypeConfig type) where T : class
        {
            return Register<T>( hostname, port, requestname, new RequestConfig(type));
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string ip, string port, string servicename, RequestConfig config) where T : class
        {
            if (!NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
            }
            net.Requests.TryGetValue(servicename, out Request request);
            if (request == null)
            {
                Tuple<string, string> clientkey = new Tuple<string, string>(ip, port);
                request = Request.Register<T>(clientkey, servicename,config);
                net.Requests[servicename] = request;
            }
            return (T)(request as object);
        }
        public static bool Get(string ip, string port, string servicename, out Request reqeust)
        {
            if (NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                return net.Requests.TryGetValue(servicename, out reqeust);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
        }
        public static bool Get(Tuple<string, string, string> key, out Request reqeust)
        {
            if (NetCore.Get(new Tuple<string, string>(key.Item1, key.Item2), out Net net))
            {
                return net.Requests.TryGetValue(key.Item3, out reqeust);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}Net未找到");
        }
        public static bool UnRegister(Tuple<string, string, string> key)
        {
            if (NetCore.Get(new Tuple<string, string>(key.Item1, key.Item2), out Net net))
            {
                return net.Requests.Remove(key.Item3, out Request value);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}Net未找到");
        }
        #endregion
    }
}
