using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Use this to ensure consecutive events do not happen 
    /// too closely to each others.
    /// </summary>
    public class SpreadPattern
    {
        #region [ Private Variables ]
        long _lastExecuted = DateTime.MinValue.Ticks;
        System.Windows.Forms.Timer _tmr = null;
        System.Timers.Timer _sysTmr = null;

        // This value is stored in ticks, to avoid conversion 
        // at time of compare
        private int _minimumSpreadInTicks = 0;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets the minimum spread in miliseconds.
        /// </summary>
        /// <value>
        /// The minimum spread.
        /// </value>
        public int MinimumSpread
        {
            get
            {
                return _minimumSpreadInTicks / Tmr.INT_TicksPerMillisecond;
            }
            set
            {
                _minimumSpreadInTicks = value * Tmr.INT_TicksPerMillisecond;
            }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SpreadPattern" /> class.
        /// </summary>
        /// <param name="minimumMilisecondsDelay">The minimum miliseconds delay.</param>
        public SpreadPattern(int minimumMilisecondsDelay)
        {
            MinimumSpread = minimumMilisecondsDelay;
        }
        #endregion

        #region [ Helper ]
        /// <summary>
        /// Determines whether this instance can be executed 
        /// (ie time between executions is long enough).
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </returns>
        public bool CanExecute()
        {
            if (_lastExecuted == 0)
            {
                return true;  
            }
            return _lastExecuted + _minimumSpreadInTicks > DateTime.Now.Ticks;
        }

        /// <summary>
        /// Marks the execution time.
        /// </summary>
        public void MarkExecution()
        {
            _lastExecuted = DateTime.Now.Ticks;
        }
        #endregion

        #region [ Execute ]
        /// <summary>
        /// Executes the specified action if the time spread is satisfied
        /// and mark the execution time when done.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="executeLastAction">If the last action is too soon,
        /// run the very last action when the spread is satisfied.</param>
        /// <param name="useFormTimer">Use form timer to avoid managing crossthread error.</param>
        public void Execute(Action action, bool executeLastAction = false, bool useFormTimer = true)
        {
            if (CanExecute())
            {
                action();
                MarkExecution();
                return;
            }

            if (executeLastAction)
            {
                ClearOutPreviousQueue();

                long nextExec = (_lastExecuted + _minimumSpreadInTicks) - DateTime.Now.Ticks;
                int delay = Convert.ToInt32(nextExec / Tmr.INT_TicksPerMillisecond);

                // this is too low to wait for a timer
                if (delay < 50)
                {
                    action();
                    return;
                }

                if (useFormTimer)
                {
                    _tmr = Tmr.Run(action, delay);
                }
                else
                {
                    _sysTmr = Tmr.SysTmrRun(action, delay);
                }
            }
        }
        #endregion

        private void ClearOutPreviousQueue()
        {
            // Clear out last queued action, 
            if (_tmr != null)
            {
                _tmr.Stop();
                _tmr.Dispose();
            }

            if (_sysTmr != null)
            {
                _sysTmr.Stop();
                _sysTmr.Dispose();
            }
        }

    }
}
