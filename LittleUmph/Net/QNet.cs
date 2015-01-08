using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Management;
using System.IO;

using LittleUmph;


namespace LittleUmph
{

    #region [ ShutDownOptions Enum ]
    /// <summary>
    /// Parameter for RemoteShutDown
    /// </summary>
    [Flags]
    public enum ShutDownOptions : uint
    {
        /// <summary>
        /// All sessions are forcefully logged off. If this flag is not set and users other than the current user are logged on to the computer specified by the lpMachineName parameter, this function fails with a return value of ERROR_SHUTDOWN_USERS_LOGGED_ON.
        /// </summary>
        SHUTDOWN_FORCE_OTHERS = 0x1,
        /// <summary>
        /// Specifies that the originating session is logged off forcefully. If this flag is not set, the originating session is shut down interactively, so a shutdown is not guaranteed even if the function returns successfully.
        /// </summary>
        SHUTDOWN_FORCE_SELF = 0x2,
        /// <summary>
        /// Overrides the grace period so that the computer is shut down immediately.
        /// </summary>
        SHUTDOWN_GRACE_OVERRIDE = 0x20,
        /// <summary>
        /// Beginning with InitiateShutdown running on Windows 8, you must include the SHUTDOWN_HYBRID flag with one or more of the flags in this table to specify options for the shutdown.
        /// Beginning with Windows 8, InitiateShutdown always initiate a full system shutdown if the SHUTDOWN_HYBRID flag is absent. 
        /// </summary>
        SHUTDOWN_HYBRID = 0x200,
        /// <summary>
        /// The computer installs any updates before starting the shutdown.
        /// </summary>
        SHUTDOWN_INSTALL_UPDATES = 0x40,
        /// <summary>
        /// The computer is shut down but is not powered down or rebooted.
        /// </summary>
        SHUTDOWN_NOREBOOT = 0x10,
        /// <summary>
        /// The computer is shut down and powered down.
        /// </summary>
        SHUTDOWN_POWEROFF = 0x8,
        /// <summary>
        /// The computer is shut down and rebooted.
        /// </summary>
        SHUTDOWN_RESTART = 0x4,
        /// <summary>
        /// The system is rebooted using the ExitWindowsEx function with the EWX_RESTARTAPPS flag. This restarts any applications that have been registered for restart using the RegisterApplicationRestart function.
        /// </summary>
        SHUTDOWN_RESTARTAPPS = 0x80
    }
    #endregion

    /// <summary>
    /// Network assisted functions (ex: ping, url encode/decode etc.)
    /// </summary>
    public class QNet
    {
        #region [ IsNetworkAvailable ]
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// Determines whether the network available is availiable.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the network available is availiable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNetworkAvailable()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                int nDescription;
                return InternetGetConnectedState(out nDescription, 0);
            }

            return false;
        }
        #endregion

        #region [ SimplePing ]
        /// <summary>
        /// Ping network address
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="timeout">Unit in millisecond</param>
        /// <param name="payload">Data to send with the ping</param>
        /// <returns></returns>
        public static bool SimplePing(string address, int timeout, string payload)
        {
            if (Str.IsEmpty(address))
            {
                return false;
            }

            try
            {
                using (Ping ping = new Ping())
                {
                    byte[] data = Encoding.ASCII.GetBytes(payload);
                    PingReply pingReply = ping.Send(address, timeout, data);

                    if (pingReply != null && pingReply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
            }
            catch (Exception xpt)
            {
                //QDebug.Log(xpt.Message, "Ping Exception: ");
            }

            return false;
        }

        /// <summary>
        /// Ping network address
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="timeout">The timeout (in milliseconds).</param>
        /// <param name="payloadLength">Length of the payload.</param>
        /// <returns></returns>
        public static bool SimplePing(string address, int timeout, int payloadLength)
        {
            string data = new string('Q', payloadLength);
            return SimplePing(address, timeout, data);
        }

        /// <summary>
        /// Send one byte of data using the ping protocol, if success return true
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="timeout">The timeout (in milliseconds).</param>
        /// <returns></returns>
        public static bool SimplePing(string address, int timeout)
        {
            return SimplePing(address, timeout, "Q");
        }

        /// <summary>
        /// Simple ping function, send one byte and timeout in 300 millisecond.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public static bool SimplePing(string address)
        {
            return SimplePing(address, 300);
        }
        #endregion

        #region [ WOL ]
        /// <summary>
        /// Wakes the on LAN.
        /// </summary>
        /// <param name="MACAddress">The MAC address (00-1E-8C-B9-84-CE or 001E8CB984CE).</param>
        /// <returns></returns>
        public static bool WakeOnLAN(string MACAddress)
        {
            try
            {
                string mac = "FFFFFFFFFFFF" + Str.Repeat(MACAddress, 16);
                var magicPackage = Hex.ToBytes(mac);

                if (magicPackage.Length != 102)
                {
                    return false;
                }

                var client = new UdpClient();
                client.Connect(IPAddress.Broadcast, 7);

                int byteSent = client.Send(magicPackage, magicPackage.Length);
                return byteSent == magicPackage.Length;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("WakeOnLAN()", xpt);
                return false;
            }
        }
        #endregion

        #region [ GetMacAddress ]
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        /// <summary>
        /// Gets the MAC address.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address.</param>
        /// <returns></returns>
        public static byte[] GetMACAddress(string hostNameOrAddress)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
            if (hostEntry.AddressList.Length == 0)
                return null;

            var macAddr = new byte[6];
            var macAddrLen = (uint)macAddr.Length;
            if (SendARP((int)hostEntry.AddressList[0].Address, 0, macAddr, ref macAddrLen) != 0)
            {
                return null; // The SendARP call failed
            }

            return macAddr;
        }

        /// <summary>
        /// Gets the MAC address as hex.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address.</param>
        /// <returns></returns>
        public static string GetMACAddressAsHex(string hostNameOrAddress)
        {
            var mac = GetMACAddress(hostNameOrAddress);

            if (mac == null)
            {
                return null;
            }
            string macString = ByteArr.ToHex(mac);
            return macString;
        }
        #endregion

        #region [ Get Wifi Signal ]
        /// <summary>
        /// Get Wifi signal strength in percent (1 to 100).
        /// </summary>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static int GetWifiSignalStrength()
        {
            ////Todo: verify make sure it works, remove the attribute when done.

            int strength = -1;
            try
            {
                // Query the management object with the valid scope and the correct query statment
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT Ndis80211ReceivedSignalStrength FROM MSNdis_80211_ReceivedSignalStrength WHERE active=true"))
                {
                    // Call the get in order to populate the collection
                    ManagementObjectCollection adapterObjects = searcher.Get();
                    // Loop though the management object and pull out the signal strength
                    foreach (ManagementObject mo in adapterObjects)
                    {
                        string signalStr = mo["Ndis80211ReceivedSignalStrength"].ToString();

                        int signal = int.Parse(signalStr);
                        int maxSignal = -30;
                        int disassociationSignal = -90;
                        int percentage = 100 - 80 * (maxSignal - signal) / (maxSignal - disassociationSignal);

                        strength = percentage;
                        break;
                    }
                }
            }
            catch (Exception) { }
            return strength;
        }
        #endregion

        #region [ RemoteShutDown ]
        /// <summary>
        /// Remotes the shut down.
        /// </summary>
        /// <param name="computerName">Name of the remote computer.</param>
        /// <param name="force">if set to <c>true</c> force. When set to true the computer do not wait for the user to save their work.</param>
        /// <param name="timeout">The timeout.  Time to wait for user to cancel.</param>
        /// <param name="comment">The comment to display to the user.</param>
        /// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/aa376872(v=vs.85).aspx</remarks>
        public static void RemoteShutDown(string computerName, bool force, int timeout, string comment)
        {
            #region [ Old Code Using ShutDown.exe ]
            string cmd = Path.Combine(Environment.SystemDirectory, "shutdown.exe");
            string args = string.Format(@" /s /m \\{0} /d P:1:1", computerName);

            if (force)
            {
                args += @" /f";
            }

            timeout = Num.Filter(timeout, 0, 600);
            args += string.Format(@" /t {0}", timeout);

            if (!Str.IsEmpty(comment))
            {
                args += string.Format(@" /c ""{0}""", comment.Replace("\"", "\\\""));
            }

            string result = IOFunc.ExecSilence(cmd, args.Trim());
            Console.WriteLine(result);

            
            #endregion

            //if (force)
            //{
            //    return RemoteShutDown(computerName, ShutDownOptions.SHUTDOWN_FORCE_OTHERS | ShutDownOptions.SHUTDOWN_POWEROFF,
            //        timeout, comment);
            //}

            //return RemoteShutDown(computerName, ShutDownOptions.SHUTDOWN_POWEROFF,
            //            timeout, comment);
        }

        /// <summary>
        /// Remotes the shut down.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="option">The shutdown option.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public static bool RemoteShutDown(string computerName, ShutDownOptions option, int timeout, string comment)
        {
            uint SHTDN_REASON_MINOR_WMI = 0x00000015;
            uint grace = Convert.ToUInt32(timeout);

            return InitiateShutdown(computerName, comment, grace, (UInt32)option, SHTDN_REASON_MINOR_WMI);
        }

        // Call InitiateShutdown
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitiateShutdown(string lpMachineName,
                                            string lpMessage,
                                            UInt32 dwGracePeriod,
                                            UInt32 dwShutdownFlags,
                                            UInt32 dwReason);
        #endregion
    }
}
