﻿using EtherealC.Core.Model;
using EtherealC.Request.Interface;

namespace EtherealC.Request.Abstract
{
    public abstract class RequestConfig: IRequestConfig
    {

        #region --字段--
        private bool tokenEnable = true;
        private AbstractTypes types;
        private int timeout = -1;

        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public int Timeout { get => timeout; set => timeout = value; }
        #endregion

        #region --方法--
        public RequestConfig(AbstractTypes types)
        {
            this.types = types;
        }

        #endregion
    }
}