using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace TCP
{
    public class TCPManager
    {
        #region Variable

        private TcpClient _client;
        private TcpListener _listener;
        private Thread _listenThread;
        private NetworkStream _networkStream;
        private BinaryWriter _binaryWriter;

        private static int _connectionCount;

        #endregion

        #region Property

        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public int ReceivePort { get; private set; }
        public int ConnectionCount { get { return _connectionCount; } }

        #endregion

        #region Event

        public event ReceiveHandler Received;

        #endregion

        #region Constructor

        public TCPManager()
        {
            this._client = new TcpClient();
        }

        public TCPManager(string serverIP, int serverPort)
            : this()
        {
            this.ServerIP = serverIP;
            this.ServerPort = serverPort;
        }

        #endregion

        #region Public Method

        public void StartServer(int receivePort)
        {
            if (this._listenThread != null) return;

            this.ReceivePort = receivePort;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, receivePort);

            this._listener = new TcpListener(localEndPoint);
            this._listener.Start();

            this._listenThread = new Thread(new ThreadStart(Listen));
            this._listenThread.Start();
        }

        public void StopServer()
        {
            if (this._listener != null) this._listener.Stop();
            if (this._listenThread != null) this._listenThread.Abort();

            this._listener = null;
            this._listenThread = null;
        }

        public void Clean()
        {
            StopServer();
            Close();
        }

        public void Close()
        {
            if (this._binaryWriter != null) this._binaryWriter.Close();
            if (this._networkStream != null) this._networkStream.Close();
            if (this._client != null) this._client.Close();

            this._binaryWriter = null;
            this._networkStream = null;
            this._client = null;
        }

        public bool Send(object obj)
        {
            var bytes = ObjectSerializer.ObjectSerializer.ObjectToByteArray(obj);
            return Send(bytes);
        }

        public bool Send(string serverIP, int serverPort, object obj)
        {
            var bytes = ObjectSerializer.ObjectSerializer.ObjectToByteArray(obj);
            return Send(serverIP, serverPort, bytes);
        }

        public bool Send(byte[] bytes)
        {
            return Send(this.ServerIP, this.ServerPort, bytes);
        }

        public bool Send(string serverIP, int serverPort, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(serverIP))
            {
                throw new SocketException();
            }

            this._client = new TcpClient();
            var connection = Connection(serverIP, serverPort);

            if (connection)
            {
                SendStream(bytes);
            }

            this._client.Close();

            return connection;
        }

        #endregion

        #region Private Method

        private void Listen()
        {
            while (true)
            {
                try
                {
                    var acceptClient = this._listener.AcceptTcpClient();
                    AcceptClientThread connection = new AcceptClientThread(this, acceptClient);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(connection.Run));
                }
                catch (Exception exception)
                {
#if DEBUG
                    Console.WriteLine(exception.Message);
#endif
                    break;
                }
            }
        }

        private void SendStream(byte[] bytes)
        {
            this._binaryWriter.Write(bytes.Length);
            this._binaryWriter.Write(bytes);
            this._binaryWriter.Flush();
        }

        private bool Connection(string serverIP, int serverPort)
        {
            bool successed = false;
            var ar = this._client.BeginConnect(serverIP, serverPort, null, null);
            successed = ar.AsyncWaitHandle.WaitOne(100, true);
            ar.AsyncWaitHandle.Close();

            if (successed)
            {
                this._networkStream = this._client.GetStream();
                this._binaryWriter = new BinaryWriter(this._networkStream);
            }

            return successed;
        }

        private void CreateSocket()
        {
            Close();
            this._client = new TcpClient();
        }

        #endregion

        private class AcceptClientThread
        {
            #region Property

            public TCPManager Manager { get; private set; }
            public TcpClient Client { get; private set; }

            #endregion

            #region Variable

            private NetworkStream _networkStream;
            private BinaryReader _binaryReader;

            #endregion

            public AcceptClientThread(TCPManager manager, TcpClient client)
            {
                this.Manager = manager;
                this.Client = client;
            }

            public void Run(object state)
            {
                _connectionCount++;

                string clientIP = (this.Client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                Console.WriteLine("connected : " + clientIP);

                this._networkStream = this.Client.GetStream();
                this._binaryReader = new BinaryReader(this._networkStream);

                while (true)
                {
                    try
                    {
                        /*
                        var msg = this._binaryReader.ReadInt32();

                        if (msg == 1)
                        {
                            var byteLength = this._binaryReader.ReadInt32();
                            var buffer = this._binaryReader.ReadBytes(byteLength);
                            this.Manager.Received(this.Manager, new TCPReceiveEventArgs(buffer, msg.ToString(), clientIP));
                        }
                        else
                        {
                            this.Manager.Received(this.Manager, new TCPReceiveEventArgs(null, msg.ToString(), clientIP));
                        }*/

                        var byteLength = this._binaryReader.ReadInt32();
                        var buffer = this._binaryReader.ReadBytes(byteLength);
                        this.Manager.Received(this.Manager, new TCPReceiveEventArgs(buffer, "", clientIP));
                    }
                    catch (Exception exception)
                    {
#if DEBUG
                        Console.WriteLine(exception.Message);
#endif
                        
                        Close();
                        break;
                    }
                }
            }

            public void Close()
            {
                _connectionCount--;

                if (this._binaryReader != null) this._binaryReader.Close();
                if (this._networkStream != null) this._networkStream.Close();
                if (this.Client != null) this.Client.Close();

                this._binaryReader = null;
                this._networkStream = null;
                this.Client = null;
            }
        }
    }
}