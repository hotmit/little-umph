using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    /// <summary>
    /// Javascript wrapper and tools
    /// </summary>
    public class JTool
    {
        /// <summary>
        /// Escape string, replace " with \"
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string StringEscape(string txt)
        {
            return txt.Replace("\"", "\\\"");
        }
    }
}
