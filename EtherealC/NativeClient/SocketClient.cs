using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EtherealC.Model;
using EtherealC.RPCNet;
using Newtonsoft.Json;

namespace EtherealC.NativeClient
{

    public sealed class SocketClient : IDisposable
    {
        IPEndPoint hostEndPoint;
        /// <summary>
        /// 信号量,控制线程进行连接等待
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        private ClientConfig config;
        private Tuple<string, string> clientKey;
        private SocketAsyncEventArgs socketArgs;
        private Random random = new Random();
        public SocketAsyncEventArgs SocketArgs { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public SocketClient(Tuple<string,string> clientKey, ClientConfig config)
        {
            this.clientKey = clientKey;
            this.config = config;
            this.SocketArgs = new SocketAsyncEventArgs();
            // Get host related information.
            IPAddress[] addressList = Dns.GetHostEntry(clientKey.Item1).AddressList;
            // Instantiates the endpoint and socket.
            hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], int.Parse(clientKey.Item2));
        }

        public void Start()
        {
            try
            {
                SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += OnConnect;
                Socket socket = new Socket(hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                acceptArgs.RemoteEndPoint = hostEndPoint;
                acceptArgs.AcceptSocket = socket;
                socket.ConnectAsync(acceptArgs);
                autoConnectEvent.WaitOne();
                SocketError errorCode = acceptArgs.SocketError;
                if (errorCode == SocketError.Success)
                {
                    SocketArgs = new SocketAsyncEventArgs();
                    SocketArgs.Completed += OnReceiveCompleted;
                    SocketArgs.SetBuffer(new Byte[config.BufferSize], 0, config.BufferSize);
                    SocketArgs.AcceptSocket = acceptArgs.AcceptSocket;
                    SocketArgs.UserToken = new DataToken(SocketArgs, clientKey, config);
                    SocketArgs.RemoteEndPoint = hostEndPoint;
                    SocketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                    if (!SocketArgs.AcceptSocket.ReceiveAsync(SocketArgs))
                    {
                        ProcessReceive(SocketArgs);
                    }
                }
                else
                {
                    Reconnect();
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("连接服务器失败，尝试重连" + e.StackTrace);
                Reconnect();
            }
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
        }

        public void Disconnect()
        {
            SocketArgs.AcceptSocket.Disconnect(false);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive(e);
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    (e.UserToken as DataToken).ProcessData();
                    if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        // Read the next block of data sent by client.
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    Reconnect();
                }
            }
            else
            {
                Reconnect();
            }
        }
        public bool Reconnect()
        {
            Debug.WriteLine("与服务器连接异常,开始尝试重连！");
            Socket clientSocket = SocketArgs.AcceptSocket;
            for (int i = 1; i <= 10; i++)
            {
                if (clientSocket != null)
                {
                    Debug.WriteLine("开始销毁历史Socket");
                    clientSocket.Close();
                    clientSocket.Dispose();
                    Debug.WriteLine("历史Socket销毁完成！");
                }
                Debug.WriteLine($"开始进行第{i}次尝试");
                clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += OnConnect;
                acceptArgs.AcceptSocket = clientSocket;
                acceptArgs.RemoteEndPoint = hostEndPoint;
                try
                {
                    clientSocket.ConnectAsync(acceptArgs);
                    autoConnectEvent.WaitOne();
                    SocketError errorCode = acceptArgs.SocketError;
                    if (errorCode == SocketError.Success)
                    {
                        SocketArgs = new SocketAsyncEventArgs();
                        SocketArgs.SetBuffer(new byte[config.BufferSize], 0, config.BufferSize);
                        SocketArgs.Completed += OnReceiveCompleted;
                        SocketArgs.UserToken = new DataToken(SocketArgs,clientKey, config);
                        if (!clientSocket.ReceiveAsync(SocketArgs))
                        {
                            ProcessReceive(SocketArgs);
                        }
                        Debug.WriteLine("重连成功！");
                        break;
                    }
                    else
                    {
                        Debug.WriteLine("重连失败，5秒后重试！");
                        Thread.Sleep(5000);
                    }
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("重连失败，5秒后重试！" + e.StackTrace);
                    Thread.Sleep(5000);
                }
            }
            if (!clientSocket.Connected)
            {
                Debug.WriteLine($"重连失败！");
                return false;
            }
            else return true;
        }
        public void Send(ClientRequestModel request)
        {
            if (socketArgs.AcceptSocket != null && socketArgs.AcceptSocket.Connected)
            {
#if DEBUG
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{clientKey.Item1}:{clientKey.Item2}::[客-请求]\n{request}");
                Console.WriteLine("---------------------------------------------------------");
#endif
                int id = random.Next();
                //构造data数据
                byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                //构造消息类型 0 为Respond,1 为Request
                byte[] pattern = { 0 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[headerBytes.Length + pattern.Length + future.Length + bodyBytes.Length];
                ///拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                socketArgs.AcceptSocket.SendAsync(sendEventArgs);
            }
        }
        public void SendVoid(ClientRequestModel request)
        {
            if (socketArgs.AcceptSocket != null && socketArgs.AcceptSocket.Connected)
            {
#if DEBUG
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"{DateTime.Now}::{clientKey.Item1}:{clientKey.Item2}::[客-指令]\n{request}");
                Console.WriteLine("--------------------------------------------------");
#endif
                //构造data数据
                byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                //构造表头数据，固定4个字节的长度，表示内容的长度
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                //构造消息类型 0 为Respond,1 为VoidRespond
                byte[] pattern = { 1 };
                //预备未来的一些数据
                byte[] future = new byte[27];
                //总计需要
                byte[] sendBuffer = new byte[headerBytes.Length + pattern.Length + future.Length + bodyBytes.Length];
                ///拷贝到同一个byte[]数组中
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                socketArgs.AcceptSocket.SendAsync(sendEventArgs);
            }
        }
        #region IDisposable Members
        bool isDipose = false;

        ~SocketClient()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Socket clientSocket = SocketArgs.AcceptSocket;
            if (isDipose) return;
            if (disposing)
            {
                hostEndPoint = null;
                autoConnectEvent.Close();
                autoConnectEvent = null;
            }
            //处理非托管资源
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                clientSocket.Close();
                clientSocket = null;
            }
            isDipose = true;
        }

        #endregion
    }
}
