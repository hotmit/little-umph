using System;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif
using System.Text;
using System.Text.RegularExpressions;

namespace LittleUmph
{
    /// <summary>
    /// 
    /// </summary>
    public class Diff
    {
        #region [ Words Match ]
        /// <summary>
        /// Count common words, if the common words between to string
        /// exceeds the specified tolerant of the total words the then 
        /// it is a match (case-insensitive, non alphanum will be strip)
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <param name="tolerant">The tolerant (percent value from 0 to 1.0).</param>
        /// <returns>
        /// 	<c>true</c> if the specified text is match; otherwise, <c>false</c>.
        /// </returns>
        /// <example>"Hello world! 1" and "hello world, 2" => this would yield a match of 33% (1match/3total words)</example>
        public static bool IsWordMatch(string a, string b, double tolerant)
        {
            double percent = WordMatchPercent(a, b);
            return percent >= tolerant;
        }

        /// <summary>
        /// Calculate the percental of the number of common words
        /// and the total words (case-insensitive, non alphanum will be strip)
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns></returns>
        /// <example>"Hello world! 1" and "hello world, 2" => this would yield a match of 33% (1match/3total words)</example>
        public static double WordMatchPercent(string a, string b)
        {
            string[] aWords = getWords(a);
            string[] bWords = getWords(b);

            // number of matching words
            int match = intersectWords(aWords, bWords);

            // think of the Venn Diagram, get the total number of words from
            // both sides and remove the duplicate and common middle 
            // to get the total
            int totalWords = aWords.Length + bWords.Length - match;

            return match / (totalWords * 1.0);
        }

        /// <summary>
        /// Number of matching words needle in the haystack.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <returns></returns>
        public static int WordContains(string haystack, string needle)
        {
#if NET35_OR_GREATER
            string[] hWords = getWords(haystack);
            string[] needles = getWords(needle);

            int count = 0;
            foreach (var n in needles)
            {
                string match = hWords.FirstOrDefault(w => Str.IsEqual(w, n));
                if (match != null)
                {
                    count++;
                }
            }
            return count;
#else
            throw new NotImplementedException("Only support framework 3.5+");
#endif
        }

        /// <summary>
        /// Number of words in the needle matches the words in haystack.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        /// <param name="needle">The needle.</param>
        /// <returns></returns>
        public static double WordContainsPercent(string haystack, string needle)
        {
            int words = WordContains(haystack, needle);
            int total = getWords(needle).Length;

            return words / (total * 1.0);
        }
        #region [ Helper ]
        /// <summary>
        /// Count how many matching words between the two list (case-insensitive)
        /// </summary>
        /// <param name="listA">The list A.</param>
        /// <param name="listB">The list B.</param>
        /// <returns></returns>
        private static int intersectWords(IList<string> listA, IList<string> listB)
        {
#if NET35_OR_GREATER
            var matches = (from h in listA
                          from n in listB
                          where fuzzyMatch(h,n)
                          select h).Count();
#else
            int matches = 0;
            foreach (string a in listA)
            {
                foreach (string b in listB)
                {
                    if (fuzzyMatch(a, b))
                    {
                        matches++;
                        break;
                    }
                }
            }
#endif
            return matches;
        }

        private static bool fuzzyMatch(string a, string b)
        {
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove all non-alphanum characters and split the string into words.
        /// </summary>
        /// <param name="wordsString">The words string.</param>
        /// <returns></returns>
        private static string[] getWords(string wordsString)
        {
            // for words like "it's" "don't"
            // you want to keep the letter together
            wordsString = wordsString.Replace("'", "");

            wordsString = Regex.Replace(wordsString, @"[\W_]", " ");
            wordsString = Regex.Replace(wordsString, " +", " ");
            wordsString = wordsString.Trim();

            string[] words = wordsString.Split(' ');
            return words;
        }
        #endregion
        #endregion

        #region [ Levenshtein ]
        /// <summary>
        /// Calculate the different between two strings.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>http://www.dotnetperls.com/levenshtein</remarks>
        public static int Levenshtein(string source, string key)
        {
            int n = source.Length;
            int m = key.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (key[j - 1] == source[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        #region [ Other Implementation Of Levenshtein Distance ]
        ///// <summary>
        ///// Levenshteins the specified a.
        ///// </summary>
        ///// <param name="a">A.</param>
        ///// <param name="b">The b.</param>
        ///// <returns></returns>
        //public static int Levenshtein(String a, String b)
        //{
        //    if (string.IsNullOrEmpty(a))
        //    {
        //        if (!string.IsNullOrEmpty(b))
        //        {
        //            return b.Length;
        //        }
        //        return 0;
        //    }

        //    if (string.IsNullOrEmpty(b))
        //    {
        //        if (!string.IsNullOrEmpty(a))
        //        {
        //            return a.Length;
        //        }
        //        return 0;
        //    }

        //    Int32 cost;
        //    Int32[,] d = new int[a.Length + 1, b.Length + 1];
        //    Int32 min1;
        //    Int32 min2;
        //    Int32 min3;

        //    for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
        //    {
        //        d[i, 0] = i;
        //    }

        //    for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
        //    {
        //        d[0, i] = i;
        //    }

        //    for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
        //    {
        //        for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
        //        {
        //            cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

        //            min1 = d[i - 1, j] + 1;
        //            min2 = d[i, j - 1] + 1;
        //            min3 = d[i - 1, j - 1] + cost;
        //            d[i, j] = Math.Min(Math.Min(min1, min2), min3);
        //        }
        //    }

        //    return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        //}


        ///// <summary>
        ///// LDs the specified s.
        ///// </summary>
        ///// <param name="s">The s.</param>
        ///// <param name="t">The t.</param>
        ///// <remarks>Levenshtein Distance Algorithm: C# Implementation by Lasse Johansen</remarks>
        ///// <see cref="http://www.merriampark.com/ldcsharp.htm"/>
        ///// <returns></returns>
        //public static int LD(string s, string t)
        //{
        //    int n = s.Length; //length of s
        //    int m = t.Length; //length of t
        //    int[,] d = new int[n + 1, m + 1]; // matrix
        //    int cost; // cost
        //    if (n == 0) return m;
        //    if (m == 0) return n;
        //    for (int i = 0; i <= n; d[i, 0] = i++) ;
        //    for (int j = 0; j <= m; d[0, j] = j++) ;
        //    for (int i = 1; i <= n; i++)
        //    {
        //        for (int j = 1; j <= m; j++)
        //        {
        //            cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
        //            d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
        //                      d[i - 1, j - 1] + cost);
        //        }
        //    }
        //    return d[n, m];
        //}
        #endregion
        #endregion

    }
}
