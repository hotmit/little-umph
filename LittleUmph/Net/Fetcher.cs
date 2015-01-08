using System;
using System.Text.RegularExpressions;

////Todo: consolidate this class with WebTool

namespace LittleUmph
{
    /// <summary>
    /// Get data from webpages.
    /// </summary>
    public class Fetcher
    {
        /// <summary>
        /// Get the page and try to extract it
        /// </summary>
        /// <param name="urlOrHTML"></param>
        /// <param name="regex"></param>
        /// <param name="regexIndex">regex index</param>
        /// <returns>return empty string if not found</returns>
        public static string Fetch(string urlOrHTML, Regex regex, int regexIndex)
        {
            if (Str.IsEmpty(urlOrHTML))
            {
                return "";
            }

            string page = urlOrHTML;
            if (urlOrHTML.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
            {
                page = WebTools.GetPage(urlOrHTML);
            }

            Match m = regex.Match(page);

            if (m.Success)
            {
                return m.Groups[regexIndex].Value;
            }
            return "";
        }

        /// <summary>
        /// Get the page using regex to extract the link
        /// </summary>
        /// <param name="urlOrHTML"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public static string Fetch(string urlOrHTML, Regex regex)
        {
            return Fetch(urlOrHTML, regex, 0);
        }

        /// <summary>
        /// Fetch one page, fetch first page -> extract link, fetch second page -> extract data or link
        /// </summary>
        /// <param name="urlOrHTML"></param>
        /// <param name="firstRegex"></param>
        /// <param name="firstIndex"></param>
        /// <param name="secondRegex"></param>
        /// <param name="secondIndex"></param>
        /// <returns></returns>
        public static string Fetch2Source(string urlOrHTML, Regex firstRegex, int firstIndex, Regex secondRegex, int secondIndex)
        {
            string firstLink = Fetch(urlOrHTML, firstRegex, firstIndex);
            string secondLink = Fetch(firstLink, secondRegex, secondIndex);

            return secondLink;
        }
    }
}
