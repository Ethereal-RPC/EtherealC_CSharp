using System.Reflection;
using System.Text;
using EtherealC.Core.Model;
using EtherealC.Service.Abstract;

namespace EtherealC.Service.WebSocket
{

    public abstract class WebSocketService:Abstract.Service
    {
        #region --属性--
        public new WebSocketServiceConfig Config { get => (WebSocketServiceConfig)config; set => config = value; }

        #endregion

        #region --方法--
        
        #endregion

    }
}
