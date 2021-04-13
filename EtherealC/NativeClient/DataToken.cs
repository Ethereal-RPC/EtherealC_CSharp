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
        private int dynamicAdjustBufferCount = -1;
        //下面两部分只负责接收部分，发包构造部分并没有使用，修改时请注意！
        //下面这部分用于拆包分析   
        private static int headsize = 32;//头包长度
        private static int bodysize = 4;//数据大小长度
        private static int patternsize = 1;//消息类型长度
        private static int futuresize = 27;//后期看情况加
        Random random = new Random();
        private Tuple<string, string> clientKey;
        private SocketAsyncEventArgs socketArgs;
        private DotNetty.Buffers.IByteBuffer buffer;
        private ClientConfig config;


        public SocketAsyncEventArgs SocketArgs { get => socketArgs; set => socketArgs = value; }


        public DataToken(Tuple<string, string> clientKey, ClientConfig config)
        {
            this.SocketArgs = new SocketAsyncEventArgs();
            this.clientKey = clientKey;
            this.config = config;
        }
        public void Connect(Socket socket)
        {
            buffer = DotNetty.Buffers.UnpooledByteBufferAllocator.Default.DirectBuffer(config.BufferSize, config.MaxBufferSize);
            SocketArgs.AcceptSocket = socket;
            socketArgs.SetBuffer(buffer.Array,0,buffer.Capacity);
        }

        public void DisConnect()
        {
            buffer = null;
            socketArgs.SetBuffer(null);
        }

        public void ProcessData()
        {
            buffer.ResetReaderIndex();
            buffer.SetWriterIndex(SocketArgs.BytesTransferred + buffer.WriterIndex);
            while (buffer.ReaderIndex < buffer.WriterIndex)
            {
                //数据包大小
                int count = buffer.WriterIndex - buffer.ReaderIndex;
                //凑够头包
                if (headsize < count)
                {
                    //Body数据长度 4 字节
                    int body_length = BitConverter.ToInt32(buffer.Array, buffer.ReaderIndex);
                    //请求方式 1 字节
                    byte pattern = buffer.Array[buffer.ReaderIndex + bodysize];
                    //未来用 27 字节
                    byte[] future = new byte[futuresize];
                    Buffer.BlockCopy(buffer.Array, buffer.ReaderIndex + bodysize + patternsize, future, 0, futuresize);
                    //数据总长
                    int length = body_length + headsize;
                    //判断Body数据是否足够
                    if (length <= count)
                    {
                        try
                        {
                            string data = buffer.GetString(buffer.ReaderIndex + headsize, body_length, config.Encoding);
                            if (!NetCore.Get(clientKey, out NetConfig netConfig))
                            {
                                throw new RPCException(RPCException.ErrorCode.RuntimeError, "未找到NetConfig");
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
                                netConfig.ClientResponseReceive(clientKey.Item1, clientKey.Item2, netConfig,
                                    response);
                            }
                            buffer.SetReaderIndex(buffer.ReaderIndex + length);
                        }
                        catch
                        {
                            throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{clientKey}-{SocketArgs.RemoteEndPoint}:用户数据错误，已自动断开连接！");
                        }
                    }
                    else
                    {
                        //迁移数据至缓冲区头
                        if (buffer.ReaderIndex != 0)
                        {
                            Buffer.BlockCopy(buffer.Array, 0, buffer.Array, buffer.ReaderIndex, count);
                            buffer.ResetReaderIndex();
                            buffer.SetWriterIndex(count);
                        }
                        //数据还有一些未接收到，原因可能是前面已经有了数据，也可能是本身大小不够
                        if (length > buffer.Capacity)
                        {
                            if (length < buffer.MaxCapacity)
                            {
                                buffer.AdjustCapacity(length);
                                dynamicAdjustBufferCount = config.DynamicAdjustBufferCount;
                                SocketArgs.SetBuffer(buffer.Array, buffer.WriterIndex, buffer.Capacity - count);
                                return;
                            }
                            else
                            {
                                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{clientKey}-{SocketArgs.RemoteEndPoint}:用户请求数据量太大，中止接收！");
                            }
                        }
                        SocketArgs.SetBuffer(buffer.WriterIndex, buffer.Capacity - count);
                        return;
                    }
                }
                else
                {
                    //头包凑不够，迁移数据至缓冲区头
                    if (buffer.ReaderIndex != 0)
                    {
                        Buffer.BlockCopy(buffer.Array, 0, buffer.Array, buffer.ReaderIndex, buffer.WriterIndex - buffer.ReaderIndex);
                        buffer.ResetReaderIndex();
                        buffer.SetWriterIndex(count);
                    }
                    SocketArgs.SetBuffer(buffer.WriterIndex, buffer.Capacity - count);
                    return;
                }
            }
            buffer.ResetReaderIndex();
            buffer.ResetWriterIndex();
            //充分退出，说明可能存在一定的空间浪费
            if (buffer.Capacity > config.BufferSize && dynamicAdjustBufferCount != -1 && dynamicAdjustBufferCount-- == 0)
            {
                buffer.AdjustCapacity(config.BufferSize);
                SocketArgs.SetBuffer(buffer.Array, 0, buffer.Capacity);
            }
            else SocketArgs.SetBuffer(0, buffer.Capacity);
        }
    }
}
