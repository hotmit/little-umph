using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Hex conversions.
    /// </summary>
    public class Hex
    {
        #region [ Hex To Bin ]
        /// <summary>
        /// Hex to binary string. Return emptied string on error.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <returns></returns>
        public static string ToBin(string hex)
        {
            return Hex.ToBin(hex, string.Empty);
        }

        /// <summary>
        /// Hex to binary string.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static string ToBin(string hex, string valueOnError)
        {
            const long ERROR = long.MinValue + 1;
            long dec = Hex.ToLong(hex, ERROR);
            if (dec == ERROR)
            {
                return valueOnError;
            }
            string bin = Dec.ToBin(dec);
            return bin;
        }
        #endregion

        #region [ Hex To Byte ]
        /// <summary>
        /// Convert hex string into to a array of bytes.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <returns>Return null on error.</returns>
        public static byte[] ToBytes(string hex)
        {
            return Hex.ToBytes(hex, 0);
        }

        /// <summary>
        /// Convert hex string into to a array of bytes. Padded with zero if it is not long enough.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="totalLength">The total length.</param>
        /// <returns></returns>
        public static byte[] ToBytes(string hex, int totalLength)
        {
            try
            {
                hex = CleanHex(hex);

                if (hex.Length % 2 != 0)
                {
                    hex = "0" + hex;
                }

                int byteLength = hex.Length / 2;
                if (totalLength > byteLength)
                {
                    hex = hex.PadLeft(totalLength * 2, '0');
                    byteLength = totalLength;
                }

                byte[] bytes = new byte[byteLength];
                for (int index = 0, i = 0; i < hex.Length; i += 2, index += 1)
                {
                    bytes[index] = (byte)(_hexLookupTable[hex[i] - '0'] << 4 |
                                      _hexLookupTable[hex[i + 1] - '0']);
                }
                return bytes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static readonly int[] _hexLookupTable = new int[] 
                                {   
                                    0x00, 0x01, 0x02, 0x03, 0x04, 
                                    0x05, 0x06, 0x07, 0x08, 0x09,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 
                                    0x00, 0x00, 0x0A, 0x0B, 0x0C, 
                                    0x0D, 0x0E, 0x0F 
                                };
        #endregion

        #region [ Hex To Dec ]
        /// <summary>
        /// Covert hex string into int.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static int ToInt(string hex, int valueOnError)
        {
            try
            {
                hex = CleanHex(hex);
                int value = Convert.ToInt32(hex, 16);
                return value;
            }
            catch
            {
                return valueOnError;
            }
        }

        /// <summary>
        /// Covert hex string into long.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static long ToLong(string hex, long valueOnError)
        {
            try
            {
                hex = CleanHex(hex);
                long value = Convert.ToInt64(hex, 16);
                return value;
            }
            catch
            {
                return valueOnError;
            }
        }
        #endregion

        #region [ Hex To String ]
        /// <summary>
        /// Convert hex to bytes and bytes to ascii encoded string.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <returns>
        /// Return emptied string on error.
        /// </returns>
        public static string ToString(string hex)
        {
            return Hex.ToString(hex, string.Empty);
        }

        /// <summary>
        /// Convert hex to bytes and bytes to ascii encoded string.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static string ToString(string hex, string valueOnError)
        {
            return Hex.ToString(hex, valueOnError, Encoding.ASCII);
        }

        /// <summary>
        /// Convert hex to bytes and bytes to encoded string.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// </returns>
        public static string ToString(string hex, string valueOnError, Encoding encoding)
        {
            // This function also clean the hex value
            byte[] b = Hex.ToBytes(hex);
            if (b != null)
            {
                string str = encoding.GetString(b);
                return str;
            }
            else
            {
                return valueOnError;
            }
        }

        /// <summary>
        /// Convert hex to bytes and bytes to utf8 encoded string.
        /// </summary>
        /// <param name="hex">The hex (Acceptable format FEC4A3B2, FE C4 A3 B2 or FE-C4-A4-B2).</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static string ToUtf8(string hex, string valueOnError)
        {
            return Hex.ToString(hex, valueOnError, Encoding.UTF8);
        }
        #endregion


        #region [ Helper ]
        /// <summary>
        /// Cleans the hex string replace space and dash.
        /// </summary>
        /// <param name="hex">The hex.</param>
        /// <returns></returns>
        private static string CleanHex(string hex)
        {
            hex = hex.Replace("-", "").ToUpper();
            hex = hex.Replace(" ", "").Trim();

            return hex;
        }
        #endregion
    }
}
