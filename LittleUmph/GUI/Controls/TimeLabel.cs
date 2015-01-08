using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LittleUmph.GUI.Controls
{
    public partial class TimeLabel : Label
    {                
        #region [ Private Variables ]
        private bool _enable = true;
        private string _format = "dd/MM/yyyy hh:mm:ss tt";
        private DateTime _value = DateTime.Now;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Define how the date and time is formated.
        /// </summary>
        public string Format
        {
            get { return _format; }
            set
            {                
                _format = value;
            }
        }

        /// <summary>
        /// The current time on the label.
        /// </summary>
        public DateTime Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Indicate whether this control is active and keep track of time.
        /// </summary>
        public bool Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
            }
        }

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The text associated with this control.
        /// </returns>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = Value.ToString(_format);
            }
        }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLabel"/> class.
        /// </summary>
        public TimeLabel()
        {
            InitializeComponent();

            TimeLabel.ActivateTimer();
            TimeLabel.TimeUpdated += TimeLabel_TimeUpdated;
            base.Text = Value.ToString(_format);
        }
        #endregion

        #region [ Time Update ]
        protected void TimeLabel_TimeUpdated()
        {
            if (Enable)
            {
                Value = DateTime.Now;
                try
                {
                    string timeString = Value.ToString(_format);
                    if (timeString != Text)
                    {
                        base.Text = timeString;
                    }
                }
                catch
                {
                    base.Text = "Error";
                }
            }
        }
        #endregion

        #region [ Static Scope ]
        protected static Timer _timer;
        private static int _refreshInterval = 1000;

        /// <summary>
        /// Gets or sets the refresh interval.
        /// </summary>
        /// <value>The refresh interval.</value>
        public static int RefreshInterval
        {
            get
            {
                return _refreshInterval;
            }
            set
            {
                if (_refreshInterval != value || _timer == null)
                {
                    _refreshInterval = value;

                    if (_timer == null)
                    {
                        _timer = new Timer();
                        _timer.Tick += new EventHandler(timer_Tick);
                    }                    
                    _timer.Enabled = false;
                    _timer.Interval = _refreshInterval;

                    RefreshTimer();
                    _timer.Enabled = true;
                }
            }
        }

        public delegate void TimeUpdatedHandler();
        public static event TimeUpdatedHandler TimeUpdated;

        protected static void timer_Tick(object sender, EventArgs e)
        {
            if (TimeUpdated != null)
            {
                TimeUpdated();
            }
        }

        /// <summary>
        /// Activates the timer.
        /// </summary>
        protected static void ActivateTimer()
        {
            RefreshInterval = _refreshInterval;
        }

        /// <summary>
        /// Manually refreshes the value of timer, intead of waiting 
        /// till the next clock cycle.
        /// </summary>
        public static void RefreshTimer()
        {
            timer_Tick(null, null);
        }
        #endregion
    }
}
