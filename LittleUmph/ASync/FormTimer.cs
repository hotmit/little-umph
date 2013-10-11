using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LittleUmph
{
    /// <summary>
    /// Wrapper for the form timer.
    /// </summary>
    public class FormTimer : ATimer
    {
        #region [ Private Variables ]
        private Timer _formTimer;
        private bool _intialDelayServed;
        #endregion

        #region [ Constructors ]
        public FormTimer()
        {
            TimerType = TimerType.FormTimer;
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

                if (_formTimer == null)
                {
                    _formTimer = new Timer();
                    _formTimer.Tick += timer_TimeElapsed;
                }

                _intialDelayServed = false;
                if (InitialDelay > 0)
                {                    
                    _formTimer.Interval = InitialDelay;
                    _formTimer.Start();
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
                    _formTimer.Interval = Interval;                                        
                    _formTimer.Start();
                }
            }
        }
        #endregion       

        #region [ On Time Elapsed ]
        private void timer_TimeElapsed(object sender, EventArgs e)
        {
            if (!_intialDelayServed)
            {
                _formTimer.Stop();
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
            if (_formTimer != null)
            {
                _formTimer.Stop();
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
            if (_formTimer != null)
            {
                try
                {
                    _formTimer.Disposed += timer_Disposed;
                    _formTimer.Dispose();
                }
                catch (Exception)
                {
                    _formTimer = null;
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
            _formTimer = null;
            onDisposed();
        }
        #endregion
    }
}
