using System;
using System.Collections.Generic;
using System.Text;

using LittleUmph;

namespace LittleUmph
{
    /// <summary>
    /// 
    /// </summary>
    public class ChronoTask
    {
        /** Goals:
         *  1. Just delay execute an event.
         *  2. Create a reoccuring event with interval and number of runs.
         *  3. Run multiple calls, but only execute when the last call exceeded a specified time window.
         *  4. Can choose between the 3 types of timer.
         *  5. Can choose if the timer is single instance or not (during process do not invoke another run, similar to form timer)
         *  
         *  Note: By default it should use the form timer.
         **/


        #region [ Run ]
        /// <summary>
        /// Executes the specified function after the delay time.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="delay">The time delay to start (in millisecond).</param>
        public static void Run(Action action, int delay)
        {
            Run(action, delay, TimerType.FormTimer);
        }

        /// <summary>
        /// Executes the specified function after the delay time.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="delay">The time delay to start (in millisecond).</param>
        /// <param name="type">The type of timer to use.</param>
        public static void Run(Action action, int delay, TimerType type)
        {
            ATimer timer = ATimer.CreateTimer(type);
            timer.AutoDispose = true;
            timer.StartSingle(action, delay);
        }
        #endregion

        #region [ Recurring Overload Call ]
        /// <summary>
        /// Execute the delegate multiple times.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="initialDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <returns>The Timer.</returns>
        public static ATimer Recurring(Action action, int initialDelay, int interval, int count)
        {
            return Recurring(action, initialDelay, interval, count, TimerType.FormTimer, false);
        }

        /// <summary>
        /// Execute the delegate multiple times.
        /// </summary>
        /// <param name="timeElapsedFunc">The function to call when the time is up (use this if you want access to the timer when the interval is up).</param>
        /// <param name="initialDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <returns>The Timer.</returns>
        public static ATimer Recurring(TimeElapsed timeElapsedFunc, int initialDelay, int interval, int count)
        {
            return Recurring(timeElapsedFunc, initialDelay, interval, count, TimerType.FormTimer, false);
        }
        #endregion

        #region [ Recurring Actual Code ]
        /// <summary>
        /// Execute the delegate multiple times.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="initialDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <param name="type">The type of timer to use.</param>
        /// <param name="executeConcurrently">if set to <c>true</c> if the first event is processing and the second event fires, the second is also started.
        /// if set to <c>false</c> the first event is processing and the second event fires, the second event is skipped.</param>
        /// <returns>The Timer.</returns>
        public static ATimer Recurring(Action action, int initialDelay, int interval, int count, TimerType type, bool executeConcurrently)
        {
            ATimer timer = ATimer.CreateTimer(type);
            timer.AutoDispose = true;
            timer.StartRecurring(action, initialDelay, interval, count, executeConcurrently);

            return timer;
        }

        /// <summary>
        /// Execute the delegate multiple times.
        /// </summary>
        /// <param name="timeElapsedFunc">The function to call when the time is up.</param>
        /// <param name="initialDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <param name="type">The type of timer to use.</param>
        /// <param name="executeConcurrently">if set to <c>true</c> if the first event is processing and the second event fires, the second is also started.
        /// if set to <c>false</c> the first event is processing and the second event fires, the second event is skipped.</param>
        /// <returns>The Timer.</returns>
        public static ATimer Recurring(TimeElapsed timeElapsedFunc, int initialDelay, int interval, int count, TimerType type, bool executeConcurrently)
        {
            ATimer timer = ATimer.CreateTimer(type);
            timer.AutoDispose = true;
            timer.StartRecurring(timeElapsedFunc, initialDelay, interval, count, executeConcurrently);

            return timer;
        }
        #endregion

        #region [ Spacer ]
        /// <summary>
        /// Guarantee a task do not execute more than once in the specified time window.
        /// </summary>
        /// <param name="action">The function to call when the time is right.</param>
        /// <param name="timeWindow">The minimum time window between each call of the same task (in millisecond).</param>
        public static void Spacer(Action action, int timeWindow)
        {
            Spacer(action, timeWindow, TimerType.SystemTimer);
        }

        /// <summary>
        /// Guarantee a task do not execute more than once in the specified time window.
        /// </summary>
        /// <param name="action">The function to call when the time is right.</param>
        /// <param name="timeWindow">The minimum time window between each call of the same task (in millisecond).</param>
        /// <param name="type">The type of timer to use.</param>
        public static void Spacer(Action action, int timeWindow, TimerType type)
        {
            
        }
        #endregion

        #region [ RunLast ]
        /// <summary>
        /// Queue the requests and only execute the last one when the timer ran out of time.
        /// Use this to avoid redundant request for executing multiple times.
        /// </summary>
        /// <param name="action">The function to call when the wait time have been served.</param>
        /// <param name="waitTime">Do not execute until the request have waited the specified time (in millisecond).</param>
        public static ATimer RunLast(Action action, int waitTime)
        {
            return RunLast(action, waitTime, TimerType.SystemTimer);
        }

        private static Dictionary<Delegate, ATimer> _runLastTimer = new Dictionary<Delegate, ATimer>();

        /// <summary>
        /// Runs the last.
        /// </summary>
        /// <param name="action">The function to call when the time is right.</param>
        /// <param name="waitTime">The wait time.</param>
        /// <param name="type">The type of timer to use.</param>
        /// <returns></returns>
        public static ATimer RunLast(Action action, int waitTime, TimerType type)
        {
            if (!_runLastTimer.ContainsKey(action))
            {
                ATimer timer = ATimer.CreateTimer(type);
                timer.AutoDispose = true;
                timer.ThreadSafeInvoke = false;
                timer.ExecuteConcurrently = false;
                timer.Completed += delegate(ATimer atimer)
                                    {
                                        lock (_runLastTimer)
                                        {
                                            _runLastTimer.Remove(action);
                                        }
                                        atimer = null;
                                    };

                _runLastTimer[action] = timer;
                timer.StartSingle(action, waitTime);
                return timer;
            }
            else
            {
                _runLastTimer[action].Interval = waitTime;
                _runLastTimer[action].Restart();
                return _runLastTimer[action];
            }
        }
        #endregion
    }
}
