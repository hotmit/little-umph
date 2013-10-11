using System;
using System.Collections.Generic;
using System.Text;

namespace LittleUmph
{
    #if NET20
    public delegate void Action();
    public delegate void Action<T>(T arr);
    #endif

    /// <summary>
    /// A class to store library wide supports.
    /// </summary>
    public class Gs
    {
        private static ELog _Log = new ELog();

        /// <summary>
        /// Gets or sets the global log instance.
        /// </summary>
        /// <value>The log.</value>
        public static ELog Log
        {
            get
            {
                return _Log;
            }
            set
            {
                _Log = value;
            }
        }

        static Gs()
        {
            Log.LogFilePath = "LittleUmpLog.txt";
            Log.LogMaxSize = 1;

            #if !DEBUG
                Log.Enable = false;
            #endif
        }
    }
}
