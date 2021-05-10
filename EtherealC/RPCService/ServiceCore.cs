using EtherealC.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EtherealC.RPCService
{
    public class ServiceCore
    {
        public static Service Register(object instance, string ip, string port, string servicename, RPCTypeConfig type)
        {
            return Register(instance, ip, port, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>(string hostname, string port, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), hostname, port, servicename, config);
        }
        public static Service Register<T>(string hostname, string port, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(new T(), hostname, port, servicename, new ServiceConfig(type));
        }
        public static Service Register(object instance,string ip, string port, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentException("参数为空", nameof(ip));
            }

            if (string.IsNullOrEmpty(port))
            {
                throw new ArgumentException("参数为空", nameof(port));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }
            if (!NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
            }
            net.Services.TryGetValue(servicename, out Service service);
            if(service == null)
            {
                try
                {
                    SocketClient socketClient = ClientCore.Get(ip, port);
                    service = new Service();
                    service.Register(instance,new Tuple<string, string>(ip,port),servicename,config);
                    net.Services[servicename] = service;
                    return service;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("发生异常报错,销毁注册\n" + e.StackTrace);
                    UnRegister(new Tuple<string, string, string>(servicename, ip, port));
                }
            }
            return null;
        }
        public static bool UnRegister(Tuple<string, string, string> key)
        {
            if (!NetCore.Get(new Tuple<string, string>(key.Item1, key.Item2), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}Net未找到");
            }
            return net.Services.TryRemove(key.Item3, out Service value);
        }

        public static bool Get(string ip, string port, string servicename, out Service proxy)
        {
            if (!NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
            }
            return net.Services.TryGetValue(servicename, out proxy);
        }
    }
}
