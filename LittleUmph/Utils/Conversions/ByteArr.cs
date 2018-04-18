using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Byte[] conversion.
    /// </summary>
    public class ByteArr
    {
        #region [ Bytes To Bin ]
        /// <summary>
        /// Convert bytes to binary string. Return emptied string on error.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToBin(byte[] bytes)
        {
            string hex = ByteArr.ToHex(bytes);
            return Hex.ToBin(hex);
        }
        #endregion

        #region [ Bytes To Dec ]
        /// <summary>
        /// Convert the hex value in the byte into an int.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static int ToInt(byte[] bytes, int valueOnError)
        {
            try
            {
                return (int)ByteArr.ToLong(bytes, valueOnError);
            }
            catch
            {
                return valueOnError;
            }
        }

        /// <summary>
        /// Convert the hex value in the byte into an long value.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="valueOnError">The value on error.</param>
        /// <returns></returns>
        public static long ToLong(byte[] bytes, long valueOnError)
        {
            try
            {
                string hex = ByteArr.ToHex(bytes);
                if (string.IsNullOrEmpty(hex))
                {
                    return valueOnError;
                }

                long result = Hex.ToLong(hex, valueOnError);
                return result;
            }
            catch
            {
                // This happend when the value of long is exceeded the Min/MaxValue
                return valueOnError;
            }
        }
        #endregion

        #region [ Bytes To Hex ]
        /// <summary>
        /// Convert bytes to hex string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static string ToHex(byte[] data, string byteSepartor = "")
        {
            if (data == null)
            {
                return string.Empty;
            }

            string hex = BitConverter.ToString(data).Replace("-", byteSepartor);
            return hex;
        }
        #endregion

        #region [ Bytes To String ]
        /// <summary>
        /// Bytes to ascii encoded string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>
        /// </returns>
        public static string ToString(byte[] bytes)
        {
            return ByteArr.ToString(bytes, Encoding.ASCII);
        }

        /// <summary>
        /// Bytes to encoded string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// </returns>
        public static string ToString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Bytes to utf8 encoded string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToUtf8(byte[] bytes)
        {
            return ByteArr.ToString(bytes, Encoding.UTF8);
        }
        #endregion

        #region [ GetConstructString ]
        /// <summary>
        /// Return byte in string format "new byte[]{ }". 
        /// You can use this to display an array of byte for debug or log purposes.
        /// You can also use this to initialize the byte array in your code.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string GetConstructString(byte[] bytes)
        {
            string result = "new byte[] { ";
            foreach (byte b in bytes)
            {
                result += ((int)b).ToString() + ", ";
            }
            result = result.TrimEnd(',', ' ');
            result += " };";
            return result;
        }
        #endregion

        #region [ IsEquals ]
        /// <summary>
        /// Determines whether the two byte array is equals
        /// </summary>
        /// <param name="arr1">The arr1.</param>
        /// <param name="arr2">The arr2.</param>
        /// <returns>
        ///   <c>true</c> if the specified arr1 is equals; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEquals(byte[] arr1, byte[] arr2)
        {
            if (arr1 == arr2)
            {
                return true;
            }
            if ((arr1 != null) && (arr2 != null))
            {
                if (arr1.Length != arr2.Length)
                {
                    return false;
                }

#if NET40_OR_GREATER
                IStructuralEquatable eqa1 = arr1;
                return eqa1.Equals(arr2, StructuralComparisons.StructuralEqualityComparer);
#else
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (arr1[i] != arr2[i])
                    {
                        return false;
                    }
                }
                return true;
#endif
            }
            return false;
        }
        #endregion

        #region [ Bytes To/From Base64 ]
        /// <summary>
        /// Convert byte array into a base64 string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Covnert from base64 encoded string into byte array.
        /// </summary>
        /// <param name="base64">The base64.</param>
        /// <returns></returns>
        public static byte[] FromBase64(string base64)
        {
            return Convert.FromBase64String(base64);
        }
        #endregion
    }
}
