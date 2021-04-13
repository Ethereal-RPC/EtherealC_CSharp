using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using Newtonsoft.Json;

namespace EtherealC.RPCService
{
    public class ServiceCore
    {
        private static Dictionary<Tuple<string, string, string>, Service> services { get; } = new Dictionary<Tuple<string, string, string>, Service>();

        public static void Register<T>(string servicename, string hostname, string port,RPCType type) where T : new()
        {
            Register(new T(), servicename, hostname, port, new ServiceConfig(type));
        }
        public static void Register<T>(object instance,string servicename, string hostname, string port, RPCType type) where T : new()
        {
            Register(instance, servicename, hostname, port, new ServiceConfig(type));
        }
        public static void Register<T>(string servicename, string hostname, string port, ServiceConfig config) where T : new()
        {
            Register(new T(), servicename, hostname, port, config);
        }
        public static void Register(object instance,string servicename,string hostname, string port, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException("参数为空", nameof(hostname));
            }

            if (string.IsNullOrEmpty(port))
            {
                throw new ArgumentException("参数为空", nameof(port));
            }

            if (config.Type is null)
            {
                throw new ArgumentNullException(nameof(config.Type));
            }
            Service service = null;
            Tuple<string, string, string> key = new Tuple<string, string, string>(servicename, hostname, port);
            services.TryGetValue(key,out service);
            if(service == null)
            {
                try
                {
                    SocketClient socketClient = ClientCore.Get(hostname, port);
                    service = new Service();
                    service.Register(instance,config);
                    services[key] = service;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("发生异常报错,销毁注册\n" + e.StackTrace);
                    UnRegister(servicename, hostname, port);
                }
            }
        }

        public static void UnRegister(string servicename, string hostname, string port)
        {
            services.Remove(new Tuple<string, string, string>(servicename,hostname,port), out Service value);
        }
        public static bool Get(string servicename, string hostname, string port ,out Service service)
        {
            return services.TryGetValue(new Tuple<string, string, string>(servicename, hostname, port), out service);
        }
        public static bool Get(Tuple<string, string, string> key, out Service service)
        {
            return services.TryGetValue(key, out service);
        }
    }
}
