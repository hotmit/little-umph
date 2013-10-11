using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;

namespace MTool
{
    public class Transition
    {
        public static Timer SlideUp(Form form, int height)
        {
            return SlideUp(form, height, 30, 14);
        }

        public static Timer SlideUp(Form form, int height, int interval, int step)
        {
            Timer tmr = new Timer();
            tmr.Interval = interval;

            tmr.Tick += (o, e) =>
                {
                    if (form.Height < height)
                    {
                        form.Top -= step;
                        form.Height += step;

                        if (form.Height + step > height)
                        {
                            form.Top -= height - form.Height;
                            form.Height = height;
                            tmr.Stop();
                        }
                    }
                    else
                    {
                        form.Top -= height - form.Height;
                        form.Height = height;
                        tmr.Stop();
                    }
                };
            tmr.Start();
            return tmr;
        }
    }
}
