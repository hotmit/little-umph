using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace LittleUmph
{
    /// <summary>
    /// Wrapper for the system timer.
    /// </summary>
    public class SystemTimer : ATimer
    {
        #region [ Private Variables ]
        private Timer _systemTimer;
        private bool _intialDelayServed;
        #endregion

        #region [ Constructors ]
        public SystemTimer()
        {
            TimerType = TimerType.SystemTimer;
        }
        #endregion

        #region [ Start ]
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

                if (_systemTimer == null)
                {
                    _systemTimer = new Timer();
                    _systemTimer.AutoReset = true;                    
                    _systemTimer.Elapsed += timer_TimeElapsed;
                }

                _intialDelayServed = false;
                if (InitialDelay > 0)
                {                    
                    _systemTimer.Interval = InitialDelay;
                    _systemTimer.Start();
                }
                else
                {
                    _intialDelayServed = true;
                    serveRemainingInterval();
                }
            }
            else
            {
                Stop();
                return false;
            }

            return true;
        }

        private void serveRemainingInterval()
        {
            if (Enabled && _intialDelayServed && RemainingCount > 0)
            {
                // The timer will execute right after the initial deday have been served
                onTimeElapsed();

                // onTimeElapsed() will decrement the RemainingCount, and Enabled may change
                // That's why we need to check it again
                if (Enabled && RemainingCount > 0 && Interval > 0)
                {
                    _systemTimer.Interval = Interval;                                        
                    _systemTimer.Start();
                }
            }
        }
        #endregion       

        #region [ On Time Elapsed ]
        private void timer_TimeElapsed(object sender, EventArgs e)
        {
            if (!_intialDelayServed)
            {
                _systemTimer.Stop();
                _intialDelayServed = true;
                serveRemainingInterval();
            }
            else
            {
                onTimeElapsed();
            }
        }
        #endregion

        #region [ Stop ]
        /// <summary>
        /// Stops this timer.
        /// </summary>
        public override void Stop()
        {
            if (_systemTimer != null)
            {
                _systemTimer.Stop();
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
            if (_systemTimer != null)
            {
                try
                {
                    _systemTimer.Disposed += timer_Disposed;
                    _systemTimer.Dispose();
                }
                catch (Exception)
                {
                    _systemTimer = null;
                    onDisposed();
                }
            }
            else
            {
                onDisposed();
            }
        }

        private void timer_Disposed(object sender, EventArgs e)
        {
            _systemTimer = null;
            onDisposed();
        }
        #endregion
    }
}
