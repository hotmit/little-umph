using System;

namespace LittleUmph
{
    /// <summary>
    /// Contains common system enviroment paths (eg. Desktop, Windows, etc.)
    /// </summary>
    public class SystemPath
    {
        /// <summary>
        /// Desktop path with the ended slash.
        /// </summary>
        public static readonly string Desktop;

        /// <summary>
        /// Path to the window directory (with the ended slash).
        /// </summary>
        public static readonly string WindowPath;

        /// <summary>
        /// Path to the Program Files directory (with the ended slash).
        /// </summary>
        public static readonly string ProgramFiles;

        /// <summary>
        /// Path to the Application Data directory (with the ended slash).
        /// </summary>
        public static readonly string ApplicationData;
        
        /// <summary>
        /// Path to the Common Application Data directory (with the ended slash).
        /// </summary>
        public static readonly string CommonApplicationData;

        /// <summary>
        /// Path to the System32 directory (with the ended slash).
        /// </summary>
        public static readonly string System32;

        /// <summary>
        /// Initializes the <see cref="SystemPath"/> class.
        /// </summary>
        static SystemPath()
        {
            Desktop = AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            WindowPath = AddSlash(Environment.GetEnvironmentVariable("SystemRoot"));
            ProgramFiles = AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            ApplicationData = AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            CommonApplicationData = AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            System32 = AddSlash(Environment.SystemDirectory);
        }

        /// <summary>
        /// Adds the slash at the end of the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string AddSlash(string path)
        {
            if (!path.EndsWith("\\"))
            {
                return path + "\\";
            }
            return path;
        }
    }
}
