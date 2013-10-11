using System;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Windows.Forms;

namespace LittleUmph.Helper
{
    /// <summary>
    /// Compare two control and grade them based on the screen position.
    /// Top left has the highest rank.
    /// </summary>
    public class ControlLocationComparer : IComparer<Control>
    {
        /// <summary>
        /// Compares the specified first.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns></returns>
        public int Compare(Control first, Control second)
        {
            if (first == second)
            {
                return 0;
            }
            else if (first.Name == second.Name)
            {
                return 0;
            }

            if (Math.Abs(first.Location.Y - second.Location.Y) < 5)
            {
                // X Coordinate: less is closer to the left (ie higher rank)
                if (first.Location.X > second.Location.X)
                {
                    return 1;
                }
                return -1;
            }

            // Y Coordinate: less is closer to the top (ie higher rank)
            if (first.Location.Y > second.Location.Y)
            {
                return 1;
            }
            return -1;
        }
    }
}
