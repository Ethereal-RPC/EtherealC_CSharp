using EtherealC.Core.Model;
using EtherealC.NativeClient;
using EtherealC.RPCNet.NetNodeClient.Model;
using EtherealC.RPCNet.NetNodeClient.Request;
using EtherealC.RPCNet.NetNodeClient.Service;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace EtherealC.RPCNet
{
    public interface IBaseNet
    {
        #region --方法--
        public bool Publish();
        public void ServerRequestReceiveProcess(ServerRequestModel request);
        public void ClientResponseReceiveProcess(ClientResponseModel response);
        #endregion
    }
}
