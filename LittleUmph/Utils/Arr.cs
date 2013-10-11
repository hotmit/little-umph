using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Array utilities.
    /// </summary>
    public class Arr
    {
        #region [ Contains ]
        /// <summary>
        /// Determine if the Array contains the specified item (ignore case).
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <returns>
        /// 	<c>true</c> if needle is in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(IList<string> haystack, string needle)
        {
            return Contains(haystack, needle, true);
        }

        /// <summary>
        /// Determine if the Array contains the specified item.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore case.</param>
        /// <returns>
        /// 	<c>true</c> if needle is in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(IList<string> haystack, string needle, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return haystack.Contains(needle);
            }

            foreach (string str in haystack)
            {
                if (string.Compare(str, needle, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region [ Implode/Trim/Chunk/ToString/ToObject ]
        /// <summary>
        /// Implodes the specified glue.
        /// </summary>
        /// <param name="glue">The glue.</param>
        /// <param name="pieces">The pieces.</param>
        /// <returns></returns>
        public static string Implode(string glue, params string[] pieces)
        {
            return Implode(glue, (IList<string>)pieces);
        }

        /// <summary>
        /// Concatinate the array using the specified separator.
        /// </summary>
        /// <param name="glue">The glue.</param>
        /// <param name="pieces">The pieces.</param>
        /// <returns></returns>
        public static string Implode(string glue, IList<string> pieces)
        {
            string str = "";
            foreach (string s in pieces)
            {
                str += s + glue;
            }

            if (str.Length == 0)
            {
                return "";
            }

            str = str.Substring(0, str.Length - glue.Length);
            return str;
        }

        /// <summary>
        /// Trim all the ELEMENT in the array. NOT the array itself.
        /// </summary>
        /// <param name="array"></param>
        public static List<string> Trim(IList<string> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                array[i] = array[i].Trim();
            }
            return (List<string>)array;
        }

        /// <summary>
        /// Convert an array of type T into an array of string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static List<string> ToString (IList array)
        {
            List<string> list = new List<string>(array.Count);
            foreach (var item in array)
            {
                list.Add(Convert.ToString(item));
            }
            return list;
        }

        /// <summary>
        /// Convert an array of type T into an array of objects
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static List<object> ToObject(IList array)
        {
            List<object> list = new List<object>(array.Count);
            foreach (var item in array)
            {
                list.Add((object)item);
            }
            return list;
        }

        /// <summary>
        /// Split array into multiple arrays. Example: 1,2,3,4,5 chunk size 2 => return[0] = [1,2], return[1] = [3,4], return [2] = [5].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static List<T[]> Chunk<T>(T[] array, int size)
        {
            List<T[]> finalList = new List<T[]>();
            List<T> curList = new List<T>();

            for (int i = 0, len = array.Length; i < len; i++)
            {
                curList.Add(array[i]);

                if (curList.Count == size || i + 1 == len)
                {
                    finalList.Add(curList.ToArray());
                    curList = new List<T>();
                }
            }

            return finalList;
        }
        #endregion

        #region [ Merge ]
        /// <summary>
        /// Join two array together to make one long one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr1">First array.</param>
        /// <param name="arr2">The remainding array(s).</param>
        /// <returns></returns>
        public static T[] Merge<T>(T[] arr1, params T[][] arr2)
        {
            List<T> list = new List<T>(arr1);
            foreach (T[] arr in arr2)
            {
                list.AddRange(arr);
            }
            return list.ToArray();
        }
        #endregion

        public static List<T> FromString<T>(string str,  T[] valueOnError, string separator = ",", int count = 0)
        {
            List<T> list = new List<T>();
            string[] arr = Str.Split(str, separator, count);
            foreach (var itm in arr)
            {
                list.Add(DType.Cast<T>(itm, default(T)));
            }
            return list;
        }
    }
}
