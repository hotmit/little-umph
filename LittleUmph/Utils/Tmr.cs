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
    /// <summary>
    /// Time related or thread functions
    /// </summary>
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

        /// <summary>
        /// Run the action() when the time is up, and run onCompleted once
        /// action() is done.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="onCompleted">The on completed.</param>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        public static FTimer Run(Action action, Action onCompleted, int delay = 10)
        {
            return Run(() =>
            {
                action();
                onCompleted();
            }, delay);
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

        /// <summary>
        /// Runs the specified action when the delay is up (using system timer).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onCompleted"></param>
        /// <param name="delay"></param>
        /// <returns>System.Timer</returns>
        public static STimer SysTmrRun(Action action, Action onCompleted, int delay = 10)
        {
            return SysTmrRun(() =>
            {
                action();
                onCompleted();
            }, delay);
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
                            if (delay > 0)
                            {
                                System.Threading.Thread.Sleep(delay);
                            }
                            action();
                        }));
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Runs the specified action when the delay is up (using thread).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onCompleted"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static Thread ThreadRun(Action action, Action onCompleted, int delay = 0)
        {
            return ThreadRun(() =>
            {
                action();
                onCompleted();
            }, delay);
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
                    if (delay > 0)
                    {
                        Thread.Sleep(delay);
                    }
                    action();
                });
        }

        /// <summary>
        /// Runs the specified action when the delay is up (using thread pool).
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onCompleted"></param>
        /// <param name="delay"></param>
        public static void ThreadPoolRun(Action action, Action onCompleted, int delay = 0)
        {
            ThreadPoolRun(() =>
            {
                action();
                onCompleted();
            }, delay);
        }
        #endregion

        #region [ Constants ]
        /// <summary>
        /// The number of ticks in one miliseconds.
        /// </summary>
        public const int INT_TicksPerMillisecond = 10000;

        public enum TicksCount : long
        {
            InMilisecond = INT_TicksPerMillisecond

        }
        #endregion

        #region [ Benchmark ]
        /// <summary>
        /// Run a benchmark on a block of code.        
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TimeSpan Benchmark(Action<Stopwatch> action)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            action(stopWatch);

            stopWatch.Stop();
            return stopWatch.Elapsed;
        }
        #endregion


    }
}
