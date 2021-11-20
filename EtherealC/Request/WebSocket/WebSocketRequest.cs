﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealC.Core.Model;
using Newtonsoft.Json;

namespace EtherealC.Request.WebSocket
{
    public abstract class WebSocketRequest:Abstract.Request
    {
        #region --字段--


        #endregion


        #region --属性--

        public new WebSocketRequestConfig Config { get => (WebSocketRequestConfig)config; set => config = value; }

        #endregion


        #region --方法--

        public WebSocketRequest()
        {
            Config = new WebSocketRequestConfig();
        }


        #endregion
    }
}
        