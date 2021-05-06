using EtherealC.Model;
using EtherealC.NativeClient;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EtherealC.RPCService
{
    public class ServiceCore
    {
        private static Dictionary<Tuple<string, string, string>, Service> services { get; } = new Dictionary<Tuple<string, string, string>, Service>();

        public static Service Register<T>( string hostname, string port, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(new T(), hostname, port, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>(object instance,string hostname, string port, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(instance, hostname, port, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>( string hostname, string port, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), hostname, port, servicename, config);
        }
        public static Service Register(object instance,string hostname, string port, string servicename, ServiceConfig config)
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

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }
            Service service = null;
            Tuple<string, string, string> key = new Tuple<string, string, string>(hostname, port, servicename);
            services.TryGetValue(key,out service);
            if(service == null)
            {
                try
                {
                    SocketClient socketClient = ClientCore.Get(hostname, port);
                    service = new Service();
                    service.Register(instance,new Tuple<string, string>(hostname,port),servicename,config);
                    services[key] = service;
                    return service;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("发生异常报错,销毁注册\n" + e.StackTrace);
                    UnRegister(servicename, hostname, port);
                }
            }
            return null;
        }

        public static void UnRegister( string hostname, string port, string servicename)
        {
            services.Remove(new Tuple<string, string, string>(hostname,port,servicename), out Service value);
        }
        public static bool Get(string hostname, string port, string servicename, out Service service)
        {
            return services.TryGetValue(new Tuple<string, string, string>( hostname, port, servicename), out service);
        }
        public static bool Get(Tuple<string, string, string> key, out Service service)
        {
            return services.TryGetValue(key, out service);
        }
    }
}
