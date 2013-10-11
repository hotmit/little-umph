using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LittleUmph
{
    /// <summary>
    /// The Error Levels
    /// </summary>
    [Flags]
    public enum ErrorLevel
    {
        /// <summary>
        /// 
        /// </summary>
        Trace = 0,
        /// <summary>
        /// 
        /// </summary>
        Info = 1,
        /// <summary>
        /// 
        /// </summary>
        Warn = 2,
        /// <summary>
        /// 
        /// </summary>
        Error = 4,
        /// <summary>
        /// 
        /// </summary>
        Fatal = 16
    }

    /// <summary>
    /// Log information to a text dir.
    /// </summary>
    public class ELog
    {
        #region [ Constants ]
        private const int INT_BytesInMeg = 1048576;
        #endregion

        #region [ Private Variables ]
        private string _LogFilePath     = "Log.txt";
        private int    _LogMaxSize      = 5;
        private string _DateFormat      = "MM/dd/yyyy hh:mm:ss.fff tt";
        private string _FieldsSeparator = "\t";

        private FileInfo _logFileInfo;
        private long _maxLogSizeInByte;
        #endregion

        #region [ Properties ]
        private bool _Enable = true;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ELog"></see> is enable.
        /// </summary>
        /// <value>
        /// <c>true</c> if enable; otherwise, <c>false</c>.</value>
        public bool Enable
        {
            get
            {
                return _Enable;
            }
            set
            {
                _Enable = value;
            }
        }

        /// <summary>
        /// Gets or sets the log dir path (relative or absolute path include filename and extension).
        /// </summary>
        /// <value>The log dir path.</value>
        public string LogFilePath
        {
            get
            {
                return _LogFilePath;
            }
            set
            {
                _LogFilePath = value;

                if (!LogFilePath.Contains(":"))
                {
                    _LogFilePath = Path.Combine(IOFunc.ExecutableDirectory, _LogFilePath);
                }

                _logFileInfo = new FileInfo(_LogFilePath);
            }
        }

        /// <summary>
        /// Gets or sets the size of the log max in megabytes (default is 5MB).
        /// </summary>
        /// <value>The size of the log max in megabytes.</value>
        public int LogMaxSize
        {
            get
            {
                return _LogMaxSize;
            }
            set
            {
                _LogMaxSize = value;
                _maxLogSizeInByte = _LogMaxSize * INT_BytesInMeg;
            }
        }

        /// <summary>
        /// Gets or sets the fields separator (default is horizontal tab).
        /// </summary>
        /// <value>The fields separator.</value>
        public string FieldsSeparator
        {
            get
            {
                return _FieldsSeparator;
            }
            set
            {
                _FieldsSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the date format (default is MM/dd/yyyy hh:mm:ss.fff tt).
        /// </summary>
        /// <value>The date format.</value>
        public string DateFormat
        {
            get
            {
                return _DateFormat;
            }
            set
            {
                _DateFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log trace entry of the Error Level.
        /// </summary>
        /// <value><c>true</c> if [log trace]; otherwise, <c>false</c>.</value>
        public bool LogTrace { get; set; }

        /// <summary>
        /// Gets a value indicating whether log dir exists.
        /// </summary>
        /// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
        public bool Exists
        {
            get
            {
                return LogExist();
            }
        }
        #endregion

        #region [ Constructors ]
        public ELog()
        {
            LogFilePath = _LogFilePath;
            LogMaxSize  = _LogMaxSize;
        }
        #endregion

        #region [ Write Log ]
        /// <summary>
        /// Writes the specified error level.
        /// </summary>
        /// <param name="errorLevel">The error level.</param>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages (log as comma delimited).</param>
        public virtual void Write(ErrorLevel errorLevel, string header, params object[] messages)
        {
            if (messages.Length > 0)
            {
                string message = header + ": ";
                foreach (object msg in messages)
                {
                    if (msg is Exception)
                    {
                        message += ((Exception)msg).Message + ", ";
                    }                    
                    else
                    {
                        message += msg.ToString() + ", ";
                    }
                }
                message = message.Trim(',', ' ');
                WriteFormat(errorLevel, "{0}", message);
            }
            else
            {
                WriteFormat(errorLevel, "{0}", header.TrimEnd(' '));
            }
        }

        /// <summary>
        /// Dumbs the object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public string DumbObject(object data)
        {
            string message = "";
            if (data == null)
            {
                return "NULL";
            }

            if (data is IEnumerable)
            {
                IEnumerable list = (IEnumerable)data;
                message += "[";
                foreach (object o in list)
                {
                    message += o.ToString() + ", ";
                }
                message = message.Trim(',', ' ');
                message += "], ";
            }
            else
            {
                message = data.ToString();
            }

            message = message.Trim(',', ' ');
            return message;
        }

        /// <summary>
        /// Writes log with the specified format.
        /// </summary>
        /// <param name="errorLevel">The error level.</param>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void WriteFormat(ErrorLevel errorLevel, string formatString, params object[] messages)
        {
            if (!Enable || (errorLevel == ErrorLevel.Trace && !LogTrace))
            {
                return;
            }

            try
            {
                string log = string.Format(formatString, messages);

                log = log.Replace("\r\n", " <CRLF> ");
                log = log.Replace("\n", " <LF> ");

                string errorLevelField = errorLevel.ToString().PadRight(5, ' ');

                log = DateTime.Now.ToString(DateFormat) + FieldsSeparator +
                    errorLevelField + FieldsSeparator + log + "\r\n";

                File.AppendAllText(LogFilePath, log);

                _logFileInfo.Refresh();

                if (_logFileInfo.Length > _maxLogSizeInByte)
                {
                    string dir = Path.GetDirectoryName(LogFilePath);
                    string file = Path.GetFileNameWithoutExtension(LogFilePath);
                    string extension = Path.GetExtension(LogFilePath);

                    string bakLogPath = string.Format(@"{0}\{1}_Old{2}", dir, file, extension);

                    if (File.Exists(bakLogPath))
                    {
                        File.Delete(bakLogPath);
                    }
                    File.Move(LogFilePath, bakLogPath);
                }
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
            }
        }

        /// <summary>
        /// Writes the information in pair (ie take the "pairs" 
        /// params treat the first one as title the second as the data)
        /// </summary>
        /// <param name="errorLevel">The error level.</param>
        /// <param name="header">The header.</param>
        /// <param name="pairs">The pairs.</param>
        public void WritePairs(ErrorLevel errorLevel, string header, params object[] pairs)
        {
            if (pairs.Length % 2 != 0)
            {
                throw new Exception("Pairs must come in even set.");
            }

            try
            {
                if (pairs.Length == 0)
                {
                    Write(errorLevel, header);
                    return;
                }

                string message = "[";
                for (int i = 0; i < pairs.Length; i += 2)
                {
                    string name = pairs[i] == null ? "" : pairs[i].ToString();
                    string value = pairs[i + 1] == null ? "NULL" : pairs[i+1].ToString();
                    message = string.Format("{0}: {1}, ", name.Trim(), value.Trim());
                }
                message = message.TrimEnd(',', ' ') + "]";

                Write(errorLevel, header, message);
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
            }
        }
        #endregion

        #region [ Log Shortcut ]
        /// <summary>
        /// Write trace log.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages.</param>
        public void Trace(string header, params object[] messages)
        {
            Write(ErrorLevel.Trace, header, messages);
        }

        /// <summary>
        /// Write trace log with specified format.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void TraceF(string formatString, params object[] messages)
        {
            WriteFormat(ErrorLevel.Trace, formatString, messages);
        }

        /// <summary>
        /// Write info log.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages.</param>
        public void Info(string header, params object[] messages)
        {
            Write(ErrorLevel.Info, header, messages);
        }

        /// <summary>
        /// Write info log with specified format.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void InfoF(string formatString, params object[] messages)
        {
            WriteFormat(ErrorLevel.Info, formatString, messages);
        }

        /// <summary>
        /// Write warn log.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages.</param>
        public void Warn(string header, params object[] messages)
        {
            Write(ErrorLevel.Warn, header, messages);
        }

        /// <summary>
        /// Write warn log with specified format.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void WarnF(string formatString, params object[] messages)
        {
            WriteFormat(ErrorLevel.Warn, formatString, messages);
        }

        /// <summary>
        /// Write error log.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages.</param>
        public void Error(string header, params object[] messages)
        {
            Write(ErrorLevel.Error, header, messages);
        }

        /// <summary>
        /// Write error log with specified format.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void ErrorF(string formatString, params object[] messages)
        {
            WriteFormat(ErrorLevel.Error, formatString, messages);
        }

        /// <summary>
        /// Write fatal log.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="messages">The messages.</param>
        public void Fatal(string header, params object[] messages)
        {
            Write(ErrorLevel.Fatal, header, messages);
        }

        /// <summary>
        /// Write fatal log with specified format.
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="messages">The messages.</param>
        public void FatalF(string formatString, params object[] messages)
        {
            WriteFormat(ErrorLevel.Fatal, formatString, messages);
        }
        #endregion

        #region [ Log Management ]
        /// <summary>
        /// Deletes the log.
        /// </summary>
        public void DeleteLog()
        {
            if (File.Exists(_logFileInfo.FullName))
            {
                _logFileInfo.Delete();
                _logFileInfo.Refresh();
            }
        }

        /// <summary>
        /// Views the log.
        /// </summary>
        public void ViewLog()
        {
            if (File.Exists(_logFileInfo.FullName))
            {
                IOFunc.RunCmd(_logFileInfo.FullName);
            }
        }

        /// <summary>
        /// Logs the exist.
        /// </summary>
        /// <returns></returns>
        public bool LogExist()
        {
            return File.Exists(_logFileInfo.FullName);
        }
        #endregion
    }
}
