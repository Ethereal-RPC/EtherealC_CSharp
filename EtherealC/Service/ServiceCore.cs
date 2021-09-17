using System;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Service.Abstract;
using EtherealC.Service.WebSocket;

namespace EtherealC.Service
{
    public class ServiceCore
    {
        public static bool Get(string netName, string serviceName, out Service.Abstract.Service service)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool Get(Net.Abstract.Net net, string serviceName, out Service.Abstract.Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }
        public static WebSocketService Register(object instance, Net.Abstract.Net net, string servicename, AbstractTypes type)
        {
            if(net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(instance, net, servicename, new WebSocketServiceConfig(type));
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
        }
        public static WebSocketService Register<T>(Net.Abstract.Net net, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), net, servicename, config);
        }
        public static WebSocketService Register<T>(Net.Abstract.Net net, string servicename, AbstractTypes type) where T : new()
        {
            if (net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(new T(), net, servicename, new WebSocketServiceConfig(type));
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
            
        }
        public static WebSocketService Register(object instance, Net.Abstract.Net net, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Service.Abstract.Service service);
            if(service == null)
            {
                if (net.Type == Net.Abstract.Net.NetType.WebSocket)
                {
                    service = new WebSocketService();
                }
                else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
                service.Register(instance, net.Name, servicename, config);
                net.Services[servicename] = service;
                service.LogEvent += net.OnLog;
                service.ExceptionEvent += net.OnException;
                return service as WebSocketService;
            }
            return null;
        }
        public static bool UnRegister(string netName, string serviceName)
        {
            if (!NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if (net != null)
            {
                net.Services.TryRemove(serviceName, out Service.Abstract.Service service);
                if(service != null)
                {
                    service.LogEvent -= net.OnLog;
                    service.ExceptionEvent -= net.OnException;
                }
            }
            return true;
        }
    }
}
