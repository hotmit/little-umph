using System;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using System.Threading;
using FTimer = System.Windows.Forms.Timer;
using STimer = System.Timers.Timer;


namespace LittleUmph
{
    public class Tmr
    {
        #region [ Winform Delay Run ]
        /// <summary>
        /// Runs the specified action when the delay is up.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay in miliseconds.</param>
        /// <returns></returns>
        public static FTimer Run(Action action, int delay)
        {
            FTimer tmr = new FTimer();
            tmr.Interval = delay;

            tmr.Tick += (object sender, EventArgs e) =>
            {
                tmr.Stop();

                action();

                tmr.Dispose();
                tmr = null;
            };

            tmr.Start();

            return tmr;
        }
        #endregion

        #region [ SysTmrRun ]
        /// <summary>
        /// Runs the specified action when the delay is up (using system timer).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay in miliseconds.</param>
        /// <returns></returns>
        public static STimer SysTmrRun(Action action, int delay)
        {
            STimer tmr = new STimer();
            tmr.Interval = delay;

            tmr.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                tmr.Stop();

                action();

                tmr.Dispose();
                tmr = null;
            };

            tmr.Start();

            return tmr;
        }
        #endregion

        #region [ ThreadRun ]
        /// <summary>
        /// Runs the specified action when the delay is up (using thread).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay in miliseconds.</param>
        /// <returns></returns>
        public static Thread ThreadRun(Action action, int delay)
        {
            Thread thread = new Thread(new ThreadStart(() =>
                        {
                            System.Threading.Thread.Sleep(delay);
                            action();
                        }));
            thread.Start();
            return thread;
        }
        #endregion

        #region [ ThreadPoolRun ]
        /// <summary>
        /// Runs the specified action when the delay is up (using thread pool).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delay">The delay in miliseconds.</param>
        /// <returns></returns>
        public static void ThreadPoolRun(Action action, int delay)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    Thread.Sleep(delay);
                    action();
                });
        }
        #endregion

        /// <summary>
        /// The number of ticks in one miliseconds.
        /// </summary>
        public const int INT_TicksPerMillisecond = 10000;

        public enum TicksCount : long {
            InMilisecond = INT_TicksPerMillisecond

        }

        /// <summary>
        /// Run a benchmark on a function.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TimeSpan Benchmark(Action<Stopwatch> action)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            action(stopWatch);
            
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            return ts;
        }

    }
}
