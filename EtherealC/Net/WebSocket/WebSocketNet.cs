using System.Threading;

namespace EtherealC.Net.WebSocket
{
    public class WebSocketNet : Abstract.Net
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

        #endregion


    }
}
