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
        /// �ź���,�����߳̽������ӵȴ�
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        private ClientConfig config;
        private Tuple<string, string> clientKey;
        private DataToken dataToken;
        private string netName;
        public DataToken DataToken { get => dataToken; set => dataToken = value; }
        /// <summary>
        /// Token
        /// </summary>
        public SocketClient(Net net, Tuple<string,string> clientKey, ClientConfig config)
        {
            this.clientKey = clientKey;
            this.config = config;
            // Get host related information.
            IPAddress[] addressList = Dns.GetHostEntry(clientKey.Item1).AddressList;
            // Instantiates the endpoint and socket.
            hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], int.Parse(clientKey.Item2));
            net.ClientRequestSend = Send;
            this.netName = net.Name;
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
                    dataToken = new DataToken(netName,clientKey, config);
                    SocketAsyncEventArgs SocketArgs = dataToken.SocketArgs;
                    SocketArgs.Completed += OnReceiveCompleted;
                    SocketArgs.AcceptSocket = acceptArgs.AcceptSocket;
                    SocketArgs.UserToken = dataToken;
                    SocketArgs.RemoteEndPoint = hostEndPoint;
                    SocketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                    dataToken.Connect(socket);
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
                config.OnException(RPCException.ErrorCode.Runtime,$"{netName}-{clientKey}���ӷ�����ʧ�ܣ���������" + e.StackTrace,this);
                Reconnect();
            }
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
        }

        public void Disconnect()
        {
            dataToken.DisConnect();
            dataToken.SocketArgs.AcceptSocket.Disconnect(false);
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
                    throw new SocketException((int)SocketError.Disconnecting);
                }
            }
            else
            {
                throw new SocketException((int)SocketError.Disconnecting);
            }
        }
        public bool Reconnect()
        {
            config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}������������쳣,��ʼ����������",this);
            Socket clientSocket = null;
            if (dataToken != null)clientSocket = dataToken.SocketArgs.AcceptSocket;
            for (int i = 1; i <= 10; i++)
            {
                if (clientSocket != null)
                {
                    config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}��ʼ������ʷSocket", this);
                    clientSocket.Close();
                    clientSocket.Dispose();
                    config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}��ʷSocket������ɣ�", this);
                }
                config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}��ʼ���е�{i}�γ���", this);
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
                        dataToken = new DataToken(netName,clientKey, config);
                        SocketAsyncEventArgs SocketArgs = dataToken.SocketArgs;
                        SocketArgs.Completed += OnReceiveCompleted;
                        SocketArgs.AcceptSocket = acceptArgs.AcceptSocket;
                        SocketArgs.UserToken = dataToken;
                        SocketArgs.RemoteEndPoint = hostEndPoint;
                        SocketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                        dataToken.Connect(clientSocket);
                        if (!SocketArgs.AcceptSocket.ReceiveAsync(SocketArgs))
                        {
                            ProcessReceive(SocketArgs);
                        }
                        config.OnException(RPCException.ErrorCode.Runtime,$"{netName}-{clientKey}�����ɹ���", this);
                        break;
                    }
                    else
                    {
                        config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}����ʧ�ܣ�5������ԣ�", this);
                        Thread.Sleep(5000);
                    }
                }
                catch (SocketException e)
                {
                    config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}����ʧ�ܣ�5������ԣ�" + e.StackTrace, this);
                    Thread.Sleep(5000);
                }
            }
            if (!clientSocket.Connected)
            {
                config.OnException(RPCException.ErrorCode.Runtime, $"{netName}-{clientKey}����ʧ�ܣ�", this);
                return false;
            }
            else return true;
        }
        private void Send(ClientRequestModel request)
        {
            if (dataToken.SocketArgs.AcceptSocket != null && dataToken.SocketArgs.AcceptSocket.Connected)
            {
                string log = "";
                log += "---------------------------------------------------------\n";
                log += $"{DateTime.Now}::{clientKey.Item1}:{clientKey.Item2}::[��-����]\n{request}\n";
                log += "---------------------------------------------------------\n";
                config.OnLog(RPCLog.LogCode.Runtime,log, this);
                //����data����
                byte[] bodyBytes = config.Encoding.GetBytes(config.ClientRequestModelSerialize(request));
                //�����ͷ���ݣ��̶�4���ֽڵĳ��ȣ���ʾ���ݵĳ���
                byte[] headerBytes = BitConverter.GetBytes(bodyBytes.Length);
                //������Ϣ���� 0 ΪRequest
                byte[] pattern = { 0 };
                //Ԥ��δ����һЩ����
                byte[] future = new byte[27];
                //�ܼ���Ҫ
                byte[] sendBuffer = new byte[32 + bodyBytes.Length];
                ///������ͬһ��byte[]������
                Buffer.BlockCopy(headerBytes, 0, sendBuffer, 0, headerBytes.Length);
                Buffer.BlockCopy(pattern, 0, sendBuffer, headerBytes.Length, pattern.Length);
                Buffer.BlockCopy(future, 0, sendBuffer, headerBytes.Length + pattern.Length, future.Length);
                Buffer.BlockCopy(bodyBytes, 0, sendBuffer, headerBytes.Length + pattern.Length + future.Length, bodyBytes.Length);
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                dataToken.SocketArgs.AcceptSocket.SendAsync(sendEventArgs);
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
            Socket clientSocket = dataToken.SocketArgs.AcceptSocket;
            if (isDipose) return;
            if (disposing)
            {
                hostEndPoint = null;
                autoConnectEvent.Close();
                autoConnectEvent = null;
            }
            //������й���Դ
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                dataToken = null;
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
