using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LittleUmph
{
    /// <summary>
    /// File line count and retrieve text from a specified line number.
    /// </summary>
    public class FileLine
    {
        private List<long> _Indexes = new List<long>();

        /// <summary>
        /// Gets the number of lines in the file.
        /// </summary>
        /// <value>
        /// The line count.
        /// </value>
        public int LineCount { get; private set; }

        /// <summary>
        /// Gets the text file.
        /// </summary>
        /// <value>
        /// The text file.
        /// </value>
        public FileInfo TextFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLine" /> class.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public FileLine(string filepath) : this(new FileInfo(filepath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLine" /> class.
        /// </summary>
        /// <param name="file">The file.</param>
        public FileLine(FileInfo file)
        {
            TextFile = file;

            using (var fs = TextFile.OpenRead())
            {
                _Indexes.Add(fs.Position);
                int chr;
                while ((chr = fs.ReadByte()) != -1)
                {
                    if (chr == '\n')
                    {
                        _Indexes.Add(fs.Position);
                    }
                }
            }

            LineCount = _Indexes.Count;
        }

        /// <summary>
        /// Gets the text at the specified line number (one index).
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>Return null if the index is out of range.</returns>
        public string GetLine(int lineNumber)
        {
            List<string> lines = GetLines(lineNumber, 1);
            if (lines != null && lines.Count > 0)
            {
                return lines[0];
            }
            return null;
        }

        /// <summary>
        /// Gets the first line.
        /// </summary>
        /// <returns></returns>
        public string GetFirstLine()
        {
            return GetLine(1);
        }

        /// <summary>
        /// Gets the last line.
        /// </summary>
        /// <returns></returns>
        public string GetLastLine()
        {
            return GetLine(LineCount);
        }

        /// <summary>
        /// Gets the lines.
        /// </summary>
        /// <param name="startLineNumber">The start line number (one index).</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public List<string> GetLines(int startLineNumber, int count)
        {
            // make it a zero-index, easier to get data from the index
            startLineNumber--;

            int num = Num.Filter(startLineNumber, 0, LineCount - 1, -1, -1);
            if (num == -1)
            {
                return null;
            }

            List<string> list = new List<string>();

            string lineContent = "";
            using (var fs = TextFile.OpenRead())
            {
                fs.Position = _Indexes[startLineNumber];
                using (var sr = new StreamReader(fs))
                {
                    for (int i = 0; i < count && (lineContent = sr.ReadLine()) != null; i++)
                    {
                        list.Add(lineContent);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the range.
        /// </summary>
        /// <param name="startLineNumber">The start line number.</param>
        /// <param name="endLineNumber">The end line number (inclusive).</param>
        /// <returns></returns>
        public List<string> GetRange(int startLineNumber, int endLineNumber)
        {
            int count = (endLineNumber - startLineNumber) + 1;
            return GetLines(startLineNumber, count);
        }
    }
}
