using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Data;
using System.Data.Common;

using LittleUmph.Media;

namespace LittleUmph
{
    /// <summary>
    /// Image helper class.
    /// </summary>
    public class Img
    {
        #region [ Image Convertions ]
        /// <summary>
        /// Images to bytes.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static byte[] ToBytes(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Images from bytes.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Bitmap FromBytes(byte[] image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream(image))
            {
                Bitmap bitmap = new Bitmap(ms);
                return bitmap;
            }
        }

        /// <summary>
        /// Get the image from the DB SQl DataReader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="returnOnError">The return image on error.</param>
        /// <returns></returns>
        public static Image FromDataReader(IDataReader dr, string columnName, Image returnOnError)
        {
            try
            {
                int colIndex = dr.GetOrdinal(columnName);
                if (dr.IsDBNull(colIndex))
                {
                    return returnOnError;
                }

                byte[] data = (byte[])dr[columnName];
                Image img = FromBytes(data);
                return img;
            }
            catch (Exception)
            {
                return returnOnError;
            }
        }

        /// <summary>
        /// Create image from the a file.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns></returns>
        public static Image FromFile(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return null;
            }

            try
            {
                using (Image img = Bitmap.FromFile(imagePath))
                {
                    return Img.CloneImage((Bitmap)img);
                }
            }
            catch (Exception xpt)
            {
                return null;
            }
        }
        #endregion

        #region [ Image Manipulation ]
        /// <summary>
        /// Maintain the ratio and resize the image to fit into the frame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        public static Image BestFit(Image image, Size frame)
        {
            Size newSize = GetMaxSize(frame, image.Size);
            return Resize(image, newSize);
        }

        /// <summary>
        /// Maintain the ratio and resize the image to fit into the frame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="frame">The frame.</param>
        /// <returns></returns>
        public static Bitmap BestFit(Bitmap image, Size frame)
        {
            Size newSize = GetMaxSize(frame, image.Size);
            return (Bitmap)Resize((Image)image, newSize);
        }

        /// <summary>
        /// Resizes the image to the specified size.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static Image Resize(Image image, Size size)
        {
            using (Bitmap bmp = new Bitmap(size.Width, size.Height))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(
                        image,
                        new Rectangle(0, 0, size.Width, size.Height),
                        new Rectangle(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                }
                return CloneImage(bmp);
            }
        }

        /// <summary>
        /// Rotates the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        public static Image Rotate(Image image, RotateFlipType angle)
        {
            image.RotateFlip(angle);
            return image;
        }

        /// <summary>
        /// Do this to release the lock otherwise you can not save this bitmap to file
        /// you'll get gdi+ error and also prevent folder from being move due to file locked.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="disposeSrc">if set to <c>true</c> [dispose SRC].</param>
        /// <returns></returns>
        public static Bitmap CloneImage(Bitmap bitmap, bool disposeSrc = false)
        {
            // this somehow release the original bitmap lock on resources
            // this must of copy the bitmap to memory
            Bitmap clone = new Bitmap(bitmap);
            if (disposeSrc)
            {
                bitmap.Dispose();
            }
            return clone;
        }        
        #endregion

        #region [ Image Size ]
        /// <summary>
        /// Get the maxium size of the image where 
        /// it can fit inside the frame and maintain the aspect ratio.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="imageSize">The image.</param>
        /// <returns></returns>
        public static Size GetMaxSize(Size frame, Size imageSize)
        {
            bool widthIsBigger = imageSize.Width - frame.Width > imageSize.Height - frame.Height;
            double ratio = imageSize.Width / (imageSize.Height * 1.0);
            int width, height;

            if (widthIsBigger)
            {
                width = frame.Width;
                height = Convert.ToInt32(width / ratio);
            }
            else
            {
                height = frame.Height;
                width = Convert.ToInt32(height * ratio);
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Gets the size of the specified image (return size(0,0) on error).
        /// </summary>
        /// <param name="imgPath">The img path.</param>
        /// <returns></returns>
        public static Size GetSize(string imgPath)
        {
            try
            {
                if (File.Exists(imgPath))
                {
                    using (var b = new Bitmap(imgPath))
                    {
                        return b.Size;
                    }
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Img.GetSize(.)", xpt);
            }
            return new Size(0, 0);
        }
        #endregion

        #region [ Color Analysis ]
        /// <summary>
        /// Gets the dominant color in the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Color GetDominantColor(Bitmap image)
        {
            LbBitmap lbBitmap = new LbBitmap(image);
            Color color = lbBitmap.GetDominantColor();
            return color;
        }

        /// <summary>
        /// Gets the dominant color in the image.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns></returns>
        public static Color GetDominantColor(string imagePath)
        {
            using (var bm = (Bitmap)Img.FromFile(imagePath))
            {
                var result = GetDominantColor(bm);
                return result;
            }
        }


        /// <summary>
        /// Determines whether the image's dominant color is close to the specified color.
        /// Ex: Test if the image is mostly white => IsDominantColor(image, Color.White)
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="compareColor">Color of the compare.</param>
        /// <param name="tolerant">The tolerant.</param>
        /// <returns></returns>
        public static bool IsDominantColor(Bitmap image, Color compareColor, int tolerant = 15)
        {
            Color dominantColor = GetDominantColor(image);
            double diff = Math.Abs(Clr.CIE76Difference(dominantColor, compareColor));

            if (diff <= tolerant)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the image's dominant color is close to the specified color.
        /// Ex: Test if the image is mostly white => IsDominantColor(image, Color.White)
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <param name="compareColor">Color of the compare.</param>
        /// <param name="tolerant">The tolerant.</param>
        /// <returns>
        ///   <c>true</c> if [is dominant color] [the specified image path]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDominantColor(string imagePath, Color compareColor, int tolerant = 15)
        {
            using (var bm = (Bitmap)Img.FromFile(imagePath))
            {
                bool result = IsDominantColor(bm, compareColor, tolerant);
                return result;
            }
        }
        #endregion
    }
}
