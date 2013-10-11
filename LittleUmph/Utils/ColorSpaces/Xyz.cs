using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LittleUmph.Utils.ColorSpaces
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Source: https://github.com/THEjoezack/ColorMine</remarks>
    public class Xyz
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Xyz(Color color)
        {
            FromRgb(color);
        }

        /// <summary>
        /// Convert rgb color to Xyz.
        /// </summary>
        /// <param name="color">The color.</param>
        public void FromRgb(Color color)
        {
            var r = PivotRgb(color.R / 255.0);
            var g = PivotRgb(color.G / 255.0);
            var b = PivotRgb(color.B / 255.0);

            // Observer. = 2°, Illuminant = D65
            X = r*0.4124 + g*0.3576 + b*0.1805;
            Y = r*0.2126 + g*0.7152 + b*0.0722;
            Z = r*0.0193 + g*0.1192 + b*0.9505;
        }

        private double PivotRgb(double n)
        {
            return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100;
        }
    }
}
