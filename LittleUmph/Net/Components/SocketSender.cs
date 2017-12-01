using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using LittleUmph;

///Todo: this component need a complete clean up

namespace LittleUmph.Net.Components
{
    [ToolboxBitmap(typeof(SocketSender), "Images.SocketSender.png")]
    [DefaultEvent("ServerDataReceived")]
    public partial class SocketSender : Component,IDisposable
    {
        #region [ Private Variables ]
        private const int BufferLength = 4096;
        private string _Address;
        private Exception _LastException;
        private int _PortNumber;
        private int _ConnectionTimeout = 7000;
        private Socket _CurrentSocket;
        private bool _listening = false;
        private IPAddress[] _ipAddresses;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// The address to send data, it could be an ip address, a LAN computer name, or a domain name on the internet.
        /// </summary>
        [Category("[ SocketSender ]")]
        [Description("The address to send data, it could be an ip address, a LAN computer name, or a domain name on the internet.")]
        public string Address
        {
            get { return _Address; }
            set { 
                _Address = value;
                _ipAddresses = null;
            }
        }

        /// <summary>
        /// The port number to send data.
        /// </summary>
        [Category("[ SocketSender ]")]
        [Description("The port number to send data.")]
        public int PortNumber
        {
            get { return _PortNumber; }
            set { _PortNumber = value; }
        }

        /// <summary>
        /// Gets the last exception.
        /// </summary>
        /// <value>The last exception.</value>
        [Browsable(false)]
        public Exception LastException
        {
            get { return _LastException; }
            private set { _LastException = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool HasError
        {
            get { return _LastException != null; }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        /// <value>
        /// The connection timeout.
        /// </value>
        [Category("[ SocketSender ]")]
        [Description("Connection timeout limit (in millisecond.")]
        [DefaultValue(7000)]
        public int ConnectionTimeout
        {
            get { return _ConnectionTimeout; }
            set { _ConnectionTimeout = value; }
        }

        /// <summary>
        /// The socket that is currently in use to transmit and receive data.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public Socket CurrentSocket
        {
            get { return _CurrentSocket; }
            private set { _CurrentSocket = value; }
        }
        #endregion

        #region [ Event Declaration ]
        /// <summary>
        /// Occurs when received data from the server.
        /// </summary>
        [Category("[ SocketSender ]")]
        [Description("Occurs when received data from the server.")]
        public event ServerDataReceivedHandler ServerDataReceived;
        #endregion

        #region [ Constructors ]
        public SocketSender()
        {
            InitializeComponent();
        }

        public SocketSender(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        #endregion

        #region [ Connect & Disconnect ]
        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            // use the default timeout
            Connect(ConnectionTimeout);
        }

        /// <summary>
        /// Connects the specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public void Connect(int timeout)
        {
            try
            {
                if (CurrentSocket != null && CurrentSocket.Connected)
                {
                    return;
                }
                else
                {
                    CurrentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    if (_ipAddresses == null)
                    {
                        _ipAddresses = Dns.GetHostAddresses(Address);
                        if (_ipAddresses == null || _ipAddresses.Length == 0)
                        {
                            LastException = new Exception("Invalid Address");
                            CurrentSocket = null;
                            return;
                        }
                    }

                    if (timeout > 0)
                    {
                        IAsyncResult connectAr = CurrentSocket.BeginConnect(_ipAddresses, PortNumber, 
                            delegate(IAsyncResult ar) 
                            {
                                try
                                {
                                    Socket s = (Socket)ar.AsyncState;
                                    s.EndConnect(ar);
                                }
                                catch (Exception xpt)
                                {
                                    
                                }
                            }, CurrentSocket);
                        
                        if (!connectAr.IsCompleted)
                        {
                            connectAr.AsyncWaitHandle.WaitOne(timeout);
                            if (!connectAr.IsCompleted)
                            {
                                CurrentSocket.EndConnect(connectAr);
                                CurrentSocket = null;
                            }
                        }
                    }
                    else
                    {
                        CurrentSocket.Connect(_ipAddresses, PortNumber);
                    }


                    if (CurrentSocket.Connected)
                    {
                        if (!_listening && ServerDataReceived != null)
                        {
                            BeginReceiving();
                        }
                    }
                    else
                    {
                        CurrentSocket = null;
                    }
                }
            }
            catch (Exception xpt)
            {
                _listening = false;
                CurrentSocket = null;
            }
        }

        /// <summary>
        /// Disconnects the current connection.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            _listening = false;
            closeConnection(CurrentSocket);
        }
        #endregion

        #region [ Send ]
        /// <summary>
        /// Sends the data to the specified Address and PortNumber in the properties.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public bool Send(string data)
        {
            return send(data);
        }

        /// <summary>
        /// Sends the data asynchronously.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SendAsync(string data)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                send(data);
            });
        }
        
        private bool send(string data)
        {
            try
            {
                Connect();
                if (CurrentSocket == null)
                {
                        return false;
                }

                byte[] content = Encoding.UTF8.GetBytes(data);
                int byteCount = CurrentSocket.Send(content);

                bool success = byteCount == content.Length;
                return success;
            }
            catch (Exception xpt)
            {
                LastException = xpt;
                return false;
            }
        }

        private static void closeConnection(Socket socket)
        {
            if (socket == null)
            {
                return;
            }
            else if (!socket.Connected)
            {
                socket.Close();
                socket = null;
                return;
            }


            socket.Shutdown(SocketShutdown.Send);                        
            try
            {
                if (socket.Available > 0)
                {
                    byte[] buffer = new byte[BufferLength];
                    int lenRecv = 0;
                    while ((lenRecv = socket.Receive(buffer)) > 0)
                    {
                        if (socket.Available == 0 || lenRecv == 0)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception xpt)
            {
                Console.WriteLine("closeConnection: " + xpt.Message);
            }
            socket.Close();
            socket = null;
        }
        #endregion

        #region [ Begin & End Receive ]
        /// <summary>
        /// Receives the data from the server.
        /// </summary>
        public void BeginReceiving()
        {
            if (_listening)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem((o) =>
            {
                if (_listening)
                {
                    return;
                }
                _listening = true;

                Connect();
                if (CurrentSocket == null)
                {
                    return;
                }

                try
                {
                    while (_listening && CurrentSocket.Connected)
                    {
                        if (CurrentSocket.Available == 0)
                        {
                            Thread.Sleep(200);
                        }
                        else
                        {
                            List<byte> rawData = new List<byte>();
                            byte[] buffer = new byte[4096];
                            int lenRecv;
                            string data = "";
                            while ((lenRecv = CurrentSocket.Receive(buffer)) > 0)
                            {
                                if (!_listening)
                                {
                                    return;
                                }

                                data += Encoding.UTF8.GetString(buffer, 0, lenRecv);

                                if (CurrentSocket.Available == 0)
                                {
                                    Dlgt.ThreadSafeInvoke(ServerDataReceived, this,
                                        new SocketDataEventArgs(CurrentSocket, Address, PortNumber, data, rawData));
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception xpt)
                {
                    Console.WriteLine(xpt.Message);
                }
                finally
                {
                    _listening = false;
                }
            });
        }

        public void EndReceiving()
        {
            _listening = false;
        }
        #endregion

        #region [ IDisposable Members ]
        void IDisposable.Dispose()
        {
            closeConnection(CurrentSocket);
        }
        #endregion

        #region [ Delegates ]
        public delegate void ServerDataReceivedHandler(SocketSender sender, SocketDataEventArgs e);
        #endregion
    }
}
