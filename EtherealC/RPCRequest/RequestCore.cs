using EtherealC.Model;
using System;
using System.Collections.Generic;
using EtherealC.RPCNet;

namespace EtherealC.RPCRequest
{
    public class RequestCore
    {

        #region --字段--
        private static Dictionary<Tuple<string, string, string>, object> requests { get; } = new Dictionary<Tuple<string, string, string>, object>();
        #endregion

        #region --属性--

        #endregion

        #region --方法--
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string servicename, string hostname, string port,RPCType type) where T : class
        {
            return Register<T>(servicename, hostname, port, new RequestConfig(type));
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string requestname, string hostname, string port, RequestConfig config) where T : class
        {
            T request = default(T);
            Tuple<string, string, string> key = new Tuple<string, string, string>(requestname, hostname, port);
            requests.TryGetValue(key, out object obj);
            request = (T)obj;
            if (request == null)
            {
                Tuple<string, string> clientkey = new Tuple<string, string>(hostname, port);
                request = Request.Register<T>(requestname, clientkey, config);
                requests[key] = request;
            }
            return request;
        }
        public static bool Get<T>(string servicename, string ip, string port,out T t) where T : class
        {
            return Get<T>(new Tuple<string, string, string>(servicename, ip, port),out t);
        }
        public static bool Get<T>(Tuple<string,string,string> key,out T t) where T : class
        {
            t = default(T);
            if (requests.TryGetValue(key, out object obj))
            {
                t = (T)obj;
                return true;
            }
            return false;
        }
        public static void Destory(Tuple<string, string, string> key)
        {
            requests.Remove(key, out object value);
        }
        #endregion
    }
}
