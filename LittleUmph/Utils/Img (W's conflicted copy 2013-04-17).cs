using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using System.Data;
using System.Data.Common;

namespace LittleUmph
{
    public class Img
    {
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

        #region [ Image Handling ]
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
            Bitmap bmp = new Bitmap(size.Width, size.Height);
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
        #endregion

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
        /// <returns></returns>
        public static Bitmap CloneImage(Bitmap bitmap)
        {
            // this somehow release the original bitmap lock on resources
            // this must of copy the bitmap to memory
            Bitmap clone = new Bitmap(bitmap);
            bitmap.Dispose();
            return clone;
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
    }
}
