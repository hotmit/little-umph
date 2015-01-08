using System;
using System.Collections.Generic;
using System.Text;

using LittleUmph;

namespace LittleUmph
{
    /// <summary>
    /// Binary Conversions
    /// </summary>
    public class Bin
    {
        #region [ Bin To Bytes ]
        /// <summary>
        /// Binary string to byte[]. Return null on error.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <returns></returns>
        public static byte[] ToBytes(string bin)
        {
            return Bin.ToBytes(bin, 0);
        }

        /// <summary>
        /// Binary string to byte[]. Return null on error.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static byte[] ToBytes(string bin, int totalLength)
        {
            string hex = Bin.ToHex(bin, "Error");
            if (hex == "Error")
            {
                return null;
            }
            return Hex.ToBytes(hex, totalLength);
        }
        #endregion

        #region [ Bin To Dec ]
        /// <summary>
        /// Convert binary string to int.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static int ToInt(string bin, int valueOnError)
        {
            try
            {
                return Convert.ToInt32(bin, 2);
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Bin.ToLong()", xpt.Message);
                return valueOnError;
            }
        }

        /// <summary>
        /// Convert binary string to long.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static long ToLong(string bin, long valueOnError)
        {
            try
            {
                return Convert.ToInt64(bin, 2);
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Bin.ToLong()", xpt.Message);
                return valueOnError;
            }
        }
        #endregion

        #region [ Bin To Hex ]
        /// <summary>
        /// Binary to hex string. Return emptied string on error.
        /// </summary>
        /// <param name="bin">The binary string.</param>
        /// <returns>Emptied string on error.</returns>
        public static string ToHex(string bin)
        {
            return Bin.ToHex(bin, string.Empty);
        }

        /// <summary>
        /// Binary to hex string.
        /// </summary>
        /// <param name="bin">The binary string.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static string ToHex(string bin, string valueOnError)
        {
            try
            {
                string hex = Dec.ToHex(Convert.ToInt64(bin, 2));
                return hex;
            }
            catch (Exception xpt)
            {
                return valueOnError;
            }
        }

        /// <summary>
        /// Binary to hex string. 
        /// Pad zero to the left to fill the total length.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static string ToHex(string bin, int totalLength)
        {
            return Bin.ToHex(bin).PadLeft(totalLength, '0');
        }
        #endregion
    }
}
