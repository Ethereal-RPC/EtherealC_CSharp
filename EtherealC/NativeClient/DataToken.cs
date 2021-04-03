using System;
using System.Net.Sockets;
using System.Text;
using EtherealC.Model;
using EtherealC.RPCNet;
using Newtonsoft.Json;

namespace EtherealC.NativeClient
{
    /// <summary>
    /// Token for use with SocketAsyncEventArgs.
    /// </summary>
    public sealed class DataToken
    {
        //下面两部分只负责接收部分，发包构造部分并没有使用，修改时请注意！
        //下面这部分用于拆包分析   
        private static int headsize = 32;//头包长度
        private static int bodysize = 4;//数据大小长度
        private static int patternsize = 1;//消息类型长度
        private static int futuresize = 27;//后期看情况加
        //下面这部分的byte用于接收数据
        private static byte pattern;
        private static byte[] future = new byte[futuresize];
        Random random = new Random();
        private Tuple<string, string> clientKey;
        private SocketAsyncEventArgs socketArgs;
        private DotNetty.Buffers.IByteBuffer content;
        private int needRemain;
        private ClientConfig config;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="connection">Socket to accept incoming data.</param>
        /// <param name="capacity">Buffer size for accepted data.</param>
        public DataToken(SocketAsyncEventArgs eventArgs,Tuple<string, string> clientKey, ClientConfig config)
        {
            this.socketArgs = eventArgs;
            this.content = DotNetty.Buffers.UnpooledByteBufferAllocator.Default.DirectBuffer(eventArgs.Buffer.Length, 1024000);
            this.clientKey = clientKey;
            this.config = config;
            content.ResetWriterIndex();
        }
        public void ProcessData()
        {
            int writerIndex = socketArgs.BytesTransferred + socketArgs.Offset;
            int readerIndex = 0;
            while (readerIndex < writerIndex)
            {
                //存在断包
                if (needRemain != 0)
                {
                    //如果接收数据满足整条量
                    if (needRemain <= writerIndex - readerIndex)
                    {
                        content.WriteBytes(socketArgs.Buffer, readerIndex, needRemain);
                        string data = content.GetString(0, content.WriterIndex, Encoding.UTF8);
                        content.ResetWriterIndex();
                        readerIndex = needRemain + readerIndex;
                        needRemain = 0;
                        if (!NetCore.Get(clientKey, out NetConfig netConfig))
                        {
                            throw new RPCException(RPCException.ErrorCode.NotFoundNetConfig, "未找到NetConfig");
                        }

                        //0-Request 1-Response
                        if (pattern == 0)
                        {
                            ServerRequestModel request = JsonConvert.DeserializeObject<ServerRequestModel>(data);
                            netConfig.ServerRequestReceive(clientKey.Item1, clientKey.Item2, netConfig,
                                request);
                        }
                        else
                        {
                            ClientResponseModel response = JsonConvert.DeserializeObject<ClientResponseModel>(data);
                            netConfig.ClientResponseReceive(clientKey.Item1, clientKey.Item1, netConfig,
                                response);
                        }
                    }
                    else
                    {
                        int remain = writerIndex - readerIndex;
                        content.WriteBytes(socketArgs.Buffer, readerIndex, remain);
                        needRemain -= remain;
                        break;
                    }
                }
                else
                {
                    int remain = writerIndex - readerIndex;
                    //头包凑不齐，直接返回等待下一次数据
                    if (remain < headsize)
                    {
                        Buffer.BlockCopy(socketArgs.Buffer, readerIndex, socketArgs.Buffer, 0, remain);
                        socketArgs.SetBuffer(remain, socketArgs.Buffer.Length - remain);
                        return;
                    }
                    //收到头包，开始对头包拆分
                    //4个字节的数据大小
                    needRemain = BitConverter.ToInt32(socketArgs.Buffer, readerIndex);
                    //1个字节的接收方式
                    pattern = socketArgs.Buffer[readerIndex + bodysize];
                    //接收剩下的27个不用的字节
                    Buffer.BlockCopy(socketArgs.Buffer, readerIndex + bodysize + patternsize, future, 0, futuresize);
                    readerIndex += headsize;
                    continue;
                }
            }
            socketArgs.SetBuffer(0, socketArgs.Buffer.Length);
        }

    }
}
