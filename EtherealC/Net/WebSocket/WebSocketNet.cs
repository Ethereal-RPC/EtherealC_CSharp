using System;
using System.Threading;
using EtherealC.Client;
using EtherealC.Client.Abstract;
using EtherealC.Client.WebSocket;
using EtherealC.Core.Model;
using EtherealC.Request;
using EtherealC.Service;

namespace EtherealC.Net.WebSocket
{
    public class WebSocketNet:Abstract.Net
    {
        #region --字段--
        private AutoResetEvent connectSign = new AutoResetEvent(false);
        #endregion

        #region --属性--
        public new WebSocketNetConfig Config { get => (WebSocketNetConfig)config; set => config = value; }

        #endregion

        #region --方法--
        public WebSocketNet(string name) : base(name)
        {
            config = new WebSocketNetConfig();
        }
        public override bool Publish()
        {
            try
            {
                Client?.Connect();
            }
            catch (TrackException e)
            {
                OnException(e);
            }
            catch (Exception e)
            {
                OnException((TrackException)e);
            }
            return true;
        }

        #endregion

        
    }
}
