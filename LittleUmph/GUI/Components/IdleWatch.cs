using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
#if !NET20
using System.Linq;
#endif
using System.Text;
using System.Runtime.InteropServices;

using LittleUmph;

namespace LittleUmph.GUI.Components
{
    public partial class IdleWatch : Component, ISupportInitialize  
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private int _idleTimeInSeconds;

        /// <summary>
        /// The time to trigger the idle event.
        /// </summary>
        [Category("[ IdleWatch ]")]
        [Description("The time to trigger the idle event.")]
        public int IdleTime { get; set; }

        public enum TimeUnit : int
        {
            Second = 10,
            Minute = 100,
            Hour = 1000
        }

        /// <summary>
        /// Time unit
        /// </summary>
        [Category("[ IdleWatch ]")]
        [Description("Time unit")]
        public TimeUnit IdleTimeUnit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="watcher">The watcher.</param>
        /// <param name="idleTime">The idle time in seconds.</param>
        public delegate void IdleOccurredHandler(IdleWatch watcher, int idleTime);

        /// <summary>
        /// Occurs when idle occurred.
        /// </summary>
        [Category("[ IdleWatch ]")]
        [Description("When the idle time exceeds the IdleTime limit.")]
        public event IdleOccurredHandler IdleOccurred;

        public IdleWatch()
        {
            InitializeComponent();
        }

        public IdleWatch(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// Gets the idle time in seconds.
        /// </summary>
        /// <returns></returns>
        public int GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            var diff = (((uint)Environment.TickCount) - lastInPut.dwTime);
            int seconds = Convert.ToInt32(diff / 1000);

            return seconds;
        }


        public void BeginInit()
        {
        }

        public void EndInit()
        {
            switch (IdleTimeUnit)
            {
                case TimeUnit.Second:
                    _idleTimeInSeconds = IdleTime;
                    break;
                case TimeUnit.Minute:
                    _idleTimeInSeconds = IdleTime * 60;
                    break;
                case TimeUnit.Hour:
                    _idleTimeInSeconds = IdleTime * 3600;
                    break;
                default:
                    _idleTimeInSeconds = Int32.MaxValue;
                    break;
            }

            // Example calculations:
            // 10s => interval is 1000ms
            // 10m => 6000ms
            // 10h => 6min
            int factor = (int)IdleTimeUnit;

            int interval = Num.MinFilter((_idleTimeInSeconds * 1000) / factor, 1000);
            tmrWatcher.Interval = interval;
            tmrWatcher.Start();
        }

        private void tmrWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrWatcher.Stop();

                int idleTime = GetIdleTime();
                if (idleTime > _idleTimeInSeconds)
                {
                    Dlgt.Invoke(IdleOccurred, this, idleTime);
                }
            }
            finally
            {
                tmrWatcher.Start();
            }
        }
    }
}
