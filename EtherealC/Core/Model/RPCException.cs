﻿using EtherealC.NativeClient;
using EtherealC.RPCNet;
using EtherealC.RPCRequest;
using EtherealC.RPCService;
using System;

namespace EtherealC.Core.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class RPCException : Exception
    {
        #region --字段--
        public enum ErrorCode { Core, Runtime }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;
        private Net net;
        private Client client;
        private Service service;
        private Request request;
        private Exception exception;
        #endregion

        #region --属性--
        public ErrorCode Error { get => errorCode; set => errorCode = value; }
        public Net Net { get => net; set => net = value; }
        public Service Service { get => service; set => service = value; }
        public Request Request { get => request; set => request = value; }
        public Exception Exception { get => exception; set => exception = value; }
        public Client Client { get => client; set => client = value; }
        #endregion

        public RPCException(string message) : base(message)
        {
            exception = this;
        }
        public RPCException(Exception e) : base(e.Message)
        {
            exception = this;
            this.exception = e;
        }

        public RPCException(ErrorCode errorCode, string message) : base(message)
        {
            exception = this;
            this.errorCode = errorCode;
        }
    }
}
