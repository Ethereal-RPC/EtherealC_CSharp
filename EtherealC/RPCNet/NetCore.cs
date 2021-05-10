﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using EtherealC.Model;
using EtherealC.RPCRequest;
using EtherealC.RPCService;

namespace EtherealC.RPCNet
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class NetCore
    {
        private static Dictionary<Tuple<string, string>, Net> nets { get; } = new Dictionary<Tuple<string, string>, Net>();
        public static bool Get(Tuple<string, string> key, out Net net)
        {
            return nets.TryGetValue(key, out net);
        }
        public static Net Register(string ip, string port)
        {
            return Register(ip, port, new NetConfig());
        }
        public static Net Register(string ip, string port, NetConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(new Tuple<string, string>(ip, port), out Net net))
            {
                net = new Net();
                net.Config = config;
                nets.Add(new Tuple<string, string>(ip, port), net);
                return net;
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}服务的NetConfig已经注册");
        }
        public static bool UnRegister(string ip, string port)
        {
            return nets.Remove(new Tuple<string, string>(ip, port));
        }
    }
}
