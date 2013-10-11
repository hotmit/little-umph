using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace LittleUmph
{
    public class Clor
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
        /// <see cref="http://www.hgrebdes.com/colour/spectrum/brightnessdiff.html"/>
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
        /// <see cref="http://colaargh.blogspot.co.uk/2012/08/readability-of-type-in-colour-w3c.html"/>
        /// <returns></returns>
        public static int ColorDifference(Color c1, Color c2)
        {
            //(max (Red 1, Red 2) - min (Red 1, Red 2)) + (max (Green 1, Green 2) - min (Green 1, Green 2)) + (max (Blue 1, Blue 2) - min (Blue 1, Blue 2))
            int diff = (Max(c2.R, c2.R) - Min(c1.R, c2.R)) + (Max(c2.G, c2.G) - Min(c1.G, c2.G)) + (Max(c2.B, c2.B) - Min(c1.B, c2.B));
            return diff;
        }

        #region [ Short Func ]
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
