using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Functions to validate and manipulate character.
    /// </summary>
    public class Chr
    {
        /// <summary>
        /// Is the char 0 to 9 inclusive
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns>
        /// 	<c>true</c> if c is ASCII digit; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsciiDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Is the char a-zA-Z
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsAsciiAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        /// <summary>
        /// Is the char a-zA-Z0-9
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsAsciiAlphaNumeric(char c)
        {
            return IsAsciiDigit(c) || IsAsciiAlpha(c);
        }

        /// <summary>
        /// Determines whether the specified a is equals to b.
        /// </summary>
        /// <param name="a">The first char.</param>
        /// <param name="b">The second char.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>
        ///   <c>true</c> if the specified a is equals; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEquals(char a, char b, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);
            }
            return a == b;
        }

        /// <summary>
        /// Determines whether the specified a is not equals to b.
        /// </summary>
        /// <param name="a">The first char.</param>
        /// <param name="b">The second char.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>
        ///   <c>true</c> if the specified a is equals; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEquals(char a, char b, bool ignoreCase = true)
        {
            return !IsEquals(a, b, ignoreCase);
        }
    }
}
