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
        private bool _PersistConnection = true;
        private int _PortNumber;
        private int _Retries = 1;
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
        /// Set to false to close the connection after sending the message.
        /// </summary>
        [Category("[ SocketSender ]")]
        [Description("Set to false to close the connection after sending the message.")]
        [DefaultValue(true)]
        public bool PersistConnection
        {
            get { return _PersistConnection; }
            set { _PersistConnection = value; }
        }
        /// <summary>
        /// Number of retry to attempt sending the package after it failed.
        /// </summary>
        [Category("[ SocketSender ]")]
        [Description("Number of retry to attempt sending the package after it failed.")]
        [DefaultValue(1)]
        public int Retries
        {
            get { return _Retries; }
            set { _Retries = value; }
        }

        /// <summary>
        /// The socket that is currently in use to transmit and receive data.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public Socket CurrentSocket
        {
            get { return _CurrentSocket; }
            set { _CurrentSocket = value; }
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
        public Socket Connect()
        {
            // use the default timeout
            return Connect(-1);
        }

        public Socket Connect(int timeout)
        {
            try
            {
                if (_CurrentSocket != null && _CurrentSocket.Connected)
                {
                    return _CurrentSocket;
                }
                else
                {
                    _CurrentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    if (_ipAddresses == null)
                    {
                        _ipAddresses = Dns.GetHostAddresses(Address);
                        if (_ipAddresses == null || _ipAddresses.Length == 0)
                        {
                            LastException = new Exception("Invalid Address");
                            return null;
                        }
                    }

                    if (timeout > 0)
                    {
                        IAsyncResult connectAr = _CurrentSocket.BeginConnect(_ipAddresses, PortNumber, 
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
                            }, _CurrentSocket);

                        WaitHandle wait = connectAr.AsyncWaitHandle;

                        if (!connectAr.IsCompleted)
                        {
                            wait.WaitOne(timeout);
                            if (!connectAr.IsCompleted)
                            {
                                _CurrentSocket.EndConnect(connectAr);
                                _CurrentSocket = null;
                                return _CurrentSocket;
                            }
                            else
                            {
                                _CurrentSocket.EndConnect(connectAr);
                            }
                        }
                    }
                    else
                    {
                        _CurrentSocket.Connect(_ipAddresses, PortNumber);
                    }


                    if (_CurrentSocket.Connected)
                    {
                        if (!_listening && ServerDataReceived != null)
                        {
                            BeginReceiving();
                        }
                    }
                    else
                    {
                        _CurrentSocket = null;
                        return _CurrentSocket;
                    }

                    return _CurrentSocket;
                }
            }
            catch (Exception xpt)
            {
                _listening = false;
                _CurrentSocket = null;
                return _CurrentSocket;
            }
        }

        /// <summary>
        /// Disconnects the current connection.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            _listening = false;
            closeConnection(_CurrentSocket);
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
            return send(data, Retries);
        }

        /// <summary>
        /// Sends the data asynchronously.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SendAsync(string data)
        {
            ThreadPool.QueueUserWorkItem(sendThread);
        }

        private void sendThread(object o)
        {
            string data = (string)o;
            send(data, Retries);
        }

        private bool send(string data, int tries)
        {
            try
            {
                if (tries <= 0)
                {
                    return false;
                }

                Socket socket = Connect();
                if (socket == null)
                {
                    if (tries > 0)
                    {
                        return send(data, tries--);
                    }
                    else
                    {
                        return false;
                    }
                }

                byte[] content = Encoding.UTF8.GetBytes(data);
                int byteCount = socket.Send(content);

                if (!PersistConnection)
                {
                    closeConnection(socket);
                }
                bool success = byteCount == content.Length;

                if (!success && tries > 0)
                {
                    return send(data, tries--);
                }
                return success;
            }
            catch (Exception xpt)
            {
                LastException = xpt;

                // Exit if it is an address related issues
                if (xpt.Message.Contains(":" + PortNumber))
                {
                    return false;
                }

                if (tries > 0)
                {
                    return send(data, tries--);
                }
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

            ThreadPool.QueueUserWorkItem(receivingData);
        }

        public void EndReceiving()
        {
            _listening = false;
        }

        private void receivingData(object o)
        {
            if (_listening)
            {
                return;
            }
            _listening = true;

            Socket socket = Connect();
            if (socket == null)
            {
                return;
            }

            try
            {
                while (_listening)
                {
                    byte[] buffer = new byte[4096];
                    int lenRecv;
                    string data;
                    while ((lenRecv = socket.Receive(buffer)) > 0)
                    {
                        data = Encoding.UTF8.GetString(buffer, 0, lenRecv);

                        Dlgt.ThreadSafeInvoke(ServerDataReceived, this, 
                                new SocketDataEventArgs(socket, Address, PortNumber, data));

                        if (socket.Available == 0 || !_listening)
                        {
                            break;
                        }
                    }

                    if (!socket.Connected || lenRecv == 0)
                    {
                        _listening = false;
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
        }
        #endregion

        #region [ IDisposable Members ]
        void IDisposable.Dispose()
        {
            closeConnection(_CurrentSocket);
        }
        #endregion

        #region [ Delegates ]
        public delegate void ServerDataReceivedHandler(SocketSender sender, SocketDataEventArgs e);
        #endregion
    }
}
