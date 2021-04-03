﻿using System;

namespace EtherealC.Model
{
    public class RPCException : Exception
    {
        public enum ErrorCode { Main, Intercepted,NotFoundService, RegisterError, NotFoundNetConfig }

        private ErrorCode errorCode;

        public ErrorCode Error { get => errorCode; set => errorCode = value; }

        public RPCException(string message) : base(message)
        {

        }

        public RPCException(ErrorCode errorCode,string message):base(message)
        {
            this.errorCode = errorCode;
        }
    }
}
