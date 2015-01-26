using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Windows.Forms;

namespace LittleUmph
{
    /// <summary>
    /// String manipulation and validation methods.
    /// </summary>
    public class Str
    {
        #region [ Number Detection ]
        /// <summary>
        /// Check if the string is a valid integer
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>
        /// 	<c>true</c> if the specified num is integer; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInteger(string num)
        {
            num = Trim(num);

            if (IsEmpty(num))
            {
                return false;
            }

            if (num[0] == '-' || num[0] == '+')
            {
                num = num.Substring(1);
            }

            if (num.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < num.Length; i++)
            {
                if (!Chr.IsAsciiDigit(num[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check to see the string is a valid double format (Valid: -103.3, +23.8, 3.23, 8))
        /// </summary>
        /// <param name="num">The number.</param>
        /// <returns>
        /// 	<c>true</c> if the specified num is double; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDouble(string num)
        {
            num = Str.Trim(num);

            if (IsEmpty(num))
            {
                return false;
            }

            if (num[0] == '-' || num[0] == '+')
            {
                num = num.Substring(1);
            }

            if (num.Length == 0)
            {
                return false;
            }

            bool hasOneDot = false;
            for (int i = 0; i < num.Length; i++)
            {
                if (!Chr.IsAsciiDigit(num[i]) && num[i] != '.')
                {
                    return false;
                }

                if (num[i] == '.')
                {
                    if (hasOneDot)
                    {
                        return false;
                    }
                    hasOneDot = true;
                }
            }
            return true;
        }
        #endregion

        #region [ WordWrap ]
        /// <summary>
        /// Wraps the passed string up to 
        /// the next whitespace on or after the total charCount has been reached
        /// for that line.  Uses the environment new line for the break text.
        /// </summary>
        /// <param name="input">The string to wrap.</param>
        /// <param name="charCount">The number of characters per line.</param>
        /// <returns>A string.</returns>
        public static string WordWrap(string input, int charCount)
        {
            return WordWrap(input, charCount, false, Environment.NewLine);
        }

        /// <summary>
        /// Break one long string into multiple lines.
        /// </summary>
        /// <param name="input">The string to wrap.</param>
        /// <param name="charCount">The number of characters per line.</param>
        /// <param name="cutOff">If true, will break in the middle of a word.</param>
        /// <returns>A string.</returns>
        public static string WordWrap(string input, int charCount, bool cutOff)
        {
            return WordWrap(input, charCount, cutOff, Environment.NewLine);
        }

        /// <summary>
        /// Break one long string into multiple lines.
        /// </summary>
        /// <param name="input">The string to wrap.</param>
        /// <param name="charCount">The number of characters per line.</param>
        /// <param name="cutOff">If true, will break in the middle of a word.</param>
        /// <param name="breakText">The line break text to use (usually "\n").</param>
        /// <returns>A string.</returns>
        public static string WordWrap(string input, int charCount, bool cutOff, string breakText)
        {
            StringBuilder sb = new StringBuilder(input.Length + 100);
            int counter = 0;

            if (cutOff)
            {
                while (counter < input.Length)
                {
                    if (input.Length > counter + charCount)
                    {
                        sb.Append(input.Substring(counter, charCount));
                        sb.Append(breakText);
                    }
                    else
                    {
                        sb.Append(input.Substring(counter));
                    }
                    counter += charCount;
                }
            }
            else
            {
                string[] strings = input.Split(' ');
                for (int i = 0; i < strings.Length; i++)
                {
                    counter += strings[i].Length + 1; // the added one is to represent the inclusion of the space.
                    if (i != 0 && counter > charCount)
                    {
                        sb.Append(breakText);
                        counter = 0;
                    }

                    sb.Append(strings[i] + ' ');
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Wrap the text and return the array of lines
        /// </summary>
        /// <param name="input">The string to wrap</param>
        /// <param name="charCount">The number of characters per line</param>
        /// <returns></returns>
        public static List<string> WordWrapLine(string input, int charCount)
        {
            string wrapped = input.Replace("\r\n", "$~Break~$");
            wrapped = wrapped.Replace("\n", "$~Break~$");

            string[] paragraph = wrapped.Split(new string[] { "$~Break~$" }, StringSplitOptions.None);

            List<string> lines = new List<string>();
            foreach (string straighLine in paragraph)
            {
                string[] oneParagraphLines = WordWrap(straighLine, charCount, false, "$~Break~$").Split(new string[] { "$~Break~$" }, StringSplitOptions.RemoveEmptyEntries);
                lines.AddRange(oneParagraphLines);
            }

            return (List<string>)Arr.Trim(lines);
        }
        #endregion

        #region [ IsEmpty ]
        /// <summary>
        /// Determine if the string is empty (null or filled with space)
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsEmpty(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return true;
            }
            return txt.Trim().Length == 0;
        }

        /// <summary>
        /// Determines whether the string has something other than space.
        /// </summary>
        /// <param name="txt">The TXT.</param>
        /// <returns>
        /// 	<c>true</c> if the string is not empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEmpty(string txt)
        {
            return !IsEmpty(txt);
        }

        /// <summary>
        /// Determine if the textbox is emptied (null or filled with space)
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsEmpty(TextBox txt)
        {
            if (txt == null)
            {
                return true;
            }
            return IsEmpty(txt.Text);
        }

        /// <summary>
        /// Determines whether the textbox has non space characters.
        /// </summary>
        /// <param name="txt">The TXT.</param>
        /// <returns>
        /// 	<c>true</c> if has some non space chrarcters; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEmpty(TextBox txt)
        {
            return !IsEmpty(txt);
        }

        /// <summary>
        /// Determine if the control's text field is emptied (control is null or filled with space)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsEmpty(Control c)
        {
            if (c == null)
            {
                return true;
            }
            return IsEmpty(c.Text);
        }

        /// <summary>
        /// Determines whether the text on the control contains non space characters.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>
        /// 	<c>true</c> if the text on the control contains non space characters; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEmpty(Control c)
        {
            return !IsEmpty(c);
        }
        #endregion

        #region [ Case Manipulation ]
        /// <summary>
        /// Breaks the camel case string ("firstName" into "First Name").
        /// </summary>
        /// <param name="camelString">The camel string.</param>
        /// <returns></returns>
        public static string BreakCamelCase(string camelString)
        {
            if (camelString == null)
            {
                return null;
            }

            camelString = Trim(camelString);
            if (IsEmpty(camelString))
            {
                return "";
            }

            camelString = camelString.Replace("_", " ");

            // IBMWorld => IBM World
            camelString = Regex.Replace(camelString, "([A-Z])([a-z])", " $1$2");

            // MyHBOChannel => My HBOChannel
            camelString = Regex.Replace(camelString, "([A-Z0-9]+)", " $1");  ///TODO: need to confirm this works

            camelString = camelString.Replace(" +", " ");

            if (camelString.Length > 0)
            {
                camelString = char.ToUpper(camelString[0]) + camelString.Substring(1);
            }

            camelString = Regex.Replace(camelString, @" +", " ");
            return camelString.Trim();
        }

        /// <summary>
        /// Convert the string into proper title case. (ex: "one fine day" => "One Fine Day");
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ToTitleCase(string str)
        {
            if (str == null)
            {
                return null;
            }

            str = Trim(str);
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }
        #endregion

        #region [ Is Equal ]
        /// <summary>
        /// Case insensitive string comparision.
        /// </summary>
        /// <param name="a">First string.</param>
        /// <param name="b">Second string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified a is equal to b; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqual(string a, string b)
        {
            return IsEqual(a, b, true);
        }

        /// <summary>
        /// Compare the two strings, null value accepted.
        /// </summary>
        /// <param name="a">The first string</param>
        /// <param name="b">The second string.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore case when compare.</param>
        /// <returns>
        /// 	<c>true</c> if the a equal to b; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqual(string a, string b, bool ignoreCase)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (!ignoreCase)
            {
                return a.Equals(b);
            }
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether any of the specified needle equal to the haystack. (ignore case)
        /// </summary>
        /// <param name="haysack">The haysack.</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        ///   <c>true</c> if [is equals any] [the specified haysack]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqualsAny(string haysack, params string[] needles)
        {
            return IsEqualsAny(haysack, true, needles);
        }

        /// <summary>
        /// Determines whether any of the specified needle equal to the haystack.
        /// </summary>
        /// <param name="haysack">The haysack.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        ///   <c>true</c> if [is equals any] [the specified haysack]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqualsAny(string haysack, bool ignoreCase, params string[] needles)
        {
            foreach (var n in needles)
            {
                if (IsEqual(haysack, n, ignoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Case insensitive string comparision
        /// </summary>
        /// <param name="a">First string.</param>
        /// <param name="b">Second string.</param>
        /// <returns>
        /// 	<c>true</c> if the specified a is equal to b; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEqual(string a, string b)
        {
            return IsNotEqual(a, b, true);
        }

        /// <summary>
        /// Determines whether the two string is not equal to each other.
        /// </summary>
        /// <param name="a">First string.</param>
        /// <param name="b">Second string.</param>
        /// <param name="ignoreCase">if set to <c>true</c> to ignore case when compare.</param>
        /// <returns>
        /// 	<c>true</c> if is not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotEqual(string a, string b, bool ignoreCase)
        {
            return !IsEqual(a, b, ignoreCase);
        }
        #endregion

        #region [ Trim ]
        /// <summary>
        /// Trim the string, if "s" is null then return String.Empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Trim(string s)
        {
            return Trim(s, string.Empty);
        }

        /// <summary>
        /// Trim the string, if "s" is null then return valueOnError
        /// </summary>
        /// <param name="s"></param>
        /// <param name="valueOnError"></param>
        /// <returns></returns>
        public static string Trim(string s, string valueOnError)
        {
            if (s == null)
            {
                return valueOnError;
            }
            return s.Trim();
        }

        /// <summary>
        /// Trim the string before compare
        /// </summary>
        /// <param name="a">First string.</param>
        /// <param name="b">Second string.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore case.</param>
        /// <returns></returns>
        public static bool TrimEqual(string a, string b, bool ignoreCase)
        {
            a = Trim(a, null);
            b = Trim(b, null);

            return IsEqual(a, b, ignoreCase);
        }

        /// <summary>
        /// Trim the string before compare (case insensitive)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool TrimEqual(string a, string b)
        {
            return TrimEqual(a, b, true);
        }

        /// <summary>
        /// Trim (space, tab, newline, carrage return, null, vertical tab)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimAggressive(string str)
        {
            //                              Space,       Tab,       NewLine,     CR,          Null,     VerTab
            return str.Trim(new char[] { (char)0x20, (char)0x09, (char)0x0A, (char)0x0D, (char)0x00, (char)0x0B });
        }

        /// <summary>
        /// Remove puntuations from start and end of string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string TrimPuntuations(string str)
        {
            if (str == null)
            {
                return string.Empty;
            }

            int i = 0;
            for (i = 0; i < str.Length; i++)
            {
                if (!Char.IsPunctuation(str[i]))
                {
                    break;
                }
            }
            str = str.Substring(i);

            if (str.Length == 0)
            {
                return string.Empty;
            }

            for (i = str.Length - 1; i >= 0; i--)
            {
                if (!Char.IsPunctuation(str[i]))
                {
                    break;
                }
            }
            str = str.Substring(0, i + 1);

            return str;
        }
        #endregion

        #region [ Contains ]
        /// <summary>
        /// Determines whether the haystack contains the needle (ignore case).
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <returns>
        /// 	<c>true</c> if found the needle in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string haystack, string needle)
        {
            return Contains(haystack, needle, true);
        }

        /// <summary>
        /// Check to see haystack contains the needle.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <param name="ignoreCase">if set to <c>true</c> to ignore the case.</param>
        /// <returns>
        /// 	<c>true</c> if found the needle in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string haystack, string needle, bool ignoreCase)
        {
            if (haystack == null || needle == null)
            {
                return false;
            }

            if (ignoreCase)
            {
                return haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            return haystack.Contains(needle);
        }

        /// <summary>
        /// Check to see the haystack contains all the needles (ignore case)
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        /// 	<c>true</c> if found the all the needles in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAll(string haystack, params string[] needles)
        {
            return ContainsAll(haystack, true, needles);
        }

        /// <summary>
        /// Check to see the haystack contains all the needles .
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore case.</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        /// 	<c>true</c> if found the all the needles in the haystack; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAll(string haystack, bool ignoreCase, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (!Contains(haystack, needle, ignoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check to see the haystack contains ANY of the needles (ignore case).
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        /// 	<c>true</c> if the specified haystack contains any; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(string haystack, params string[] needles)
        {
            return ContainsAny(haystack, true, needles);
        }

        /// <summary>
        /// Check to see the haystack contains ANY of the needles.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore case.</param>
        /// <param name="needles">The needles.</param>
        /// <returns>
        /// 	<c>true</c> if the specified haystack contains any; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(string haystack, bool ignoreCase, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (Contains(haystack, needle, ignoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region [ Reverse and Concat ]
        /// <summary>
        /// Reverses the string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string Reverse(string str)
        {
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// Concats the list of strings into on long string separated by the specified separator.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <param name="strs">The strings.</param>
        /// <returns></returns>
        public static string Concat(string separator, params string[] strs)
        {
            return Arr.Implode(separator, strs);
        }
        #endregion

        #region [ CutLeft/CutRight/SubString ]
        /// <summary>
        /// If the string is longer than the specified length 
        /// then return the left portion of the input.  If the input is 
        /// less than the specified length then the input is return as is.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="length">The length.</param>
        /// <returns>Return emptied string if the input is null.</returns>
        public static string CutLeft(string input, int length)
        {
            if (input == null)
            {
                return "";
            }

            if (input.Length <= length)
            {
                return input;
            }

            return input.Substring(0, length);
        }

        /// <summary>
        /// If the string is longer than the specified length 
        /// then return the right portion of the input.  If the input is 
        /// less than the specified length then the input is return as is.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="length">The length.</param>
        /// <returns>Return emptied string if the input is null.</returns>
        public static string CutRight(string input, int length)
        {
            if (input == null)
            {
                return "";
            }

            if (input.Length <= length)
            {
                return input;
            }

            int offset = input.Length - length;
            return input.Substring(offset);
        }

        /// <summary>
        /// Extended version of String.SubString().
        /// This allow negative index (which is taking the substring from the end).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <example>Str.SubString("Hello", -3) =&gt; return "llo"</example>
        public static string SubString(string input, int index)
        {
            if (index >= input.Length)
            {
                return input;
            }

            if (index > 0)
            {
                return input.Substring(index);
            }

            return CutRight(input, -index);
        }
        #endregion

        #region [ IntVal ]
        /// <summary>
        /// Evaluate and extract the integer from the string (return 0 on error)
        /// </summary>
        /// <param name="txt">The string.</param>
        /// <returns></returns>
        public static int IntVal(string txt)
        {
            return IntVal(txt, 0);
        }

        /// <summary>
        /// Try to get a number from a string, if invalid return defaultValue.
        /// </summary>
        /// <param name="txt">The string.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int IntVal(string txt, int defaultValue)
        {
            if (txt.Trim().Length == 0)
            {
                return defaultValue;
            }

            try
            {
                if (!Str.IsInteger(txt))
                {
                    return defaultValue;
                }

                int i;
                if (int.TryParse(txt, out i))
                {
                    return i;
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Convert boolean into bit representation
        /// </summary>
        /// <param name="b">Boolean value</param>
        /// <returns></returns>
        public static int IntVal(bool b)
        {
            return b ? 1 : 0;
        }

        /// <summary>
        /// Evaluate and extract the integer from the Control.Text
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public static int IntVal(Control control)
        {
            return IntVal(control.Text, 0);
        }

        /// <summary>
        /// Try to get a number from the Control.Text, if invalid return defaultValue.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int IntVal(Control control, int defaultValue)
        {
            return IntVal(control.Text, defaultValue);
        }
        #endregion

        #region [ DoubleVal ]
        /// <summary>
        /// Extract double value from a string (accept currency formatting).
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <returns>Return "0" on error.</returns>
        public static double DoubleVal(string txt)
        {
            return DoubleVal(txt, 0);
        }

        /// <summary>
        /// Extract double value from the string (accept currency formatting)
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <param name="defaultValue">The default value to return on error.</param>
        /// <returns></returns>
        public static double DoubleVal(string txt, double defaultValue)
        {
            double result;
            if (double.TryParse(txt, NumberStyles.Currency, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Convert boolean into bit representation
        /// </summary>
        /// <param name="b">Boolean value</param>
        /// <returns></returns>
        public static double DoubleVal(bool b)
        {
            return b ? 1 : 0;
        }

        /// <summary>
        /// Extract double value from the Control.Text (accept currency formatting).
        /// </summary>
        /// <param name="control">The text.</param>
        /// <returns>Return "0" on error.</returns>
        public static double DoubleVal(Control control)
        {
            return DoubleVal(control.Text, 0);
        }

        /// <summary>
        /// Extract double value from the Control.Text (accept currency formatting)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="defaultValue">The default value to return on error.</param>
        /// <returns></returns>
        public static double DoubleVal(Control control, double defaultValue)
        {
            return DoubleVal(control.Text, defaultValue);
        }
        #endregion

        #region [ SubCount ]
        /// <summary>
        /// Count occurrences of strings.
        /// </summary>
        public static int SubCount(string str, string subStr)
        {
            int count = (str.Length - str.Replace(subStr, "").Length) / subStr.Length;
            return count;
        }

        /// <summary>
        /// Count occurrences of strings.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="subStr">The sub STR.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        public static int SubCount(string str, string subStr, int startIndex)
        {
            if (startIndex >= str.Length)
            {
                return 0;
            }

            str = str.Substring(startIndex);
            return SubCount(str, subStr);
        }
        #endregion

        #region [ MaxLength ]
        /// <summary>
        /// Shorten the string to the max length, if less than max length then the string return as is
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns></returns>
        /// <example>Use this to maintain the buffer size, ensure it doesn't grow too large.</example>
        public static string MaxLength(string input, int maxLength, bool showEllipsis = false)
        {
            if (input.Length <= maxLength)
            {
                return input;
            }

            // eclipse length is 4 " ..."
            if (!showEllipsis || input.Length <= 4)
            {
                return input.Substring(0, maxLength);
            }
            else
            {
                input = MaxLength(input, maxLength - 4, false);
                return input + " ...";
            }
        }
        #endregion

        #region [ Base64 Encode & Decode ]
        /// <summary>
        /// Convert string to base64 encoded string.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <returns></returns>
        public static string Base64Encode(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// Decode base64 string into regular string.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <returns></returns>
        public static string Base64Decode(string str)
        {
            try
            {
                byte[] decbuff = Convert.FromBase64String(str);
                return Encoding.UTF8.GetString(decbuff);
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
                return null;
            }
        }
        #endregion

        #region [ Repeat ]
        /// <summary>
        /// Repeats the specified string (ex. Repeat("ABC", 3); => ABCABCABC).
        /// </summary>
        /// <param name="str">The string you want to repeat.</param>
        /// <param name="count">The count.</param>
        public static string Repeat(string str, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }
        #endregion

        #region [ GetMatchingStart ]
        /// <summary>
        /// Gets the common part of the start of the strings.
        /// </summary>
        /// <param name="str">The strings.</param>
        /// <example>"Hello Ben" + "Hello Pete" => "Hello "</example>
        /// <returns></returns>
        public static string GetMatchingStart(params string[] str)
        {
            return GetMatchingStart(true, str);
        }

        /// <summary>
        /// Gets the common part of the start of the strings.
        /// </summary>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string GetMatchingStart(bool ignoreCase, params string[] str)
        {
            string matching = "";

            for (int i = 0; true; i++)
            {
                char chr = i < str[0].Length ? str[0][i] : (char)0;

                for (int j = 1; j < str.Length; j++)
                {
                    string line = str[j];

                    if (i < line.Length)
                    {
                        if (Chr.IsNotEquals(chr, line[i]))
                        {
                            return matching;
                        }
                    }
                    else
                    {
                        return matching;
                    }
                }

                matching += chr.ToString();
            }
        }
        #endregion

        #region [ Split ]
        /// <summary>
        /// Splits the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="count">The count, set it to 0 to not limit the number of parts.</param>
        /// <param name="opt">The split option.</param>
        /// <returns></returns>
        public static string[] Split(string str, string separator, int count = 0, StringSplitOptions opt = StringSplitOptions.RemoveEmptyEntries)
        {
            return Split(str, new string[] { separator }, count, opt);
        }

        /// <summary>
        /// Splits the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="separators">The separators.</param>
        /// <param name="count">The count, set it to 0 to not limit the number of parts.</param>
        /// <param name="opt">The split option.</param>
        /// <returns></returns>
        public static string[] Split(string str, string[] separators, int count = 0, StringSplitOptions opt = StringSplitOptions.RemoveEmptyEntries)
        {
            if (count > 0)
            {
                return str.Split(separators, count, opt);
            }
            return str.Split(separators, opt);
        }

        /// <summary>
        /// Splits and return the value at the specified index.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static string SplitIndex(string str, string separator, int index, int count = 0)
        {
            string[] split = Str.Split(str, separator, count);
            if (index < split.Length)
            {
                return split[index];
            }
            return str;
        }
        #endregion

        #region [ Start & End With ]
        /// <summary>
        /// Starts the with (ignore case by default).
        /// </summary>
        /// <param name="s1">The s1.</param>
        /// <param name="s2">The s2.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static bool StartsWith(string s1, string s2, bool ignoreCase = true)
        {
            return s1.StartsWith(s2, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Endses the with (ignore case by default).
        /// </summary>
        /// <param name="s1">The s1.</param>
        /// <param name="s2">The s2.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static bool EndsWith(string s1, string s2, bool ignoreCase = true)
        {
            return s1.EndsWith(s2, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion

        #region [ Replace ]
        /// <summary>
        /// Just like string.replace but with ignoreCase option (ignore case by default)
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="search">The search.</param>
        /// <param name="replace">The replace.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static string Replace(string text, string search, string replace, bool ignoreCase = true)
        {
            if (!ignoreCase)
            {
                return text.Replace(search, replace);
            }

            search = Regex.Escape(search);
            return Regex.Replace(text, search, replace, RegexOptions.IgnoreCase);
        }
        #endregion

        #region [ Links ]
        /// <summary>
        /// Extracts the links.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static List<string> ExtractLinks(string text)
        {
            var list = new List<string>();
            MatchCollection matches = Regex.Matches(text, @"(http|https|ftp)[^\s\]""'<>]+");
            foreach (Match m in matches)
            {
                var link = WebTools.UrlDecode(m.Value);
                if (!list.Contains(link))
                {
                    list.Add(link);
                }
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Make the the name is valid to be use a variable name.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <returns></returns>
        public static string CleanVarName(string varName)
        {
            varName = Regex.Replace(varName, @"^\d+", "");
            varName = Regex.Replace(varName, @"[^a-z_A-Z]", "");

            return varName;

        }
    }
}
