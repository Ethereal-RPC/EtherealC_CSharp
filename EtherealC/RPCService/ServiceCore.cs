﻿using System;
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
                    Destory(servicename, hostname, port);
                }
            }
        }

        public static void Destory(string servicename, string hostname, string port)
        {
            services.Remove(new Tuple<string, string, string>(servicename,hostname,port), out Service value);
        }
        public static bool Get(string servicename, string hostname, string port ,out Service proxy)
        {
            return services.TryGetValue(new Tuple<string, string, string>(servicename, hostname, port), out proxy);
        }
        public static void ServerRequestReceive(string ip, string port,NetConfig config,ServerRequestModel request)
        {
            if (!services.TryGetValue(new Tuple<string, string, string>(request.Service, ip, port), out Service proxy) || !proxy.Methods.TryGetValue(request.MethodId, out MethodInfo method))
            {
#if DEBUG
                Console.WriteLine("------------------未找到该适配--------------------");
                Console.WriteLine($"{DateTime.Now}::{ip}:{port}::[客]\n{request}");
                Console.WriteLine("------------------未找到该适配--------------------");
#endif
                throw new RPCException(RPCException.ErrorCode.NotFoundService, request.Service + " ServerRequest Not Found!");
            }
            else
            {
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{ip}:{port}::[服-指令]\n{request}");
                Console.WriteLine("---------------------------------------------------------");
#endif
                proxy.ConvertParams(request.MethodId, request.Params);
                method.Invoke(proxy.Instance, request.Params);
            }
        }
    }
}