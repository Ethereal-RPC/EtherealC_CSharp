namespace EtherealC.RPCNet.Client.Model
{
    public class NetNode
    {
        #region --字段--
        /// <summary>
        /// Net节点名
        /// </summary>
        private string name;
        /// <summary>
        /// 连接数量
        /// </summary>
        private long connects;
        /// <summary>
        /// ip地址
        /// </summary>
        private string ip;
        /// <summary>
        /// port地址
        /// </summary>
        private string port;
        /// <summary>
        /// 硬件信息
        /// </summary>
        private HardwareInformation hardwareInformation;
        #endregion

        #region --属性--

        public string Name { get => name; set => name = value; }
        public long Connects { get => connects; set => connects = value; }
        public string Ip { get => ip; set => ip = value; }
        public HardwareInformation HardwareInformation { get => hardwareInformation; set => hardwareInformation = value; }
        public string Port { get => port; set => port = value; }


        #endregion

    }
}
