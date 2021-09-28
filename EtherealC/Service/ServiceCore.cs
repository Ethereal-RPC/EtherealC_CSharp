using System;
using EtherealC.Core.Model;
using EtherealC.Net;
using EtherealC.Service.Abstract;
using EtherealC.Service.WebSocket;

namespace EtherealC.Service
{
    public class ServiceCore
    {
        public static bool Get<R>(string netName, string serviceName, out R service)where R:Abstract.Service
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get<R>(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool Get<R>(Net.Abstract.Net net, string serviceName, out R service) where R : Abstract.Service
        {
            bool result = net.Services.TryGetValue(serviceName, out Abstract.Service value);
            service = value as R;
            return result;
        }
        public static R Register<R>(Net.Abstract.Net net, string servicename, ServiceConfig config) where R : Abstract.Service, new()
        {
            return Register<R>(new R(), net, servicename, config);
        }
        public static R Register<R>(Net.Abstract.Net net, string servicename, AbstractTypes type) where R : Abstract.Service, new()
        {
            if (net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(new R(), net, servicename, new WebSocketServiceConfig(type));
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
            
        }
        public static R Register<R>(R instance, Net.Abstract.Net net, string servicename, AbstractTypes type) where R : Abstract.Service
        {
            if (net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(instance, net, servicename, new WebSocketServiceConfig(type));
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
        }
        public static R Register<R>(R instance, Net.Abstract.Net net, string servicename, ServiceConfig config) where R:Abstract.Service
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Abstract.Service service);
            if(service == null)
            {
                Abstract.Service.Register(instance, net.Name, servicename, config);
                net.Services[servicename] = instance;
                instance.LogEvent += net.OnLog;
                instance.ExceptionEvent += net.OnException;
                return instance;
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
