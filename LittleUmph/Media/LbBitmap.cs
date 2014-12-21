using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using LittleUmph;

namespace LittleUmph.Media
{
    /// <summary>
    /// Wrapper for LockBits, the faster way to get pixels from an image.
    /// </summary>
    public class LbBitmap
    {
        #region [ Private Variables ]
        int _widthInBytes = 0;
        int _heightInBytes = 0;

        LbBox _wholeImageBox;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the total pixels.
        /// </summary>
        /// <value>
        /// The total pixels.
        /// </value>
        public int TotalPixels { get; private set; }

        /// <summary>
        /// Gets the bytes per pixel.
        /// </summary>
        /// <value>
        /// The bytes per pixel.
        /// </value>
        public int BytesPerPixel { get; private set; }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <value>
        /// The bitmap.
        /// </value>
        public Bitmap Bitmap { get; private set; }

        /// <summary>
        /// Gets the depth the image 8bit, 24bit or 32bit.
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the image lock mode.
        /// </summary>
        /// <value>
        /// The lock mode.
        /// </value>
        public ImageLockMode LockMode { get; private set; }
        #endregion

        public LbBitmap(string imagePath, ImageLockMode lockMode = ImageLockMode.ReadOnly) : this((Bitmap)Img.FromFile(imagePath), lockMode)
        {
        }

        public LbBitmap(Bitmap image, ImageLockMode lockMode = ImageLockMode.ReadOnly)
        {
            Bitmap = image;
            LockMode = lockMode;

            Depth = Bitmap.GetPixelFormatSize(Bitmap.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (Depth != 8 && Depth != 24 && Depth != 32)
            {
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            }

            Width = Bitmap.Width;
            Height = Bitmap.Height;

            BytesPerPixel = Depth / 8;
            TotalPixels = Width * Height;

            _widthInBytes = BytesPerPixel * Width;
            _heightInBytes = BytesPerPixel * Height;


            _wholeImageBox = new LbBox(this, 0, 0, Width, Height);
        }

        /// <summary>
        /// Divides the image into small regions to make easier to process
        /// and comsume less memory.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public List<LbBox> GetBoxes(int width, int height)
        {
            List<LbBox> list = new List<LbBox>();

            int w, h;
            for (int y = 0; y < Height; y += height)
            {
                h = y + height >= Height ? Height - y : height;
                for (int x = 0; x < Width; x += width)
                {
                    w = x + width >= Width ? Width - x : width;
                    LbBox box = new LbBox(this, x, y, w, h);
                    list.Add(box);
                }
            }

            return list;
        }

        /// <summary>
        /// Divides the image into the number of boxes specified in
        /// the parameter.
        /// </summary>
        /// <param name="totalBoxes">The total boxes.</param>
        /// <returns></returns>
        public List<LbBox> GetBoxes(int totalBoxes)
        {
            if (totalBoxes == 1)
            {
                return new List<LbBox>() { _wholeImageBox };
            }

            // x = Sqrt(WH/total)
            double width = Math.Sqrt(TotalPixels / (double)totalBoxes);
            int w = Convert.ToInt32(Math.Ceiling(width));

            return GetBoxes(w, w);
        }

        /// <summary>
        /// Locks the entire image.
        /// </summary>
        public void Lock()
        {
            _wholeImageBox.Lock();
        }

        /// <summary>
        /// Unlock the entire image.
        /// </summary>
        public void UnLock()
        {
            _wholeImageBox.UnLock();
        }

        /// <summary>
        /// Gets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            return _wholeImageBox.GetPixel(x, y);
        }

        public void Save()
        {
            _wholeImageBox.Save();
        }

        /// <summary>
        /// Sets the pixel.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public void SetPixel(Color color, int x, int y)
        {
            _wholeImageBox.SetPixel(color, x, y);
        }

        /// <summary>
        /// Gets the color pallette.
        /// </summary>
        /// <param name="rounding">The round the color value {ARGB} to the nearest interval.</param>
        /// <param name="sampleSize">Size of the sample (ie how many pixels to analyze).</param>
        /// <returns></returns>
        public Dictionary<Color, int> GetPallette(int rounding = 50, int sampleSize = 10000)
        {
            Dictionary<Color, int> frequency = new Dictionary<Color, int>();
            int increment = Convert.ToInt32(Math.Ceiling(Math.Sqrt((Width * Height) / (double)sampleSize)));

            try
            {
                Lock();

                Color prev = GetPixel(0, 0);
                for (int y = 0; y < Height; y += increment)
                {
                    for (int x = 0; x < Width; x += increment)
                    {
                        Color pixel = GetPixel(x, y);

                        // Round and group similar colors
                        pixel = Clr.Round(pixel, rounding);

                        if (frequency.ContainsKey(pixel))
                        {
                            frequency[pixel]++;
                        }
                        else
                        {
                            frequency.Add(pixel, 1);
                        }
                    }
                }
            }
            finally
            {
                UnLock();
            }
                        
#if NET35_OR_GREATER
            Dictionary<Color, int> result = frequency.OrderByDescending(d => d.Value)
                                                        .Take(40)
                                                        .ToDictionary(d => d.Key, d => d.Value);
            return result;
#else
            throw new NotSupportedException("Only for framework with LINQ.");
#endif
        }

        /// <summary>
        /// Gets the color of the dominant.
        /// </summary>
        /// <param name="rounding">The rounding.</param>
        /// <param name="sampleSize">Size of the sample.</param>
        /// <returns></returns>
        public Color GetDominantColor(int rounding = 50, int sampleSize = 10000)
        {
            Dictionary<Color, int> pallette = GetPallette(rounding, sampleSize);

            if (pallette.Count > 0)
            {
                foreach (KeyValuePair<Color, int> first in pallette)
                {
                    return first.Key;
                }
            }
            return Color.Empty;
        }
    }
}