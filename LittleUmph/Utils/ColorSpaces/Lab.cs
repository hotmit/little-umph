using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LittleUmph.Utils.ColorSpaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>https://github.com/THEjoezack/ColorMine</remarks>
    public class Lab
    {
        // Observer= 2°, Illuminant= D65
        private const double RefX = 95.047;
        private const double RefY = 100.000;
        private const double RefZ = 108.883;

        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        public Lab(Color color)
        {
            FromRgb(color);
        }

        /// <summary>
        /// Convert rgb color into Lab.
        /// </summary>
        /// <param name="color">The color.</param>
        public void FromRgb(Color color)
        {
            var xyz = new Xyz(color);

            var x = PivotXyz(xyz.X / RefX);
            var y = PivotXyz(xyz.Y / RefY);
            var z = PivotXyz(xyz.Z / RefZ);

            L = Math.Max(0, 116 * y - 16);
            A = 500 * (x - y);
            B = 200 * (y - z);
        }

        /// <summary>
        /// CIE76 different.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public double CIE76Different(Color color)
        {
            return CIE76Different(new Lab(color));
        }

        /// <summary>
        /// CIE76 different.
        /// </summary>
        /// <param name="lab">The lab.</param>
        /// <remarks>http://en.wikipedia.org/wiki/Color_difference</remarks>
        /// <returns></returns>
        public double CIE76Different(Lab lab)
        {
            var differences = Distance(L, lab.L) + Distance(A, lab.A) + Distance(B, lab.B);
            return Math.Sqrt(differences);
        }

        #region [ Helper ]
        private static double Distance(double a, double b)
        {
            return Math.Pow(a - b, 2);
        }

        private static double PivotXyz(double n)
        {
            var i = CubicRoot(n);
            return n > 0.008856 ? i : 7.787 * n + 16 / 116;
        }

        private static double CubicRoot(double n)
        {
            return Math.Pow(n, (1.0 / 3.0));
        }
        #endregion
    }
}
