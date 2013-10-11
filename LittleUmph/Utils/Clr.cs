using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using LittleUmph.Utils.ColorSpaces;

namespace LittleUmph
{
    public class Clr
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hex">Valid hex can be #FFCCFF, FFCCFF or FC0(FFCC00)</param>
        /// <returns></returns>
        public static Color ColorFromHex(string hex)
        {
            Color hexColor = Color.Black;
            hex = hex.Trim().TrimStart('#').Replace(" ", "");
            if (hex.Length == 3)
            {
                hex = string.Format("{0}{0}{1}{1}{2}{2}", hex[0], hex[1], hex[2]);
            }
            hex = "#" + hex;
            
            try
            {
                hexColor = ColorTranslator.FromHtml(hex);
            }
            catch { }

            return hexColor;
        }

        /// <summary>
        /// Convert color to html (#FFCCFF) equivalent string (ColorTranslator.ToHtml(c) wrapper)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ColorToHex(Color c)
        {
            return ColorTranslator.ToHtml(c);
        }

        /// <summary>
        /// Calculate the difference in brighness (125 or greater is good for legibility)
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>        
        /// <returns></returns>
        public static int BrightnessDifference(Color c1, Color c2)
        {
            return Math.Abs(GetBrightness(c1) - GetBrightness(c2));
        }

        /// <summary>
        /// Gets the brightness.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <remarks>http://www.hgrebdes.com/colour/spectrum/brightnessdiff.html</remarks>
        /// <returns></returns>
        public static int GetBrightness(Color c)
        {
            // ((Red value X 299) + (Green value X 587) + (Blue value X 114)) / 1000
            int r = c.R * 299;
            int g = c.G * 587;
            int b = c.B * 114;
            
            return (r + g + b) / 1000;
        }

        /// <summary>
        /// Colors the difference (ideal it should be over 500).
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <remarks>http://colaargh.blogspot.co.uk/2012/08/readability-of-type-in-colour-w3c.html</remarks>
        /// <returns></returns>
        public static int ColorDifference(Color c1, Color c2)
        {
            //(max (Red 1, Red 2) - min (Red 1, Red 2)) + (max (Green 1, Green 2) - min (Green 1, Green 2)) + (max (Blue 1, Blue 2) - min (Blue 1, Blue 2))
            int diff = (Max(c2.R, c2.R) - Min(c1.R, c2.R)) + (Max(c2.G, c2.G) - Min(c1.G, c2.G)) + (Max(c2.B, c2.B) - Min(c1.B, c2.B));
            return diff;
        }

        /// <summary>
        /// CIs the e76 difference.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <remarks>http://en.wikipedia.org/wiki/Color_difference</remarks>
        /// <returns></returns>
        public static double CIE76Difference(Color c1, Color c2)
        {
            double diff = new Lab(c1).CIE76Different(new Lab(c2));
            return Math.Abs(diff);
        }

        /// <summary>
        /// Sets the color for the control.
        /// </summary>
        /// <param name="c">The control.</param>
        /// <param name="bg">The backcolor.</param>
        /// <param name="fg">The forecolor.</param>
        public static void SetColor(Control c, Color bg, Color fg)
        {
            c.BackColor = bg;
            c.ForeColor = fg;
        }

        /// <summary>
        /// Resets the color to the default.
        /// </summary>
        /// <param name="c">The c.</param>
        public static void ResetColor(Control c)
        {
            c.BackColor = Control.DefaultBackColor;
            c.ForeColor = Control.DefaultForeColor;
        }

        /// <summary>
        /// From byte to color.
        /// </summary>
        /// <param name="argb">The ARGB.</param>
        /// <returns></returns>
        public static Color FromByte(byte argb)
        {
            return Color.FromArgb((byte)((argb & -16777216) >> 0x18),
                          (byte)((argb & 0xff0000) >> 0x10),
                          (byte)((argb & 0xff00) >> 8),
                          (byte)(argb & 0xff));
        }

        /// <summary>
        /// Color to byte.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static byte ToByte(Color c)
        {
            return (byte)((c.A << 24) | (c.R << 16) |
                    (c.G << 8) | (c.B << 0));
        }

        /// <summary>
        /// Round the color to the nearest value. 
        /// Ex: Round(Color{85,12,36,24}, 5) => {85,10,35,25}
        /// </summary>
        /// <param name="c">The color.</param>
        /// <param name="nearestInterval">near=10, 7 would equal 10, 11=>10, 16=>20.</param>
        /// <returns></returns>
        public static Color Round(Color c, int nearestInterval)
        {
            try
            {
                int alpha = Rnd(c.A, nearestInterval);
                int red = Rnd(c.R, nearestInterval);
                int green = Rnd(c.G, nearestInterval);
                int blue = Rnd(c.B, nearestInterval);

                return Color.FromArgb(alpha, red, green, blue);
            }
            catch (Exception xpt)
            {
            }
            return Color.Empty;
        }



        #region [ Short Func ]
        private static int Rnd(double value, int nearestInterval)
        {
            int round = System.Convert.ToInt32(Math.Round(value / nearestInterval)) * nearestInterval;
            return Num.MaxFilter(round, 256);
        }

        private static int Max(int num1, int num2)
        {
            return Math.Max(num1, num2);
        }

        private static int Min(int num1, int num2)
        {
            return Math.Min(num1, num2);
        }

        private static int Abs(int num)
        {
            return Abs(num);
        } 
        #endregion
    }
}
