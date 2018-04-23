using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using LittleUmph;
using System.Linq;

///Todo: this component need a complete clean up

namespace LittleUmph.Net.Components
{
    /// <summary>
    /// Automate the process of receiving data on a socket.
    /// </summary>
    [DefaultEvent("SocketDataReceived")]
    [ToolboxBitmap(typeof(SocketListener), "Images.SocketListener.png")]
    public partial class SocketListener : Component, IDisposable, ISupportInitialize
    {
        #region [ Private Variables ]
        private Thread _listeningThread;
        private bool _AutoStart = true;
        private bool _Listening;
        private int _PortNumber;
        private int _BackLog = 200;
        private Exception _LastException;
        private bool _ThreadSafe = true;

        // Thread signal.
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Start listening automatically. When false you need to call StartListening() manually.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Start listening automatically. When false you need to call StartListening() manually.")]
        [DefaultValue(true)]
        public bool AutoStart
        {
            get { return _AutoStart; }
            set { _AutoStart = value; }
        }

        /// <summary>
        /// Return true when the socket is being monitor for data.
        /// </summary>
        [Browsable(false)]
        public bool Listening
        {
            get { return _Listening; }
            private set { _Listening = value; }
        }

        /// <summary>
        /// The port number to listen to on this manchine.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("The port number to listen to on this manchine.")]
        public int PortNumber
        {
            get { return _PortNumber; }
            set { _PortNumber = value; }
        }

        /// <summary>
        /// How many connection is allowed to queue up while you process one transaction.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("How many connection is allowed to queue up while you process one transaction.")]
        [DefaultValue(200)]
        public int BackLog
        {
            get { return _BackLog; }
            set { _BackLog = value; }
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
        /// Set to true to invoke the events in a thread safe mananer.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Set to true to invoke the events in a thread safe mananer.")]
        [DefaultValue(true)]
        public bool ThreadSafe
        {
            get { return _ThreadSafe; }
            set { _ThreadSafe = value; }
        }
        #endregion

        #region [ Events Declaration ]
		/// <summary>
        /// Occurs when data received on the socket.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Occurs when data received on the socket.")]
        public event DataReceivedHandler DataReceived;

        /// <summary>
        /// Occurs when encountered an error.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Occurs when encountered an error.")]
        public event ErrorEncounteredHandler ErrorEncountered;

        /// <summary>
        /// Occurs when the client's socket first is openned.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Occurs when the client's socket first is openned.")]
        public event SocketHandler ClientConnected;

        /// <summary>
        /// Occurs when the client's socket is closed or encountered an error.
        /// </summary>
        [Category("[ SocketListener ]")]
        [Description("Occurs when the client's socket is closed or encountered an error.")]
        public event SocketHandler ClientDisconnected;

	    #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketListener"/> class.
        /// </summary>
        public SocketListener() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketListener"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public SocketListener(IContainer container)
        {
            if (container != null)
            {
                container.Add(this);
            }

            InitializeComponent();            
        }

        #endregion

        #region [ Start & Stop The Listening Thread ]
        /// <summary>
        /// Starts the listening.
        /// </summary>
        public void StartListening()
        {
            if (!Listening)
            {
                try
                {
                    _listeningThread = new Thread(listeningThread);
                    _listeningThread.IsBackground = true;

                    Listening = true;
                    _listeningThread.Start();

                    LastException = null;
                }
                catch (Exception xpt)
                {
                    Listening = false;  
                    LastException = xpt;
                    
                    Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                new SocketErrorEventArgs("StartListening()", xpt));
                }
            }
        }

        /// <summary>
        /// Stops the listening.
        /// </summary>
        public void StopListening()
        {
            if (Listening)
            {
                try
                {
                    LastException = null;
                    Listening = false;

                    AbortThread();
                }
                catch (Exception xpt)
                {
                    Listening = false;
                    LastException = xpt;
                   Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                new SocketErrorEventArgs("StopListening()", xpt));
                }
            }
        }

        /// <summary>
        /// Aborts the thread.
        /// </summary>
        private void AbortThread()
        {
            try
            {
                // Signal the thread to stop using "Listening" flag
                Listening = false;
                // Tell "WaitOne" method in listeningThread() method to stop blocking
                _allDone.Set(); 

                // Max wait time is 3000ms or 3sec
                for (int i = 0; i < 30; i++)
                {
                    if (_listeningThread != null && _listeningThread.IsAlive)
                    {
                        // wait for the thread to stop
                        _listeningThread.Join(100);
                    }
                    else
                    {
                        break;
                    }
                }


                if (_listeningThread != null && _listeningThread.IsAlive)
                {
                    _listeningThread.Abort();
                }
            }
            finally
            {
                _listeningThread = null;
            }
        }
        #endregion

        #region [ Listening Thread ]
        /// <summary>
        /// Listenings the thread.
        /// </summary>
        private void listeningThread()
        {
            try
            {
                Listening = true;

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
                    // Non-blocking
                    socket.Listen(BackLog);

                    while (Listening)
                    {
                        try
                        {
                            // This will clear the flag, and will block "WaitOne" method (NON-blocking call)
                            _allDone.Reset();

                            // Non-blocking call: when there is an incoming connection acceptingIncomingConnection() is called
                            socket.BeginAccept(acceptingIncomingConnection, socket);

                            // This will block until _allDone.Set() is called
                            _allDone.WaitOne();
                        }
                        catch (Exception xpt)
                        {
                            LastException = xpt;
                            Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                new SocketErrorEventArgs("listeningThread()", xpt));
                        }
                    }
                }
            }
            catch (Exception xpt)
            {
                Listening = false;
                LastException = xpt;
               Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                new SocketErrorEventArgs("listeningThread()", xpt));
            }
        }

        private void acceptingIncomingConnection(IAsyncResult ar)
        {
            try
            {
                // Signal the listeningThread to accept new connection
                // ie unblock "_allDone.WaitOne()" line
                _allDone.Set();

                // Get the main socket
                Socket socket = (Socket)ar.AsyncState;

                // Receive incoming socket from client
                Socket recv = socket.EndAccept(ar);

                if (!Listening)
                {
                    return;
                }

                try
                {
                    string address = "";
                    int portUsed;
                    try
                    {
                        IPEndPoint sender = (IPEndPoint)recv.RemoteEndPoint;
                        address = sender.Address.ToString();
                        portUsed = sender.Port;
                    }
                    catch (Exception xpt)
                    {
                        address = "";
                        portUsed = 0;
                    }
                    Dlgt.Invoke(ThreadSafe, ClientConnected, this,
                                         new SocketEventArgs(recv, address, portUsed));

                    processConnection(recv, address, portUsed);
                }
                catch (Exception xpt)
                {
                    Listening = false;
                    LastException = xpt;
                    Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                     new SocketErrorEventArgs("acceptingIncomingConnection(ar)", xpt));
                }
            }
            catch (System.ObjectDisposedException xpt)
            {
            }
        }

        private void processConnection(Socket recv, string address, int portUsed)
        {
            try
            {
                bool clientDisconnected = false;

                while (recv.Connected && !clientDisconnected)
                {
                    if (!Listening)
                    {
                        return;
                    }

                    // Connected but there is no data to receive
                    if (recv.Available == 0)
                    {
                        // Connection on the otherside is disconnected (mean while recv.Connected has not been updated)
                        // When Available==0, Then Poll() will return true if the connection has been closed, reset, or terminated; 
                        // REF: http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.poll%28v=vs.110%29.aspx
                        // Available==0 AND Poll()==true  ==>> Client is DISCONNECTED
                        if (recv.Poll(1000, SelectMode.SelectRead))
                        {
                            // In-case available changed during the poll
                            if (recv.Available == 0)
                            {
                                clientDisconnected = true;
                                break;
                            }
                        }

                        Thread.Sleep(100);
                    }
                    else
                    {
                        clientDisconnected = false;

                        List<byte> rawData = new List<byte>();
                        string result = "";
                        byte[] buffer = new byte[4096];
                        int lenRecv;

                        while ((lenRecv = recv.Receive(buffer)) > 0)
                        {
                            if (!Listening)
                            {
                                return;
                            }
                            var data = buffer.Take(lenRecv).ToArray();
                            rawData.AddRange(data);
                            result += Encoding.UTF8.GetString(data);

                            if (recv.Available == 0)
                            {
                                break;
                            }
                        }

                        if (result.Length > 0)
                        {
                            Dlgt.Invoke(ThreadSafe, DataReceived, this,
                                            new SocketDataEventArgs(recv, address, portUsed, result, rawData));
                        }
                    }
                }

                if (!recv.Connected || clientDisconnected)
                {
                    Dlgt.Invoke(ThreadSafe, ClientDisconnected, this,
                                 new SocketEventArgs(recv, address, portUsed));

                    recv.Shutdown(SocketShutdown.Both);
                    recv.Disconnect(false);
                    recv.Close();
                    recv = null;
                }
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
                Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                 new SocketErrorEventArgs("processConnection()", xpt));
            }
        }
        #endregion

        #region [ IDisposable Members ]
        /// <summary>
        /// Releases all resources used by the <see cref="T:System.ComponentModel.Component"/>.
        /// </summary>
        void IDisposable.Dispose()
        {
            try
            {
                AbortThread();
            }
            catch (Exception xpt)
            {
                LastException = xpt;
               Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                new SocketErrorEventArgs("Dispose()", xpt));
            }
        }
        #endregion

        #region [ ISupportInitialize Members ]
        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (!DesignMode)
            {
                if (AutoStart)
                {
                    StartListening();
                }
            }
        }
        #endregion


        /// <summary>
        /// Send a reply to the incoming socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Send(Socket socket, string data)
        {
            var payload = Encoding.UTF8.GetBytes(data);
            return Send(socket, payload);
        }

        /// <summary>
        /// Send a reply to the incoming socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Send(Socket socket, byte[] data)
        {
            if (socket.Connected)
            {
                try
                {
                    int dataSent = socket.Send(data);
                    return data.Length == dataSent;
                }
                catch (Exception xpt)
                {
                    LastException = xpt;
                    Dlgt.Invoke(ThreadSafe, ErrorEncountered, this,
                                     new SocketErrorEventArgs("Send()", xpt));
                    return false;
                }
            }
            return true;
        }
    }

    #region [ Public Delegates ]
    /// <summary>
    /// When data received on the socket.
    /// </summary>
    public delegate void DataReceivedHandler(SocketListener socket, SocketDataEventArgs e);

    /// <summary>
    /// When error encountered on the socket.
    /// </summary>
    public delegate void ErrorEncounteredHandler(SocketListener socket, SocketErrorEventArgs e);

    /// <summary>
    /// When an event occur.
    /// </summary>
    public delegate void SocketHandler(SocketListener socket, SocketEventArgs e);
	#endregion

    #region [ SocketEventArgs ]
    /// <summary>
    /// General event argument.
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
        #region [ Private Variables ]
        private Socket _Socket;
        private string _HostName;
        private string _Addresss;
        private int _Port;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the socket.
        /// </summary>
        /// <value>The socket.</value>
        public Socket Socket
        {
            get { return _Socket; }
            set { _Socket = value; }
        }

        /// <summary>
        /// Gets the addresss of the sender.
        /// </summary>
        /// <value>The addresss.</value>
        public string Addresss
        {
            get { return _Addresss; }
            protected set { _Addresss = value; }
        }

        /// <summary>
        /// Gets the port number used.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get { return _Port; }
            protected set { _Port = value; }
        }

        /// <summary>
        /// Return the host name of the sender, if not availiable it will return empty string.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName
        {
            get
            {
                if (_HostName != null)
                {
                    return _HostName;
                }

                // Do this here to avoid look up the dns everytime this event occurs.
                // It only trying to findout the host if someone actually wants the data.
                HostName = getHostName(Addresss);
                return HostName;
            }
            protected set { _HostName = value; }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketDataEventArgs" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public SocketEventArgs(Socket socket, string address, int port)
        {
            Socket = socket;
            Addresss = address;
            Port = port;
        }
        #endregion

        #region [ Helper ]
        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
		private string getHostName(string address)
        {
            try
            {
                IPHostEntry sender = Dns.GetHostEntry(IPAddress.Parse(address));
                return sender.HostName;
            }
            catch (Exception xpt)
            {
                return string.Empty;
            }
        }
	#endregion
    }
    #endregion

    #region [ SocketDataEventArgs ]
    /// <summary>
    /// Socket data received event argument.
    /// </summary>
    public class SocketDataEventArgs : SocketEventArgs
    {
        #region [ Private Variables ]
		private string _Data;
        #endregion

        #region [ Properties ]
		/// <summary>
        /// Gets the data from the sender.
        /// </summary>
        /// <value>The data.</value>
        public string Data
        {
            get { return _Data; }
            private set { _Data = value; }
        }

        /// <summary>
        /// Return the raw byte array which received from the incoming connection
        /// </summary>
        public IList<byte> RawData { get; set; }

        /// <summary>
        /// Return the hex representation of RawData
        /// </summary>
        public string Hex
        {
            get
            {
                return ByteArr.ToHex(RawData.ToArray(), "-");
            }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketDataEventArgs" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="data">The data.</param>
        /// <param name="rawData">The data in byte[] form.</param>
        public SocketDataEventArgs(Socket socket, string address, int port, string data, IList<byte> rawData) : base(socket, address, port)
        {
            Data = data;
            RawData = rawData;
        }
        #endregion
    }
	#endregion

    #region [ SocketErrorEventArgs ]
    /// <summary>
    /// Socket error encountered event argument.
    /// </summary>
    public class SocketErrorEventArgs
    {
        #region [ Private Variables ]
        private Exception _Exception;
        private string _Source; 
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception
        {
            get { return _Exception; }
            private set { _Exception = value; }
        }

        /// <summary>
        /// Gets the source of the error.
        /// </summary>
        /// <value>The source.</value>
        public string Source
        {
            get { return _Source; }
            private set { _Source = value; }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketErrorEventArgs"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="xpt">The exception.</param>
        public SocketErrorEventArgs(string source, Exception xpt)
        {
            Source = source;
            Exception = xpt;
        }
        #endregion
    }
    #endregion
}
