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
        public static bool Get(string netName, string serviceName, out Service service)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, serviceName, out service);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{netName}-{serviceName}Net未找到");
        }
        public static bool Get(Net net, string serviceName, out Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }
        public static Service Register(object instance, Net net, string servicename, RPCTypeConfig type)
        {
            return Register(instance,net, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>(Net net, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), net, servicename, config);
        }
        public static Service Register<T>(Net net, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(new T(), net, servicename, new ServiceConfig(type));
        }
        public static Service Register(object instance, Net net, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Service service);
            if(service == null)
            {
                try
                {
                    service = new Service();
                    service.Register(instance,net.Name,servicename,config);
                    net.Services[servicename] = service;
                    return service;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("发生异常报错,销毁注册\n" + e.StackTrace);
                    UnRegister(net,servicename);
                }
            }
            return null;
        }
        public static bool UnRegister(string netName, string serviceName)
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Core, $"{netName}Net未找到");
            }
            return UnRegister(net, serviceName);
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            return net.Services.TryRemove(serviceName, out Service value);
        }
    }
}
