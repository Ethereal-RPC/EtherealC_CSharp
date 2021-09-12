namespace EtherealC.RPCNet.NetNodeClient.Model
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
        private string[] prefixes;
        /// <summary>
        /// 硬件信息
        /// </summary>
        private HardwareInformation hardwareInformation;
        #endregion

        #region --属性--

        public string Name { get => name; set => name = value; }
        public long Connects { get => connects; set => connects = value; }
        public HardwareInformation HardwareInformation { get => hardwareInformation; set => hardwareInformation = value; }
        public string[] Prefixes { get => prefixes; set => prefixes = value; }


        #endregion

    }
}
