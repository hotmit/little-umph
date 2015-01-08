using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace LittleUmph
{
    #region [ TimerType Enum ]
    /// <summary>
    /// Types of timer.
    /// </summary>
    public enum TimerType
    {
        /// <summary>
        /// Use UI Thread, if the UI got tied up the timer will be delay.
        /// The nice thing is we don't have to worry about crossthread issue.
        /// </summary>
        FormTimer,

        /// <summary>
        /// This timer uses ThreadPool. It is less resource intensive than ThreadTimer.
        /// </summary>
        SystemTimer,

        /// <summary>
        /// Reliable timer, when timing is critical use this timer.
        /// </summary>
        ThreadTimer
    }
    #endregion

    #region [ Timer Delegates ]
    /// <summary>
    /// When the time is up, the timer instance will invoke this delegate.
    /// </summary>
    public delegate void TimeElapsed(ATimer timer);

    /// <summary>
    /// Event handler for timer.
    /// </summary>
    public delegate void TimerHandler(ATimer timer);
    #endregion    

    /// <summary>
    /// A wrapper class for all three different type of timers. This create a 
    /// consistent behavior accross all three.
    /// 
    /// Features Implemeted:
    ///     (1) Initial delay similar to dueTime in thread timer.
    ///     (2) Single elapsed event at a time, like the form timer.
    ///     (3) Re-start will serve the initial delay again.
    ///     (4) Timer will automatically stop when the RemainingCount is zero.
    ///     (5) Calling start will reset the timer (inital delay &amp; the interval)
    ///         but not reset the RemainingCount.
    ///     (6) Dispose when the timer stopped when AutoDispose is true.
    /// </summary>
    public abstract class ATimer : IDisposable
    {
        #region [ Static Functions ]
        /// <summary>
        /// Creates the timer based on the requested type.
        /// </summary>
        /// <param name="type">The type of timer to create.</param>
        /// <returns></returns>
        public static ATimer CreateTimer(TimerType type)
        {
            switch (type)
            {
                case TimerType.FormTimer:
                    return new FormTimer();
                case TimerType.SystemTimer:
                    return new SystemTimer();
                case TimerType.ThreadTimer:
                    return new ThreadTimer();
                default:
                    throw new Exception("Invalid Timer type.");
            }
        }
        #endregion

        #region [ Constants ]
        /// <summary>
        /// The value to indicate that the timer should loop forever until the user specifically command it to stop.
        /// </summary>
        public const int InfiniteCount = int.MaxValue;
        #endregion

        #region [ Private Variables ]
        protected bool _noParametersDelegate;
        protected Delegate _timeElapsedEvent;
        private bool _threadSafeInvoke = true;
        private int _initialDelay;
        private int _interval;
        private bool _enabled;
        private int _remainingCount;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets a value indicating whether this TimeElapsedEvent is still processing.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if still processing; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessing { get; protected set; }

        /// <summary>
        /// Gets or sets the time elapsed event delegate.
        /// </summary>
        /// <value>The time elapsed event.</value>
        public Delegate TimeElapsedEvent
        {
            get
            {
                return _timeElapsedEvent;
            }
            set
            {
                if (isValidDelegate(value))
                {
                    _timeElapsedEvent = value;
                    if (_timeElapsedEvent != null)
                    {
                        _noParametersDelegate = _timeElapsedEvent.Method.GetParameters().Length == 0;
                    }
                }
                else
                {
                    throw new Exception("Invalid delegate. TimeElapsedEvent must be Action() or TimeElapsed(ATimer timer) only");
                }
            }
        }

        /// <summary>
        /// Gets The time delay to start the first call (in millisecond)..
        /// </summary>
        /// <value>The initial delay.</value>
        public int InitialDelay
        {
            get
            {
                return _initialDelay;
            }
            set
            {
                _initialDelay = value;
                if (Enabled)
                {
                    Restart();
                }
            }
        }

        /// <summary>
        /// Gets executing the interval (in millisecond).
        /// </summary>
        /// <value>The interval.</value>
        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                _interval = value;
                if (Enabled)
                {
                    Restart();
                }
            }
        }

        /// <summary>
        /// Gets or sets the executing count remaining.
        /// </summary>
        /// <value>The remaining count.</value>
        public int RemainingCount
        {
            get { return _remainingCount; }
            set
            {
                _remainingCount = Math.Max(0, value);
                if (_remainingCount == 0)
                {
                    onCompleted();
                }
            }
        }        

        /// <summary>
        /// Gets or sets a value indicating whether to queue the events.
        /// </summary>
        /// <value><c>true</c> if queue event; otherwise, <c>false</c>.</value>
        public bool ExecuteConcurrently { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to dispose when the timer is stopped (false by default).
        /// </summary>
        /// <value><c>true</c> if dispose when the timer is stopped; otherwise, <c>false</c>.</value>
        public bool AutoDispose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to count the interval that was skipped due to the ExecuteConcurrently clause.
        /// </summary>
        /// <value><c>true</c> to decrement the RemainingCount even when we skip an interval; otherwise, <c>false</c>.</value>
        public bool CountMissedFires { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this timer is running.
        /// Use Stop &amp; Start to enable or disable the timer.
        /// </summary>
        /// <value>
        /// <c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            protected set
            {
                bool oldEnable = _enabled;
                _enabled = value;

                if (oldEnable != _enabled)
                {
                    onEnableChanged();
                }

                if (_enabled)
                {
                    onStarted();
                }
                else
                {
                    onStopped();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the timer.
        /// </summary>
        /// <value>The type of the timer.</value>
        public TimerType TimerType { get; protected set; }

        /// <summary>
        /// Gets or sets the name to indentify the Timer.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the generic tag object associated with the timer.
        /// </summary>
        /// <value>The tag.</value>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether invoke the event using thread safe method (True by default).
        /// </summary>
        /// <value>
        /// <c>true</c> to use threadsafe method; otherwise, <c>false</c>.</value>
        public bool ThreadSafeInvoke
        {
            get { return _threadSafeInvoke; }
            set { _threadSafeInvoke = value; }
        }
        #endregion

        #region [ Events Delaration ]
        /// <summary>
        /// Occurs when timer started.
        /// </summary>
        public event TimerHandler Started;

        /// <summary>
        /// Occurs when timer stopped.
        /// </summary>
        public event TimerHandler Stopped;

        /// <summary>
        /// Occurs when timer timer toggle between start and stop.
        /// </summary>
        public event TimerHandler EnableChanged;

        /// <summary>
        /// Occurs when timer is disposed.
        /// </summary>
        public event TimerHandler Disposed;

        /// <summary>
        /// Occurs when the RemainingCount is equal to zero.
        /// </summary>
        public event TimerHandler Completed;
        #endregion

        #region [ Abstract Start & Stop Functions ]
        /// <summary>
        /// Starts this timer. Calling Start() while the timer
        /// is already started the InitialDelay and Interval will be resetted.
        /// However the RemainingCount will not be touch.
        /// </summary>
        /// <returns></returns>
        public abstract bool Start();

        /// <summary>
        /// Stops this timer.
        /// </summary>
        public abstract void Stop();
        #endregion

        #region [ Start Single ]
        /// <summary>
        /// Starts a one time execute timer.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="delay">The time delay to start (in millisecond).</param>
        public void StartSingle(Action action, int delay)
        {
            TimeElapsedEvent = action;
            InitialDelay = delay;
            Interval = 0;
            RemainingCount = 1;
            Start();
        }
        #endregion

        #region [ Start Recurring ]
        /// <summary>
        /// Setup and start the multiple run timer.
        /// </summary>
        /// <param name="action">The function to call when the time is up.</param>
        /// <param name="initalDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <param name="executeConcurrently">if set to <c>true</c> execute concurrently.</param>
        public void StartRecurring(Action action, int initalDelay, int interval, int count, bool executeConcurrently)
        {
            startRecurring(action, initalDelay, interval, count, executeConcurrently);
        }

        /// <summary>
        /// Setup and start the multiple run timer.
        /// </summary>
        /// <param name="timeElapsedFunc">The function to call when the time is up.</param>
        /// <param name="initalDelay">The time delay to start the first call (in millisecond).</param>
        /// <param name="interval">The interval, time to wait between each call (in millisecond).</param>
        /// <param name="count">The count, number of time to repeat.</param>
        /// <param name="executeConcurrently">if set to <c>true</c> execute concurrently.</param>
        public void StartRecurring(TimeElapsed timeElapsedFunc, int initalDelay, int interval, int count, bool executeConcurrently)
        {
            startRecurring(timeElapsedFunc, initalDelay, interval, count, executeConcurrently);
        }

        protected void startRecurring(Delegate timeElapsedEvent, int initalDelay, int interval, int count, bool executeConcurrently)
        {
            TimeElapsedEvent = timeElapsedEvent;
            InitialDelay = initalDelay;
            Interval = interval;
            RemainingCount = count;
            ExecuteConcurrently = executeConcurrently;
            Start();
        }
        #endregion

        #region [ Restart ]
        /// <summary>
        /// Restarts this timer.
        /// </summary>
        public void Restart()
        {
            Start();
        }
        #endregion

        #region [ Run Task ]
        /// <summary>
        /// Runs the current task, just like when the timer elapsed.
        /// </summary>
        /// <returns></returns>
        public void RunTask()
        {
            onTimeElapsed();
        }

        /// <summary>
        /// Asyncs the run the current task, just like when the timer elapsed.
        /// </summary>
        public void AsyncRunTask()
        {
            ThreadPool.QueueUserWorkItem(delegate(object o) { RunTask(); });
        }
        #endregion

        #region [ Get Tag ]
        /// <summary>
        /// Gets the tag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTag<T>()
        {
            return GetTag<T>(default(T));
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T GetTag<T>(T defaultValue)
        {
            if (Tag == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(Tag, typeof(T));
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("ATimer.GetTag<T>()", xpt.Message);
                return defaultValue;
            }
        }
        #endregion

        #region [ Event Handlers ]
        private void onStopped()
        {
            Dlgt.Invoke(ThreadSafeInvoke, Stopped, this);
        }

        private void onStarted()
        {
            Dlgt.Invoke(ThreadSafeInvoke, Started, this);
        }

        private void onEnableChanged()
        {
            Dlgt.Invoke(ThreadSafeInvoke, EnableChanged, this);
        }

        private void onCompleted()
        {
            Dlgt.Invoke(ThreadSafeInvoke, Completed, this);
        }
        #endregion

        #region [ On Time Elapsed ]
        protected void onTimeElapsed()
        {
            #region [ Check Remaining Count ]
            if (RemainingCount <= 0)
            {
                Stop();
                return;
            }
            #endregion

            #region [ Concurring Execution ]
            if (!ExecuteConcurrently)
            {
                if (IsProcessing)
                {
                    if (CountMissedFires)
                    {
                        RemainingCount--;
                    }
                    return;
                }
            }
            #endregion

            IsProcessing = true;
            try
            {
                if (TimeElapsedEvent != null)
                {
                    if (_noParametersDelegate)
                    {
                        Dlgt.Invoke(ThreadSafeInvoke, TimeElapsedEvent);
                    }
                    else
                    {
                        Dlgt.Invoke(ThreadSafeInvoke, TimeElapsedEvent, this);
                    }

                    if (RemainingCount != InfiniteCount)
                    {
                        RemainingCount--;
                    }

                    if (RemainingCount <= 0)
                    {
                        Stop();
                    }
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("ATimer.onTimeElapsed()", xpt.Message);
            }
            finally
            {
                IsProcessing = false;
            }
        }
        #endregion

        #region [ On Timer Stopped ]
        protected void onTimerStopped()
        {
            Enabled = false;
            if (AutoDispose)
            {
                Dispose();
            }
        }
        #endregion

        #region [ On Disposed ]
        protected void onDisposed()
        {
            Dlgt.Invoke(ThreadSafeInvoke, Disposed, this);
        }
        #endregion

        #region [ Private Functions ]
        /// <summary>
        /// Determines whether the specified func is an acceptable delegate type.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns>
        /// 	<c>true</c> if the specified func is an acceptable delegate type; otherwise, <c>false</c>.
        /// </returns>
        protected bool isValidDelegate(Delegate func)
        {
            if (func == null)
            {
                return false;
            }

            MethodInfo method = func.Method;
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                return true;
            }
            else if (parameters.Length == 1
                && parameters[0].ParameterType == typeof(ATimer))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region [ IDisposable Members ]
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
        #endregion
    }
}
