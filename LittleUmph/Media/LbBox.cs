using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using LittleUmph;

namespace LittleUmph.Media
{
    /// <summary>
    /// A region of the LbBitmap.
    /// </summary>
    public class LbBox
    {
        LbBitmap _source;

        /// <summary>
        /// Gets or sets the box dimensions and coordinates.
        /// </summary>
        /// <value>
        /// The rect.
        /// </value>
        public Rectangle Box { get; set; }

        /// <summary>
        /// Gets or sets the lock bits (bitmap data from lockbits method).
        /// </summary>
        /// <value>
        /// The lock bits.
        /// </value>
        public BitmapData BitmapData { get; set; }

        /// <summary>
        /// Gets or sets the array that contains pixels of this region/box.
        /// </summary>
        /// <value>
        /// The pixels data.
        /// </value>
        public byte[] Pixels { get; set; }

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="LbBox" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public LbBox(LbBitmap source, int x, int y, int width, int height)
        {
            _source = source;
            Box = new Rectangle(x, y, width, height);
        }
        #endregion

        #region [ Convertion ]
        /// <summary>
        /// Take the poin in this box and convert 
        /// to the absolute location on the image.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public Point ToAbsoluteLocation(int x, int y)
        {
            return new Point(x + Box.X, y + Box.Y);
        }

        /// <summary>
        /// Convert point from the absolute location on the 
        /// image to the location in this box.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Return Point.Empty if the new point is out of bounds.</returns>
        public Point FromAbsoluteLocation(int x, int y)
        {
            Point point = new Point(x - Box.X, y - Box.Y);
            if (x < 0 || y < 0 || x >= Box.Width || y >= Box.Height)
            {
                return Point.Empty;
            }
            return point;
        }
        #endregion

        /// <summary>
        /// Locks this region of the bitmap.
        /// </summary>
        public void Lock()
        {
            BitmapData = _source.Bitmap.LockBits(Box, _source.LockMode,
                             _source.Bitmap.PixelFormat);

            int length = _source.TotalPixels * _source.BytesPerPixel;
            Pixels = new byte[length];

            // Copy data from pointer to array
            Marshal.Copy(BitmapData.Scan0, Pixels, 0, Pixels.Length);
        }

        public void Save()
        {
            if (_source.LockMode == ImageLockMode.ReadWrite
                || _source.LockMode == ImageLockMode.WriteOnly)
            {
                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, BitmapData.Scan0, Pixels.Length);
            }
        }

        /// <summary>
        /// Release the lock on this region of the bitmap.
        /// </summary>
        public void UnLock()
        {
            // Unlock bitmap data
            _source.Bitmap.UnlockBits(BitmapData);
        }

        /// <summary>
        /// Gets the pixel in this box (relative to the box not the image).
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public Color GetPixel(int x, int y)
        {
            var clr = Color.Empty;

            // Get start index of the specified pixel
            int i = ((y * Box.Width) + x) * _source.BytesPerPixel;

            if (i > Pixels.Length - _source.BytesPerPixel)
                throw new IndexOutOfRangeException();

            if (_source.Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (_source.Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (_source.Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Sets the pixel in this box (relative to the box not the image).
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public void SetPixel(Color color, int x, int y)
        {
            // Get start index of the specified pixel
            int i = ((y * Box.Width) + x) * _source.BytesPerPixel;

            if (_source.Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (_source.Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (_source.Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1} {2}x{3}", Box.X, Box.Y, Box.Width, Box.Height);
            //return Box.ToString();
        }
    }
}
