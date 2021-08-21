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
        private AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        private ClientConfig config;
        private Tuple<string, string> clientKey;
        private DataToken dataToken;
        private string netName;
        private string serviceName;
        private bool isReconnect = true;
        private bool isDispose = false;
        public DataToken DataToken { get => dataToken; set => dataToken = value; }
        public ClientConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        public Tuple<string, string> ClientKey { get => clientKey; set => clientKey = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        #region --ί��--
        public delegate void OnExceptionDelegate(Exception exception, SocketClient client);
        public delegate void OnLogDelegate(RPCLog log, SocketClient client);
        /// <summary>
        /// ���ӳɹ�ί��
        /// </summary>
        /// <param name="client">������</param>
        public delegate void OnConnectSuccessDelegate(SocketClient client);
        /// <summary>
        /// ����ʧ��ί��
        /// </summary>
        /// <param name="client">������</param>
        public delegate void OnConnectFailDelegate(SocketClient client);
        #endregion

        #region --�¼��ֶ�--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --�¼�����--
        /// <summary>
        /// ���ӳɹ��¼�
        /// </summary>
        public event OnConnectSuccessDelegate ConnectSuccessEvent;
        /// <summary>
        /// ����ʧ���¼�
        /// </summary>
        public event OnConnectFailDelegate ConnectFailEvent;
        /// <summary>
        /// ��־����¼�
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// �׳��쳣�¼�
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }
        #endregion
        /// <summary>
        /// Token
        /// </summary>
        public SocketClient(string netName,string serviceName, Tuple<string,string> clientKey, ClientConfig config)
        {
            this.ClientKey = clientKey;
            this.Config = config;
            IPAddress[] addressList = Dns.GetHostEntry(clientKey.Item1).AddressList;
            // Instantiates the endpoint and socket.
            hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], int.Parse(clientKey.Item2));
            this.netName = netName;
            this.serviceName = serviceName;

        }

        internal void Start()
        {
            Thread thread = new Thread(() =>
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
                        dataToken = new DataToken(NetName, ClientKey, Config);
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
                        OnConnectSuccess(this);
                    }
                    else
                    {
                        if(isReconnect)Reconnect();
                    }
                }
                catch (SocketException e)
                {
                    if (isReconnect)
                    {
                        OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}���ӷ�����ʧ�ܣ���������" + e.StackTrace);
                        Reconnect();
                    }
                }
            });
            thread.Start();
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent?.Set();
        }
        internal void Disconnect()
        {
            if (isDispose == false)
            {
                OnConnectFail(this);
                isReconnect = false;
                dataToken?.DisConnect();
                Dispose();
                isDispose = true;
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessReceive(e);
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
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
            catch(Exception exception)
            {
                Disconnect();
                OnException(exception);
            }
        }
        internal bool Reconnect()
        {
            OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}������������쳣,��ʼ����������");
            Socket clientSocket = null;
            if (dataToken != null)clientSocket = dataToken.SocketArgs.AcceptSocket;
            for (int i = 1; i <= 10; i++)
            {
                if (clientSocket != null)
                {
                    OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}��ʼ������ʷSocket");
                    clientSocket.Close();
                    clientSocket.Dispose();
                    OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}��ʷSocket������ɣ�");
                }
                OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}��ʼ���е�{i}�γ���");
                clientSocket = new Socket(hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += OnConnect;
                acceptArgs.AcceptSocket = clientSocket;
                acceptArgs.RemoteEndPoint = hostEndPoint;
                try
                {
                    clientSocket.ConnectAsync(acceptArgs);
                    autoConnectEvent.WaitOne();
                    SocketError errorCode = acceptArgs.SocketError;
                    if (errorCode  == SocketError.Success)
                    {
                        dataToken = new DataToken(NetName,ClientKey, Config);
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
                        OnConnectSuccess(this);
                        OnException(RPCException.ErrorCode.Runtime,$"{NetName}-{ClientKey}�����ɹ���");
                        break;
                    }
                    else
                    {
                        OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}����ʧ�ܣ�5������ԣ�");
                        Thread.Sleep(5000);
                    }
                }
                catch (SocketException e)
                {
                    OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}����ʧ�ܣ�5������ԣ�" + e.StackTrace);
                    Thread.Sleep(5000);
                }

            }
            if (!clientSocket.Connected)
            {
                OnException(RPCException.ErrorCode.Runtime, $"{NetName}-{ClientKey}����ʧ�ܣ�");
                Disconnect();
                return false;
            }
            else return true;
        }
        public bool Send(ClientRequestModel request)
        {
            if (dataToken?.SocketArgs?.AcceptSocket?.Connected == true)
            {
                string log = "";
                log += "---------------------------------------------------------\n";
                log += $"{DateTime.Now}::{ClientKey.Item1}:{ClientKey.Item2}::[��-����]\n{request}\n";
                log += "---------------------------------------------------------\n";
                OnLog(RPCLog.LogCode.Runtime, log);
                //����data����
                byte[] bodyBytes = Config.Encoding.GetBytes(Config.ClientRequestModelSerialize(request));
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
                return true;
            }
            else return false;
        }

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent(e, this);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                logEvent(log,this);
            }
        }
        public void OnConnectSuccess(SocketClient client)
        {
            if (ConnectSuccessEvent != null)
            {
                ConnectSuccessEvent(client);
            }
        }
        public void OnConnectFail(SocketClient client)
        {
            if (ConnectFailEvent != null)
            {
                ConnectFailEvent(client);
            }
        }
        #region IDisposable Members

        ~SocketClient()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            if(isDispose == false)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (isDispose) return;
            if (disposing)
            {
                autoConnectEvent.Close();
                autoConnectEvent = null;
                hostEndPoint = null;
            }
            if (dataToken != null)
            {
                Socket clientSocket = dataToken.SocketArgs.AcceptSocket;

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
            }
            isDispose = true;
        }

        #endregion
    }
}
