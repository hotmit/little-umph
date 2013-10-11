using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LittleUmph
{
    /// <summary>
    /// Wrapper for the thread timer.
    /// </summary>
    public class ThreadTimer : ATimer
    {
        #region [ Private Variables ]
        private Timer _threadTimer;
        #endregion

        #region [ Constructors ]
        public ThreadTimer()
        {
            TimerType = TimerType.ThreadTimer;
        }
        #endregion

        #region [ Start ]
        /// <summary>
        /// Starts this timer.
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            if (TimeElapsedEvent == null
                || InitialDelay + Interval <= 0)
            {
                Stop();
                return false;
            }

            if (RemainingCount > 0)
            {
                Enabled = true;
                if (_threadTimer == null)
                {
                    _threadTimer = new Timer(timer_TimeElapsed, null, InitialDelay, Interval);
                }
                else
                {
                    _threadTimer.Change(InitialDelay, Interval);
                }
            }
            else
            {
                Stop();
                return false;
            }
            return true;
        }
        #endregion        

        #region [ On Time Elapsed ]
        private void timer_TimeElapsed(object o)
        {
            onTimeElapsed();
        }
        #endregion

        #region [ Stop ]
        /// <summary>
        /// Stops this timer.
        /// </summary>
        public override void Stop()
        {
            if (_threadTimer != null)
            {
                _threadTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            onTimerStopped();
        }
        #endregion

        #region [ Dispose ]
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_threadTimer != null)
            {
                try
                {
                    _threadTimer.Dispose();
                }
                catch (Exception) { }
                finally
                {
                    _threadTimer = null;
                }
            }
            onDisposed();
        }
        #endregion
    }
}
