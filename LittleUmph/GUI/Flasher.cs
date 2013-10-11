using System.Drawing;
using System.Windows.Forms;

namespace LittleUmph
{
    /// <summary>
    /// Flash an semi-transparent over an area to highlight and grab userer's attention
    /// </summary>
    public class Flasher
    {
        ///// <summary>
        ///// Flash the control 
        ///// </summary>
        ///// <param name="c"></param>
        ///// <param name="color"></param>
        ///// <param name="flashCount"></param>
        ///// <param name="interval"></param>
        ///// <returns>You can use the timer to stop the loop, 
        ///// however when the count lapses the timer will be dispose 
        ///// automatically by this function</returns>
        //public static Timer Flash(Control c, Color color, int flashCount, int interval)
        //{            
        //    DynamicObject doc = new DynamicObject(
        //            "Control", c,
        //            "Color", Color.FromArgb(80, color)
        //        );

        //    return FTimer.ExecuteOnAnInterval(flash_Tick, doc, (flashCount-1) * 2, interval, true);
        //}

        ///// <summary>
        ///// Flash twice at 120ms interval
        ///// </summary>
        ///// <param name="c"></param>
        ///// <param name="color"></param>
        ///// <returns></returns>
        //public static Timer Flash(Control c, Color color)
        //{
        //    return Flash(c, color, 2, 120);
        //}
        
        ///// <summary>
        ///// Assisted function for "Flash" function
        ///// </summary>
        ///// <param name="doc"></param>
        //private static void flash_Tick(DynamicObject doc)
        //{
        //    Timer tmr = doc.Get<Timer>("Timer", null);
        //    Control c = doc.VCtrl("Control");
        //    if (tmr == null)
        //    {
        //        c.Invalidate();
        //        return;
        //    }

        //    // Paint the color over the control
        //    if (doc.VInt("CurrentCount") % 2 == 0)
        //    {
        //        Graphics g = c.CreateGraphics();
        //        Color color = doc.Get<Color>("Color", Color.Red);
        //        Brush brush = new SolidBrush(color);
                
        //        g.FillRectangle(brush, 0, 0, c.Width, c.Height);
        //    }                
        //    else
        //    {
        //        // Clear the painted layer
        //        c.Invalidate();
        //    }
        //}

    }
}
