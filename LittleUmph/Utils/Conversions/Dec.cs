using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Decimal (base10) conversion
    /// </summary>
    public class Dec
    {
        #region [ Dec To Bin ]
        /// <summary>
        /// Int to binary string.
        /// </summary>
        /// <param name="dec">The dec.</param>
        /// <returns></returns>
        public static string ToBin(int dec)
        {
            return Convert.ToString(dec, 2);
        }

        /// <summary>
        /// Long to binary string.
        /// </summary>
        /// <param name="dec">The dec.</param>
        /// <returns></returns>
        public static string ToBin(long dec)
        {
            return Convert.ToString(dec, 2);
        }
        #endregion

        #region [ Dec To Hex ]
        /// <summary>
        /// Convert int into hex.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToHex(int value)
        {
            return value.ToString("X");
        }

        /// <summary>
        /// Convert int into hex. Pad zero to the left of the hex satisfy 
        /// the total length specified (number of chars).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static string ToHex(int value, int totalLength)
        {
            return Dec.ToHex((long)value, totalLength);
        }

        /// <summary>
        /// Convert long into hex.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToHex(long value)
        {
            return value.ToString("X");
        }

        /// <summary>
        /// Convert long into hex. Pad zero to the left of the hex satisfy 
        /// the total length specified (number of chars).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static string ToHex(long value, int totalLength)
        {
            return value.ToString("X").PadLeft(totalLength, '0');
        }
        #endregion

        #region [ Dec To Bytes ]
        /// <summary>
        /// Convert an int into an array of bytes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static byte[] ToBytes(int value)
        {
            byte[] bytes = Dec.ToBytes(value, 0);
            return bytes;
        }

        /// <summary>
        /// Convert an int into an array of bytes
        /// padded zero's to the left to fill the byte count.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static byte[] ToBytes(int value, int totalLength)
        {
            return Dec.ToBytes((long)value, totalLength);
        }

        /// <summary>
        /// Convert a long value into an array of bytes.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static byte[] ToBytes(long value)
        {
            byte[] bytes = Hex.ToBytes(Dec.ToHex(value));
            return bytes;
        }

        /// <summary>
        /// Convert an long into an array of bytes
        /// padded zero's to the left to fill the byte count.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static byte[] ToBytes(long value, int totalLength)
        {
            string hex = Dec.ToHex(value);
            byte[] bytes = Hex.ToBytes(hex, totalLength);
            return bytes;
        }
        #endregion
    }
}
