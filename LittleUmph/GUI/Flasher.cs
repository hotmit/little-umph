using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

namespace LittleUmph
{
    /// <summary>
    /// Flash an semi-transparent over an area to highlight and grab userer's attention
    /// </summary>
    public class Flasher
    {
        private static Dictionary<Control, Timer> _timerList = new Dictionary<Control, Timer>();

        /// <summary>
        /// Flash a transparent overlay over the control.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="flashCount"></param>
        /// <param name="interval"></param>
        public static void Flash(Control c, Color color, int flashCount=1, int interval=150)
        {
            if (_timerList.ContainsKey(c))
            {
                ClearFlash(c);
            }

            int currentCount = 0;
            flashCount *= 2;
            Brush brush = new SolidBrush(Color.FromArgb(80, color));

            var tmr = new Timer();

            tmr.Interval = interval;
            tmr.Tick += (o, e) =>
            {
                if (!_timerList.ContainsKey(c))
                {
                    c.Invalidate();
                    return;
                }

                // Paint the color over the control
                if (currentCount % 2 == 0)
                {
                    Graphics g = c.CreateGraphics();                    
                    g.FillRectangle(brush, 0, 0, c.Width, c.Height);
                }
                else
                {
                    // Clear the painted layer
                    c.Invalidate();
                }

                currentCount++;

                if (currentCount > flashCount)
                {
                    Flasher.ClearFlash(c);
                }
            };
            _timerList[c] = tmr;
            tmr.Start();
        }

        public static void ClearFlash(Control c)
        {
            if (_timerList.ContainsKey(c))
            {
                var tmr = _timerList[c];
                _timerList.Remove(c);

                c.Invalidate();
                tmr.Stop();
                tmr.Dispose();
            }
        }
        
    }
}
