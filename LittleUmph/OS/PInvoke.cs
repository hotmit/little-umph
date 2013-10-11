using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace LittleUmph
{
    #region [ Enum ]
    public enum ScrollDirection
    {
        LineUp = PInvoke.SB_LINEUP, // Scrolls one line up
        CellLeft = PInvoke.SB_LINELEFT,// Scrolls one cell left
        LineDown = PInvoke.SB_LINEDOWN, // Scrolls one line down
        CellRight = PInvoke.SB_LINERIGHT,// Scrolls one cell right
        PageUp = PInvoke.SB_PAGEUP, // Scrolls one page up
        PageLeft = PInvoke.SB_PAGELEFT,// Scrolls one page left
        PageDown = PInvoke.SB_PAGEDOWN, // Scrolls one page down
        PageRight = PInvoke.SB_PAGERIGTH, // Scrolls one page right
        PageTop = PInvoke.SB_PAGETOP, // Scrolls to the upper left
        Left = PInvoke.SB_LEFT, // Scrolls to the left
        PageBottom = PInvoke.SB_PAGEBOTTOM, // Scrolls to the upper right
        Right = PInvoke.SB_RIGHT, // Scrolls to the right
        End = PInvoke.SB_ENDSCROLL // Ends scroll
    }
    #endregion

    public class PInvoke
    {
        #region [ Scroll Programmatically ]
        public const int WM_SCROLL = 276; // Horizontal scroll
        public const int WM_VSCROLL = 277; // Vertical scroll

        public const int SB_LINEUP = 0; // Scrolls one line up
        public const int SB_LINELEFT = 0;// Scrolls one cell left
        public const int SB_LINEDOWN = 1; // Scrolls one line down
        public const int SB_LINERIGHT = 1;// Scrolls one cell right
        public const int SB_PAGEUP = 2; // Scrolls one page up
        public const int SB_PAGELEFT = 2;// Scrolls one page left
        public const int SB_PAGEDOWN = 3; // Scrolls one page down
        public const int SB_PAGERIGTH = 3; // Scrolls one page right
        public const int SB_PAGETOP = 6; // Scrolls to the upper left
        public const int SB_LEFT = 6; // Scrolls to the left
        public const int SB_PAGEBOTTOM = 7; // Scrolls to the upper right
        public const int SB_RIGHT = 7; // Scrolls to the right
        public const int SB_ENDSCROLL = 8; // Ends scroll

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Scrolls the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="direction">The direction.</param>
        public static void Scroll(Control control, ScrollOrientation orientation, ScrollDirection direction)
        {
            int orient = orientation == ScrollOrientation.HorizontalScroll ? WM_SCROLL : WM_VSCROLL;
            Scroll(control, orient, (int)direction);
        }

        /// <summary>
        /// Scrolls the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="direction">The direction.</param>
        public static void Scroll(Control control, int orientation, int direction)
        {
            if (control is DataGridView)
            {
                DataGridView dg = (DataGridView)control;
                foreach (Control c in dg.Controls)
                {
                    try
                    {
                        if (orientation == WM_VSCROLL && c is VScrollBar)
                        {
                            SendMessage(dg.Handle, orientation, (IntPtr)direction, c.Handle);
                            break;
                        }
                        else if (orientation == WM_SCROLL && c is HScrollBar)
                        {
                            SendMessage(dg.Handle, orientation, (IntPtr)direction, c.Handle);
                            break;
                        }
                    }
                    catch (Exception xpt)
                    {
                        Console.WriteLine(xpt.Message);
                    }
                }
            }
            else
            {
                SendMessage(control.Handle, orientation, (IntPtr)direction, IntPtr.Zero);
            }
        }
        #endregion

        #region [ Window Process ]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public static bool IsWindowVisible(Process process)
        {
            return IsWindowVisible(process.Handle);
        }

        public enum GetWindowLongIndex : int
        {
            WNDPROC = (-4),
            HINSTANCE = (-6),
            HWNDPARENT = (-8),
            STYLE = (-16),
            EXSTYLE = (-20),
            USERDATA = (-21),
            ID = (-12)
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Gets the parent window.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        public static IntPtr GetParentWindow(Process process)
        {
            return GetParentWindow(process.MainWindowHandle);
        }

        /// <summary>
        /// Gets the parent window.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public static IntPtr GetParentWindow(IntPtr handle)
        {
            return GetWindowLongPtr(handle, (int)GetWindowLongIndex.HWNDPARENT);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        public static string GetWindowText(Process process)
        {
            return GetWindowText(process.MainWindowHandle);
        }

        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public static string GetWindowText(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return string.Empty;
            }

            int windowTextLength = GetWindowTextLength(handle);
            if (windowTextLength == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(windowTextLength + 1);
            int result = GetWindowText(handle, sb, sb.MaxCapacity);

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether [is window A task] [the specified process].
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>
        /// 	<c>true</c> if [is window A task] [the specified process]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWindowATask(Process process)
        {
            try
            {
                return IsWindowATask(process.MainWindowHandle);
            }
            catch (Exception xpt)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether [is window A task] [the specified handle].
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// 	<c>true</c> if [is window A task] [the specified handle]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWindowATask(IntPtr handle)
        {
            try
            {
                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                bool windowVisible = IsWindowVisible(handle);
                IntPtr parent = GetParentWindow(handle);
                bool hasNoParent = parent == IntPtr.Zero;
                int textLength = GetWindowTextLength(handle);
                bool hasText = textLength > 0;

                return windowVisible && hasNoParent && hasText;
            }
            catch (Exception xpt)
            {
                return false;
            }
        }
        #endregion


        #region [ IsMinimized/IsIconic ]
        /// <summary>
        /// Determines whether the specified hWND is minized (P/Invoke IsIconic).
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <returns>
        /// 	<c>true</c> if the specified h WND is iconic; otherwise, <c>false</c>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "IsIconic")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsMinimized(IntPtr hWnd);
        #endregion

        #region [ FindWindowEx ]
        /// <summary>
        /// The FindWindowEx function retrieves a handle
        /// to a window whose class name and window name 
        /// match the specified strings. The function searches 
        /// child windows, beginning with the one following 
        /// the specified child window. This function does 
        /// not perform a case-sensitive search.
        /// </summary>
        /// <param name="hwndParent">The HWND parent.</param>
        /// <param name="hwndChildAfter">The HWND child after.</param>
        /// <param name="lpszClass">The LPSZ class.</param>
        /// <param name="lpszWindow">The LPSZ window.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// The FindWindowEx function retrieves a handle
        /// to a window whose class name and window name 
        /// match the specified strings. The function searches 
        /// child windows, beginning with the one following 
        /// the specified child window. This function does 
        /// not perform a case-sensitive search.
        /// </summary>
        /// <param name="parentHandle">The parent handle.</param>
        /// <param name="childAfter">The child after.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="windowTitle">The window title.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);
        #endregion

        #region [ GetParent ]
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <returns></returns>
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        #endregion

        #region [ GetWindowThreadProcessId ]
        /// <summary>
        /// Gets the window thread process id.
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <param name="lpdwProcessId">The process id.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        /// <summary>
        /// Gets the window thread process id.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="ProcessId">The process id.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        #endregion

        #region [ Show Window ]
        /// <summary>Shows a Window</summary>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero.
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        /// <summary>
        /// Shows the window async.
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <param name="nCmdShow">WindowShowStyle</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, WindowShowStyle nCmdShow);

        /// <summary>Enumeration of the different ways of showing a window using
        /// ShowWindow</summary>
        public enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window (Value 0).</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized
            /// or maximized, the system restores it to its original size and
            /// position. An application should specify this flag when displaying
            /// the window for the first time (Value 1).</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window (Value 2).</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window (Value 3).</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window (Value 3).</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position.
            /// This value is similar to "ShowNormal", except the window is not
            /// actived (Value 4).</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size
            /// and position (Value 5).</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next
            /// top-level window in the Z order (Value 6).</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is
            /// similar to "ShowMinimized", except the window is not activated (Value 7).</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This
            /// value is similar to "Show", except the window is not activated (Value 8).</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is
            /// minimized or maximized, the system restores it to its original size
            /// and position. An application should specify this flag when restoring
            /// a minimized window (Value 9).</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application (Value 10).</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread
            /// that owns the window is hung. This flag should only be used when
            /// minimizing windows from a different thread (Value 11).</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }
        #endregion

        #region [ StrCmpLogicalW Natural Sort ]
        /// <summary>
        /// This use for natural sort of the filename. ie "hello10" is greater than "hello9"
        /// </summary>
        /// <param name="psz1">The PSZ1.</param>
        /// <param name="psz2">The PSZ2.</param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
        #endregion

        #region [ Set Foreground Window ]
        /// <summary>
        /// Allows the set foreground window.
        /// </summary>
        /// <param name="dwProcessId">The process id.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool AllowSetForegroundWindow(int dwProcessId);

        /// <summary>
        /// Sets the foreground window.
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <returns></returns>
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region [ SetWindowLongPtr ]
        // This static method is required because legacy OSes do not support SetWindowLongPtr 
        /// <summary>
        /// Changes an attribute of the specified window, 
        /// and also sets a value at the specified offset 
        /// in the extra window memory. 
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <param name="nIndex">The offset..</param>
        /// <param name="dwNewLong">The dw new long.</param>
        /// <returns></returns>
        public static IntPtr SetWindowLongPtr(HandleRef hWnd, GWL nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, (int)nIndex, dwNewLong);
            }
            return new IntPtr(SetWindowLong32(hWnd, (int)nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
        #endregion

        #region [ GetWindowLongPtr ]
        // This static method is required because legacy OSes do not support GetWindowLongPtr. 
        /// <summary>
        /// Retrieves information about the specified window, 
        /// including a value at a specified offset into 
        /// the extra window memory. 
        /// </summary>
        /// <param name="hWnd">The hWND.</param>
        /// <param name="nIndex">Index of the n.</param>
        /// <returns></returns>
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex)
        {
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, (int)nIndex);
            }

            return GetWindowLongPtr32(hWnd, (int)nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Specifies the zero-based offset to the value to be retrieved. 
        /// Valid values are in the range zero through the number of 
        /// bytes of extra window memory, minus the size of an integer. 
        /// To retrieve any other value, specify one of the following values.
        /// </summary>
        [Flags]
        public enum GWL
        {
            /// <summary>
            /// Retrieves the pointer to the window procedure, 
            /// or a handle representing the pointer to the 
            /// window procedure. You must use the CallWindowProc 
            /// function to call the window procedure.
            /// </summary>
            GWL_WNDPROC = -4,

            /// <summary>
            /// Retrieves a handle to the application instance.
            /// </summary>
            GWL_HINSTANCE = -6,

            /// <summary>
            /// Retrieves a handle to the parent window, if there is one.
            /// </summary>
            GWL_HWNDPARENT = -8,

            /// <summary>
            /// Retrieves the window styles.
            /// </summary>
            GWL_STYLE = -16,

            /// <summary>
            /// Retrieves the EXTENDED window styles. 
            /// </summary>
            GWL_EXSTYLE = -20,

            /// <summary>
            /// Retrieves the user data associated with the window. 
            /// This data is intended for use by the application 
            /// that created the window. Its value is initially zero.
            /// </summary>
            GWL_USERDATA = -21,

            /// <summary>
            /// Retrieves the identifier of the window.
            /// </summary>
            GWL_ID = -12
        }
        #endregion

        #region [ Show Window ]
        /// <summary>
        /// Shows the window. Modified p/invoke function 
        /// to allow this function to work even with hidden window.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        public static bool ShowWindow(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            bool status = false;

            if (handle != IntPtr.Zero)
            {
                if (IsMinimized(handle))
                {
                    status = ShowWindow(handle, WindowShowStyle.ShowNormal);
                }
            }
            else
            {
                handle = GetHiddenWindow(process.Id);
                if (handle != IntPtr.Zero)
                {
                    status = ShowWindow(handle, WindowShowStyle.ShowNormal);
                }
            }

            AllowSetForegroundWindow(process.Id);
            return SetForegroundWindow(handle) && status;
        }

        #region [ Custom Functions ]

        #region [ GetHiddenWindow ]
        /// <summary>
        /// Gets the hidden window.
        /// </summary>
        /// <param name="processID">The process ID.</param>
        /// <returns></returns>
        public static IntPtr GetHiddenWindow(int processID)
        {
            IntPtr handle = IntPtr.Zero;
            do
            {
                uint pid;
                handle = FindWindowEx(IntPtr.Zero, handle, null, null);
                GetWindowThreadProcessId(handle, out pid);
                if (pid == processID && GetParent(handle) == IntPtr.Zero)
                {
                    return handle;
                }
            } while (!handle.Equals(IntPtr.Zero));

            return IntPtr.Zero;
        }
        #endregion

        #endregion
        #endregion

        #region [ Shutdown Remote & Local PC ]
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InitiateSystemShutdown(
            string lpMachineName,
            string lpMessage,
            uint dwTimeout,
            bool bForceAppsClosed,
            bool bRebootAfterShutdown);


        /// <summary>
        /// Shuts down PC forcefully and do not restart after shutdown.
        /// </summary>
        /// <param name="machineName">Name of the machine. eg. QInvoke.ShutDownPC(@"\\ThePCName").</param>
        /// <returns></returns>
        public static bool ShutDownPC(string machineName)
        {
            return ShutDownPC(machineName, "", 0, true, false);
        }

        /// <summary>
        /// Shuts down local PC.
        /// </summary>
        /// <param name="forceClose">if set to <c>true</c> [force close].</param>
        /// <param name="rebootAfterShutdown">if set to <c>true</c> [reboot after shutdown].</param>
        /// <returns></returns>
        public static bool ShutDownLocalPC(bool forceClose, bool rebootAfterShutdown)
        {
            return ShutDownPC("", "", 30, forceClose, rebootAfterShutdown);
        }

        /// <summary>
        /// Shuts down PC.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="message">The message.</param>
        /// <param name="timeout">The dialog display timeout (measure in second).</param>
        /// <param name="forceClose">if set to <c>true</c> force close.</param>
        /// <param name="rebootAfterShutdown">if set to <c>true</c> reboot after shutdown.</param>
        /// <returns></returns>
        public static bool ShutDownPC(string machineName, string message, int timeout, bool forceClose, bool rebootAfterShutdown)
        {
            return InitiateSystemShutdown(machineName, message, Convert.ToUInt32(timeout), forceClose, rebootAfterShutdown);
        }
        #endregion












        #region [ Unverfied ]
        //[DllImport("user32.dll")]
        //private static extern int SetWindowLong(IntPtr window, int index, int value);

        //[DllImport("user32.dll")]
        //private static extern int GetWindowLong(IntPtr window, int index);

        //private const int GWL_EXSTYLE = -20;
        //private const int WS_EX_TOOLWINDOW = 0x00000080;
        //private const int WS_EX_APPWINDOW = 0x00040000;


        //#region [ Top Most ]
        //private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        //private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        //private const UInt32 SWP_NOSIZE = 0x0001;
        //private const UInt32 SWP_NOMOVE = 0x0002;
        //private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        //[DllImport("user32")]
        //private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
        //    int Y, int cx, int cy, uint uFlags);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="hWnd"></param>
        ///// <param name="topMost"></param>
        ///// <remarks>Usage: QInvoke.WindowTopMost(this.Handle, true);</remarks>
        //public static void WindowTopMost(IntPtr hWnd, bool topMost)
        //{
        //    if (topMost)
        //    {
        //        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        //    }
        //    else
        //    {
        //        SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        //    }
        //}
        //#endregion 
        #endregion
    }

}
