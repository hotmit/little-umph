using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// 
    /// </summary>
    public class BackOffPattern
    {
        #region [ Private Variables ]
        private long _readyTime = 0;
        private const int TicksPerMillisecond = 10000;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets or sets the min wait (in millisecond).
        /// </summary>
        /// <value>The min wait.</value>
        public int StartWaitingTime { get; set; }

        /// <summary>
        /// Gets or sets the max wait (in millisecond).
        /// </summary>
        /// <value>The max wait.</value>
        public int MaxWaitingTime { get; set; }

        /// <summary>
        /// Gets or sets the factor to push back the time.
        /// </summary>
        /// <value>The back off factor.</value>
        /// <example>if Factor is 2, that mean the waiting time will double everytime.</example>
        public double Factor { get; set; }

        /// <summary>
        /// Gets or sets the current wait time (in millisecond).
        /// </summary>
        /// <value>The current wait time.</value>
        public int CurrentWaitingTime { get; set; }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Create a back off pattern with the initial waiting time is 500ms, 
        /// max out at 30 seconds and a back off factor of 1.1.
        /// /// </summary>
        public BackOffPattern() : this(500, 30000, 1.2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackOffPattern"/> class.
        /// </summary>
        /// <param name="startWaitingTime">The start waiting time (in milliseconds).</param>
        /// <param name="maxWaitingTime">The maximum waiting time (in milliseconds).</param>
        /// <param name="backOffFactor">The back off factor. If the factor is 2, that mean the waiting time will double everytime.</param>
        public BackOffPattern(int startWaitingTime, int maxWaitingTime, double backOffFactor)
        {
            StartWaitingTime = startWaitingTime;
            MaxWaitingTime = maxWaitingTime;
            Factor = backOffFactor;

            _readyTime = 0;
            CurrentWaitingTime = startWaitingTime;
        }
        #endregion

        #region [ Is Ready ]
        /// <summary>
        /// Determines whether it is ok to run.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> the waiting time already passed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReady()
        {
            return _readyTime == 0 || DateTime.Now.Ticks > _readyTime;
        }
        #endregion

        #region [ Push Back ]
        /// <summary>
        /// Delay further the time to wait for the next execution. This should be called when an error occured.
        /// </summary>
        public void PushBack()
        {
            if (_readyTime != 0)            
            {
                CurrentWaitingTime = Math.Min(Convert.ToInt32(CurrentWaitingTime * Factor), MaxWaitingTime);
            }
            _readyTime = DateTime.Now.Ticks + (CurrentWaitingTime * TicksPerMillisecond);
        }
        #endregion

        #region [ Reset ]
        /// <summary>
        /// Run this once got successfully run.
        /// </summary>
        public void Reset()
        {
            _readyTime = 0;
            CurrentWaitingTime = StartWaitingTime;
        }
        #endregion
    }
}
