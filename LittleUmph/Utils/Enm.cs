using System;
using System.Collections.Generic;
using System.Text;
#if NET35_OR_GREATER
using System.Linq;
#endif

namespace LittleUmph
{
    /// <summary>
    /// Enum helper
    /// </summary>
    public class Enm
    {
        #region [ Utils ]
        /// <summary>
        /// Go through each value of enum T and check 
        /// if the value contains within the flag in flagEnum.
        /// If if it in is in the flagEnum, pass it to the action delegate as a parameter.
        /// Ex: Each&lt;Fruit&gt;(Apple|Pear, f => Print("Fruit: " + f.ToString()));
        /// </summary>
        /// <typeparam name="T">Enum type.</typeparam>
        /// <param name="flagEnum">The flag enum.</param>
        /// <param name="action">The action.</param>
        public static void Each<T>(T flagEnum, Action<T> action)
        {
            EnumCheck<T>();

            foreach (var e in GetList<T>())
            {
                // skip "none" value
                if (Convert.ToUInt32(e) == 0)
                {
                    continue;
                }

                if (HasFlag<T>(flagEnum, e))
                {
                    action(e);
                }
            }
        }

        /// <summary>
        /// Get all the values of enum type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static List<T> GetList<T>()
        {
#if !NET20
            EnumCheck<T>();

            List<T> list = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            return list;
#else
            throw new NotSupportedException("Only support .Net 3.5 and higher");
#endif
        }
        #endregion

        #region [ Match ]
        /// <summary>
        /// Matches any.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">The e.</param>
        /// <param name="matches">The matches.</param>
        /// <returns></returns>
        public static bool MatchAny<T>(T e, params T[] matches)
        {
            foreach (var m in matches)
            {
                if (HasFlag<T>(e, m) || HasFlag<T>(m, e))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check see if the combination has the specified flag.
        /// Ex: HasFlag(Permission, Perm.Write);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="combinationFlag">The combination flag.</param>
        /// <param name="lookingForValue">The flag to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified combination flag has flag; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlag<T>(T combinationFlag, T lookingForValue)
        {
            EnumCheck<T>();

            if (combinationFlag.Equals(lookingForValue))
            {
                return true;
            }

            uint cb = Convert.ToUInt32(combinationFlag);
            uint lv = Convert.ToUInt32(lookingForValue);

            return (cb & lv) == lv;
        }
        #endregion

        #region [ Manipulation ]
        /// <summary>
        /// Add a value to the flag.
        /// Ex: Add(Permission, Execute)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The SRC.</param>
        /// <param name="newEnum">The new enum.</param>
        /// <returns></returns>
        public static T Add<T>(T src, T newEnum)
        {
            EnumCheck<T>();

            uint value = Convert.ToUInt32(src) | Convert.ToUInt32(newEnum);
            return FromInt<T>(value, src);
        }

        /// <summary>
        /// Remove a value from the flag.
        /// Ex: Remove(Permission, Write|Execute)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The SRC.</param>
        /// <param name="valueToRemove">The value to remove.</param>
        /// <returns></returns>
        public static T Remove<T>(T src, T valueToRemove)
        {
            EnumCheck<T>();

            uint value = Convert.ToUInt32(src) & ~Convert.ToUInt32(valueToRemove);
            return FromInt<T>(value, src);

        }
        #endregion

        #region [ Conversions ]
        /// <summary>
        /// Convert an int to the enum of specified type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T FromInt<T>(int value, T defaultValue)
        {
            return FromInt((uint)value, defaultValue);
        }


        /// <summary>
        /// Convert an int to the enum of specified type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T FromInt<T>(uint value, T defaultValue)
        {
            EnumCheck<T>();

            try
            {
                return (T)Enum.ToObject(typeof(T), value);
            }
            catch (Exception xpt)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Convert a string to the enum of specified type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T FromString<T>(string value, T defaultValue)
        {
            EnumCheck<T>();

            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception xpt)
            {
                return defaultValue;
            }
        }
        #endregion

        #region [ Helper ]
        private static void EnumCheck<T>()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
        }
        #endregion
    }
}
