using System;
using System.Text.RegularExpressions;

namespace LittleUmph
{
    /// <summary>
    /// Some Viet functions.
    /// </summary>
    public class VTools
    {
        #region [ Private Variables ]
        private const string _VNA = "[àáâãăạảấầẩẫậắằẳẵặ]";
        private const string _VNO = "[òóôõơọỏốồổỗộớờởỡợ]";
        private const string _VNE = "[èéêẹẻẽếềểễệ]";
        private const string _VNU = "[ùúũưụủứừửữự]";
        private const string _VNI = "[ìíĩỉị]";
        private const string _VNY = "[ýỳỵỷỹ]";
        private const string _VND = "[đð]";

        // This order have been optimized
        // Use this to detect the present to Viet's Chars.
        private const string _fullRange = "đàôưóếáìêạâờộấảớốậầơồắúãệợểủềịọòứừíởữũặăỏùựổỉụẹẽéĩửýằẫẳỗẻễẩèỳõỡỹỷỵẵð";
        #endregion

        #region [ FuzzIt ]
        /// <summary>
        /// Replace viet char with the english equivalent
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FuzzIt(string text)
        {
            string newTxt = text;

            newTxt = Regex.Replace(newTxt, _VNA, "a");
            newTxt = Regex.Replace(newTxt, _VNO, "o");
            newTxt = Regex.Replace(newTxt, _VNE, "e");
            newTxt = Regex.Replace(newTxt, _VNU, "u");
            newTxt = Regex.Replace(newTxt, _VNI, "i");
            newTxt = Regex.Replace(newTxt, _VNY, "y");
            newTxt = Regex.Replace(newTxt, _VND, "d");

            newTxt = Regex.Replace(newTxt, _VNA.ToUpper(), "A");
            newTxt = Regex.Replace(newTxt, _VNO.ToUpper(), "O");
            newTxt = Regex.Replace(newTxt, _VNE.ToUpper(), "E");
            newTxt = Regex.Replace(newTxt, _VNU.ToUpper(), "U");
            newTxt = Regex.Replace(newTxt, _VNI.ToUpper(), "I");
            newTxt = Regex.Replace(newTxt, _VNY.ToUpper(), "Y");
            newTxt = Regex.Replace(newTxt, _VND.ToUpper(), "D");

            return newTxt;
        } 
        #endregion

        #region [ Is Viet Checking ]
        /// <summary>
        /// Does the text contains viet chars.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="vCount">Number of viet letter count before declare it's a Viet text. 
        /// The count is unique, é and é for example only count as one.</param>
        /// <param name="length">Only examine the first number of "length" characters. Use this to optimize you check.</param>
        /// <returns>
        /// 	<c>true</c> if the specified text is viet; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsViet(string text, int vCount, int length)
        {
            text = Str.MaxLength(text, length);

            for (int i = 0; i < _fullRange.Length; i++)
            {
                if (text.IndexOf(_fullRange[i]) > -1)
                {
                    vCount--;

                    if (vCount < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check to see any Viet char is in the string.
        /// At least must contains 3 Viet chars and within 1024 first char only.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// 	<c>true</c> if the specified text is viet; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsViet(string text)
        {
            return IsViet(text, 3, 1024);
        } 
        #endregion

        #region [ String Contains ]
        /// <summary>
        /// Fuzz both string and then check to see if the needle is in the haystack (case insensitive).
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static bool Contains(string haystack, string needle)
        {
            needle = FuzzIt(needle);

            if (IsViet(haystack))
            {
                haystack = FuzzIt(haystack);
            }

            return Str.Contains(haystack, needle, true);
        } 
        #endregion
    }
}
