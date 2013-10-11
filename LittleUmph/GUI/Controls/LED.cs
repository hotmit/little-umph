using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Windows.Forms;

using LittleUmph;

namespace LittleUmph.GUI.Controls
{
    public partial class LED : UserControl
    {
        private bool _On;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LED"/> is on.
        /// </summary>
        /// <value>
        ///   <c>true</c> if on; otherwise, <c>false</c>.
        /// </value>
        [Category("[ LED ]")]
        [Description("Value indicate wheather the led is on.")]
        public bool On
        {
            get { return _On; }
            set
            {
                bool oldValue = _On;
                _On = value;

                if (_On != oldValue)
                {
                    picLED.BackgroundImage = _On ? Properties.Resources.LEDOn : Properties.Resources.LEDOff;

                    if (StatusChanged != null)
                    {
                        StatusChanged(this, _On);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LED"/> is off.
        /// </summary>
        /// <value>
        ///   <c>true</c> if off; otherwise, <c>false</c>.
        /// </value>
        [Category("[ LED ]")]
        [Description("Value indicate wheather the led is off.")]
        public bool Off
        {
            get { return !On; }
            set { On = !value; }
        }

        /// <summary>
        /// Whent the led is change for on to off or vice versa.
        /// </summary>
        /// <param name="led">The led.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        public delegate void StatusChangedHandler(LED led, bool status);

        /// <summary>
        /// Occurs when the led's status changed.
        /// </summary>
        [Category("[ LED ]")]
        [Description("When the led status changed from on to off, or vice versa.")]
        public event StatusChangedHandler StatusChanged;

        public LED()
        {
            CursorChanged += new EventHandler(LED_CursorChanged);

            InitializeComponent();
        }

        void LED_CursorChanged(object sender, EventArgs e)
        {
            picLED.Cursor = Cursor;
        }

        public new event EventHandler Click
        {
            add
            {
                picLED.Click += value;
            }
            remove
            {
                picLED.Click -= value;
            }
        }

        public new event EventHandler DoubleClick
        {
            add
            {
                picLED.DoubleClick += value;
            }
            remove
            {
                picLED.DoubleClick -= value;
            }
        }

        public void Toggle()
        {
            On = !On;
        }
    }
}
