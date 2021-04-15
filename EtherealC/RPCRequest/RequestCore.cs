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
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string hostname, string port, string requestname,RPCType type) where T : class
        {
            return Register<T>( hostname, port, requestname, new RequestConfig(type));
        }
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="requestname">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string hostname, string port, string requestname, RequestConfig config) where T : class
        {
            T request = default(T);
            Tuple<string, string, string> key = new Tuple<string, string, string>(hostname, port, requestname);
            requests.TryGetValue(key, out object obj);
            request = (T)obj;
            if (request == null)
            {
                Tuple<string, string> clientkey = new Tuple<string, string>(hostname, port);
                request = Request.Register<T>(clientkey, requestname,config);
                requests[key] = request;
            }
            return request;
        }
        public static bool Get<T>(string ip, string port, string servicename, out T t) where T : class
        {
            return Get<T>(new Tuple<string, string, string>(ip, port,servicename),out t);
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
