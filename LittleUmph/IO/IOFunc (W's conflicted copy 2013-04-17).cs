using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Collections;
// Must import manually
using System.Management;
using System.Net.Sockets;

using LittleUmph;

namespace LittleUmph
{
    /// <summary>
    /// Common IO functions
    /// </summary>
    public class IOFunc
    {
        #region [ Path ]
        /// <summary>
        /// Gets the executable path (with the exe).
        /// </summary>
        /// <value>The executable path.</value>
        public static string ExecutablePath
        {
            get
            {
                StringBuilder path = new StringBuilder(1024);
                GetModuleFileName(IntPtr.Zero, path, path.Capacity);

                return path.ToString();
            }
        }

        /// <summary>
        /// Gets the executable folder (without the exe, with ended slash).
        /// </summary>
        /// <value>The executable folder.</value>
        public static string ExecutableDirectory
        {
            get
            {
                string path = Path.GetDirectoryName(ExecutablePath);
                return AddSlash(path);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        /// <summary>
        /// Gets the desktop folder path.
        /// </summary>
        /// <value>The desktop directory.</value>
        public static string DesktopDirectory
        {
            get
            {
                return AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            }
        }

        /// <summary>
        /// Gets my document directory.
        /// </summary>
        /// <value>My document directory.</value>
        public static string MyDocumentDirectory
        {
            get
            {
                return AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        }

        /// <summary>
        /// Gets the user profile directory.
        /// </summary>
        /// <value>The user profile directory.</value>
        public static string UserProfileDirectory
        {
            get
            {
                return AddSlash(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            }
        }

        /// <summary>
        /// Adds the slash to the end of a directory path if needed ( C:\Desktop => C:\Desktop\ ).
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns></returns>
        public static string AddSlash(string directoryPath)
        {
            string slash = Path.DirectorySeparatorChar.ToString();

            if (directoryPath.EndsWith(slash))
            {
                return directoryPath;
            }

            return directoryPath + slash;
        }
        #endregion

        #region [ System Process ]
        /// <summary>
        /// Check the process list to see if an instance exist
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool ProcessIsRunning(string processName)
        {
            List<string> pList = ProcessList();
            return pList.Contains(processName);
        }

        /// <summary>
        /// Get a list of current running process name (same as Process.GetProcesses(), but return array of string instead)
        /// </summary>
        /// <returns></returns>
        public static List<string> ProcessList()
        {
            Process[] processlist = Process.GetProcesses();
            List<string> pList = new List<string>();
            foreach (Process p in processlist)
            {
                pList.Add(p.ProcessName);
            }
            return pList;
        }

        /// <summary>
        /// Kill process using process id
        /// </summary>
        /// <param name="pid">The process ID number.</param>
        /// <returns>True if success or PID is not found.</returns>
        public static bool ProcessKill(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
                return true;
            }
            catch (ArgumentException ex)
            {
                // the process already existed
                return true;
            }
            catch (Exception xpt)
            {
                return false;
            }
        }

        /// <summary>
        /// Kill process using process name
        /// </summary>
        /// <param name="processName">Name of the process (without the extension).</param>
        /// <returns>True if success or Process Name not found.</returns>
        public static bool ProcessKill(string processName)
        {
            Process[] processlist = Process.GetProcessesByName(processName);
            foreach (Process p in processlist)
            {
                try
                {
                    p.Kill();
                }
                catch (ArgumentException)
                {
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Processes the exist.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <returns></returns>
        public static bool ProcessExist(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }


        /// <summary>
        /// Determines whether current process already running.
        /// Use for single instance application detection.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the current process si already running; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentProcessAlreadyRunning()
        {
            // Detect existing instances
            string processName = Process.GetCurrentProcess().ProcessName;
            return Process.GetProcessesByName(processName).Length > 1;
        }
        #endregion

        #region [ File Manipulation ]
        /// <summary>
        /// Force delete a dir (even if the dir is read-only and it does not complain when dir doesn't exist).
        /// </summary>
        /// <param name="filePath">The dir path.</param>
        /// <returns></returns>
        public static bool ForceDelete(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                return true;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("ForceDelete()", xpt.Message);
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified filename has extension (Path.HasExtension give false positive, this is an enhanced version of that).
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>
        ///   <c>true</c> if the specified filename has extension; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasExtension(string filename)
        {
            if (!Path.HasExtension(filename))
            {
                return false;
            }

            return Regex.IsMatch(filename, @"\.[0-9A-Z_]{1-4}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Renames the specified dir.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="newFileName">New name of the file.</param>
        /// <param name="appendExistingExtension">if set to <c>true</c> [use existing extension].</param>
        /// <returns></returns>
        public static bool Rename(FileInfo file, string newFileName)
        {
            try
            {
                if (!File.Exists(file.FullName))
                {
                    return false;
                }

                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                    file.Refresh();
                }

                string newName = newFileName;

                string oldExt = file.Extension;
                string newExt = Path.GetExtension(newFileName).ToLower();

                string oldName = Path.GetFileNameWithoutExtension(file.Name);
                newName = Path.GetFileNameWithoutExtension(newName);

                // If the name is the same then you don't need to rename
                if (oldName + oldExt == newName + newExt)
                {
                    return true;
                }                 

                // If the filename is just diff by case, then you can't just rename
                // you have to rename it to something else before rename it to the new name
                if (string.Compare(oldName + oldExt, newName + newExt, ignoreCase: true) == 0)
                {
                    string tempName = "";
                    while (true)
                    {
                        tempName = file.FullName + "_" + DateTime.Now.Ticks.ToString();;
                        if (!File.Exists(tempName))
                        {
                            break;
                        }
                    }

                    File.Move(file.FullName, tempName);
                    file = new FileInfo(tempName);
                }

                try
                {
                    string newFullPath = Path.Combine(file.Directory.FullName, newName) + newExt;
                    File.Move(file.FullName, newFullPath);
                    return true;
                }
                catch (Exception xpt)
                {
                    Gs.Log.Error("IOFunc.Rename()", xpt);
                    return false;
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Rename()", xpt.Message);
                return false;
            }
        }

        /// <summary>
        /// Renames the specified dir.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="newFolderName">New name of the folder.</param>
        /// <returns></returns>
        public static bool Rename(DirectoryInfo dir, string newFolderName)
        {
            try
            {
                if (!Directory.Exists(dir.FullName))
                {
                    return false;
                }

                string oldName = dir.Name;
                string newName = newFolderName;

                // If the name is the same then you don't need to rename
                if (oldName == newName)
                {
                    return true;
                }

                // If the filename is just diff by case, then you can't just rename
                // you have to rename it to something else before rename it to the new name
                if (string.Compare(oldName, newName, ignoreCase: true) == 0)
                {
                    string tempName = "";
                    while (true)
                    {
                        tempName = dir.FullName + "_" + DateTime.Now.Ticks.ToString(); ;
                        if (!Directory.Exists(tempName))
                        {
                            break;
                        }
                    }

                    File.Move(dir.FullName, tempName);
                    dir = new DirectoryInfo(tempName);
                }

                try
                {
                    string newFullPath = Path.Combine(dir.Parent.FullName, newName);
                    Directory.Move(dir.FullName, newFullPath);
                    return true;
                }
                catch (Exception xpt)
                {
                    Gs.Log.Error("IOFunc.Rename()", xpt);
                    return false;
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("Rename()", xpt.Message);
                return false;
            }
        }
        #endregion

        #region [ Path Functions ]
        /// <summary>
        /// Use the relative path value to resolve the absolute path
        /// relative to the current executable directory.
        /// </summary>
        /// <param name="relativePath">File name or a relative path.</param>
        /// <returns></returns>
        public static string ResolvePath(string relativePath)
        {
            return ResolvePath(IOFunc.ExecutableDirectory, relativePath);
        }

        /// <summary>
        /// Resolves the absolute path from the relative path.
        /// </summary>
        /// <param name="rootPath">The root path (this is the anchor point of the relative path).</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static string ResolvePath(string rootPath, string relativePath)
        {
            try
            {
                rootPath = rootPath.Replace("/", "\\").Trim();
                relativePath = relativePath.Replace("/", "\\").Trim();

                // if the relative path is a absolute path 
                // the just return that
                if (Path.IsPathRooted(relativePath))
                {
                    return relativePath;
                }

                // Ex: .\Label\Core.prn or Label\Core.prn
                if (relativePath.StartsWith(".\\") || !relativePath.StartsWith("."))
                {
                    relativePath = relativePath.TrimStart('.');
                    return Path.Combine(rootPath, relativePath);
                }
                else if (relativePath.StartsWith("..\\"))
                {
                    DirectoryInfo root = new DirectoryInfo(rootPath);

                    while (relativePath.StartsWith("..\\"))
                    {
                        relativePath = relativePath.Substring(3);
                        root = root.Parent;
                    }

                    return Path.Combine(root.FullName, relativePath);
                }
            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
            }

            return "";
        }

        /// <summary>
        /// Extract path from a fullpath with executable.
        /// C:\Run.exe =&gt; C:\
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns>with ended slash</returns>
        public static string GetPath(string fullpath)
        {
            if (fullpath.EndsWith("\\"))
            {
                return fullpath;
            }
            /*
            GetDirectoryName('C:\MyDir\MySubDir\myfile.oldExt') returns 'C:\MyDir\MySubDir'
            GetDirectoryName('C:\MyDir\MySubDir') returns 'C:\MyDir'
            GetDirectoryName('C:\MyDir\') returns 'C:\MyDir'
            GetDirectoryName('C:\MyDir') returns 'C:\'
            GetDirectoryName('C:\') returns ''
            */
            fullpath = Path.GetDirectoryName(fullpath);
            return fullpath + "\\";
        }
        #endregion

        #region [ Command Execution ]
        /// <summary>
        /// Runs the command (C:\Notepad++.exe" or C:\Program Files\Accessory\Notepad.exe).
        /// </summary>
        /// <param name="cmd">The command path.</param>
        public static bool RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }

        /// <summary>
        /// Runs the command (C:\Notepad++.exe" or C:\Program Files\Accessory\Notepad.exe).
        /// </summary>
        /// <param name="cmd">The relative or absolute command path.</param>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static bool RunCmd(string cmd, string arg)
        {
            try
            {
                cmd = PrepCmd(cmd);
                if (Str.IsEmpty(cmd))
                {
                    return false;
                }

                if (Str.IsEmpty(arg))
                {
                    Process.Start(cmd);
                }
                else
                {
                    Process.Start(cmd, arg);
                }
                return true;
            }
            catch (Exception xpt)
            {
                ///Todo: add exception handling here.
                return false;
            }
        }

        /// <summary>
        /// Prepare the command, make sure it is valid for execution.
        /// </summary>
        /// <param name="cmd">The CMD.</param>
        /// <returns></returns>
        private static string PrepCmd(string cmd)
        {
            if (cmd.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                return cmd;
            }

            // Remove the quote just to make the code consistent
            cmd = cmd.Trim('"');

            if (cmd.Length < 2)
            {
                return "";
            }

            // Not absolute path
            if (cmd[1] != ':')
            {
                string currectDirectory = FolderCurrent;
                cmd = ResolvePath(currectDirectory, cmd);
            }

            // Surround the path with quote if it has any space
            if (cmd.Contains(" "))
            {
                //cmd = String.Format("\"{0}\"", cmd);
            }

            return cmd;
        }

        /// <summary>
        /// Rums the command in silence mode.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="lineReceived">Each line received from the standard output.</param>
        /// <param name="onError">Pass back the error message if any.</param>
        /// <returns></returns>
        public static bool RunCmdSilence(string command, string arguments, 
            Action<string> lineReceived, Action<string> onError = null)
        {
            command = PrepCmd(command);
            if (Str.IsEmpty(command))
            {
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;

            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    char c;
                    string line = "";
                    while ((c = (char)process.StandardOutput.Read()) != 0)
                    {
                        if (c == '\n')
                        {
                            lineReceived(line);
                            line = "";
                        }
                        else
                        {
                            line += c.ToString();
                        }
                    }

                    process.WaitForExit();
                    string error = process.StandardError.ReadToEnd();
                }
                catch (Exception xpt)
                {
                    if (onError != null)
                    {
                        onError(xpt.Message);
                    }
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region [ FileIsAccessible ]
        /// <summary>
        /// Check to see if the dir is ready for use 
        /// (this is good for use with FileSystemWatcher events).
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="intervalDelay">Time in millisecond.</param>
        /// <param name="maxTry">Maximum number of tries before give up.</param>
        /// <returns></returns>
        public static bool FileIsAccessible(string filepath, int delay, int maxTry)
        {
            for (int i = 0; i < maxTry; i++)
            {
                if (!File.Exists(filepath))
                {
                    return false;
                }

                try
                {
                    using (FileStream inputStream = File.Open(filepath, FileMode.Open,
                                FileAccess.Read,
                                FileShare.None))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                }
                Thread.Sleep(delay);
            }
            return false;
        }
        #endregion

        #region [ Stream ]
        /// <summary>
        /// Read the stream and convert it to string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;

            byte[] buffer = new byte[4096];
            StringBuilder sb = new StringBuilder();

            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Take a string and put it in a memory stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static MemoryStream StringToStream(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;

            return ms;
        }

        public static byte[] StreamToBytes(Stream stream)
        {
            stream.Position = 0;
            List<byte> result = new List<byte>();
            byte[] buffer = new byte[4096];
            int len = 0;
            while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (len == buffer.Length)
                {
                    result.AddRange(buffer);
                }
                else
                {
                    byte[] fit = new byte[len];
                    Array.Copy(buffer, fit, len);
                    result.AddRange(fit);
                    fit = null;
                }
            }
            return result.ToArray();
        }
        #endregion

        #region [ Get Text From File ]
        /// <summary>
        /// Get text from a dir.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFromFile(string path)
        {
            return ReadFromFile(path, GetFileEncoding(path));
        }

        /// <summary>
        /// Reads from dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static string ReadFromFile(string path, Encoding encoding)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs, encoding))
                    {
                        string data = sr.ReadToEnd();
                        return data;
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Reads the dir from the end.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="bytesToRead">The bytes to read.</param>
        /// <returns></returns>
        public static string ReadFileFromTheEnd(string path, long bytesToRead)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    bytesToRead = Math.Min(bytesToRead, fs.Length);
                    long pos = fs.Length - bytesToRead;

                    fs.Seek(pos, SeekOrigin.Begin);

                    var buf = new byte[bytesToRead];
                    fs.Read(buf, 0, buf.Length);

                    string text = GetFileEncoding(path).GetString(buf);
                    return text;
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Get text from a dir and place it in the textbox.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ReadFromFile(string path, ref TextBox txt)
        {
            string text = ReadFromFile(path);
            txt.Text = text;
            return text;
        }

        /// <summary>
        /// Get text from a dir and place it in the Richtext.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rchtxt"></param>
        /// <returns></returns>
        public static string ReadFromFile(string path, ref RichTextBox rchtxt)
        {
            string text = ReadFromFile(path);
            rchtxt.Text = text;
            return text;
        }
        #endregion

        #region [ Save Text To File ]
        /// <summary>
        /// Writes to dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static bool WriteToFile(string path, string text)
        {
            return WriteToFile(path, text, GetFileEncoding(path));
        }

        /// <summary>
        /// Writes to dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="text">The text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static bool WriteToFile(string path, string text, Encoding encoding)
        {
            try
            {
                ConstructFilepath(path);
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (var sw = new StreamWriter(fs, encoding))
                    {
                        sw.Write(text);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the dir encoding. Return Encoding.Default if no Unicode BOM (byte order mark) is found.
        /// </summary>
        /// <param name="fileName">Name of the dir.</param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(String fileName)
        {
            if (!File.Exists(fileName))
            {
                return Encoding.Default;
            }

            try
            {
                var file = new FileInfo(fileName);
                using (FileStream fs = file.OpenRead())
                {
                    Encoding[] UnicodeEncodings = { Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF8 };
                    for (int i = 0; i < UnicodeEncodings.Length; i++)
                    {
                        fs.Position = 0;
                        byte[] preamble = UnicodeEncodings[i].GetPreamble();

                        byte[] buffer = new byte[preamble.Length];
                        fs.Read(buffer, 0, preamble.Length);

                        if (ByteArr.IsEquals(preamble, buffer))
                        {
                            return UnicodeEncodings[i];
                        }
                    }
                }
            }
            catch (IOException)
            {

            }
            return Encoding.Default;
        }

        /// <summary>
        /// Writes to dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="txt">The TXT.</param>
        /// <returns></returns>
        public static bool WriteToFile(string path, TextBox txt)
        {
            return WriteToFile(path, txt.Text);
        }

        /// <summary>
        /// Writes to dir.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="rchtxt">The RCHTXT.</param>
        /// <returns></returns>
        public static bool WriteToFile(string path, RichTextBox rchtxt)
        {
            return WriteToFile(path, rchtxt.Text);
        }

        /// <summary>
        /// Save to a dir, if it already exist increment the filename by one.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static bool SaveIncremental(string fullpath, string text)
        {
            ConstructFilepath(fullpath);
            int index;
            string path = fullpath;

            while (File.Exists(path))
            {
                string[] data = ExtractPath(path);

                Match m = Regex.Match(data[(int)ExtractPathIndex.FilenameNoExt], @"^(.+?)(\d+)$");
                if (m.Success)
                {
                    index = Convert.ToInt32(m.Groups[2].Value) + 1;
                    data[(int)ExtractPathIndex.FilenameNoExt] = m.Groups[1].Value.Trim() + string.Format(" {0:00}", index);
                }
                else
                {
                    data[(int)ExtractPathIndex.FilenameNoExt] = data[(int)ExtractPathIndex.FilenameNoExt].Trim() + " 01";
                }

                path = data[(int)ExtractPathIndex.NoNameWithSlash] + data[(int)ExtractPathIndex.FilenameNoExt] + data[(int)ExtractPathIndex.Extension];
            }

            return WriteToFile(path, text);
        }

        /// <summary>
        /// Save to a dir, if it already exist increment the filename by one.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static bool SaveDateIncremental(string fullpath, string text)
        {
            ConstructFilepath(fullpath);
            string[] data = ExtractPath(fullpath);

            string date = String.Format("{0:yyyy}-{0:MM}-{0:dd}", DateTime.Now);
            fullpath = data[(int)ExtractPathIndex.NoNameWithSlash] + date + " " + data[(int)ExtractPathIndex.Filename];

            return SaveIncremental(fullpath, text);
        }
        #endregion

        #region [ Enum ]
        /// <summary>
        /// When a dir or directory already exist. 
        /// This determine the action to perform on it.
        /// </summary>
        public enum ConflictRules
        {
            /// <summary>
            /// Ignore if a conflict occured.
            /// </summary>
            Ignore,

            /// <summary>
            /// Overwrite the old dir or directory with the new one.
            /// </summary>
            Overwrite,

            /// <summary>
            /// Overwrite if the two files are identical. 
            /// If not then AutoRename.
            /// </summary>
            OverwriteIfIdentical,

            /// <summary>
            /// Rename and add an index at the end (Do not check for identical files).
            /// </summary>
            AutoRename
        }
        #endregion

        #region [ Constants ]
        public static string FolderDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
        public static string FolderMyDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
        public static string FolderAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
        public static string FolderStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\";
        public static string FolderCurrent = GetCurrentPath();
        public static string FolderIECookie = Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "\\";
        public static string FolderTemp = Path.GetTempPath();

        /// <summary>
        /// One MB in bytes.
        /// </summary>
        public const long INT_OneMbInByte = 1048576;
        #endregion

        #region [ File & Directory Operation]
        /// <summary>
        /// Moves the directory contents.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="destinationDir">The destination dir.</param>
        /// <param name="conflictRule">The conflict rule.</param>
        /// <returns></returns>
        public static bool MoveDirectoryContents(DirectoryInfo sourceDir, string destinationDir, ConflictRules conflictRule)
        {
            try
            {
                foreach (DirectoryInfo dir in sourceDir.GetDirectories())
                {
                    if (!MoveDirectory(dir, destinationDir, conflictRule))
                    {
                        return false;
                    }
                }

                foreach (FileInfo file in sourceDir.GetFiles())
                {
                    if (!MoveFile(file, destinationDir, conflictRule))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the dir (dir=D:\KO\ToldExt.txt,dest=D:\Big  ==>  D:\Big\ToldExt.txt).
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="destinationDir">The destination dir.</param>
        /// <param name="conflictRule">The conflict rule.</param>
        /// <returns></returns>
        public static bool MoveFile(FileInfo file, string destinationDir, ConflictRules conflictRule)
        {
            try
            {
                string targetPath = AddSlash(destinationDir) + file.Name;
                FileInfo targetFile = new FileInfo(targetPath);

                // When the folder name is the same as the dir name it will generate an error
                // That's why we need the second condition

                if (File.Exists(targetPath) || Directory.Exists(targetPath))
                {
                    if (conflictRule == ConflictRules.Ignore)
                    {
                        return false;
                    }

                    if (conflictRule == ConflictRules.Overwrite)
                    {
                        targetFile.Delete();
                    }

                    if (conflictRule == ConflictRules.OverwriteIfIdentical)
                    {
                        if (IsSameFile(file.FullName, targetFile.FullName))
                        {
                            targetFile.Delete();
                        }
                        else
                        {
                            conflictRule = ConflictRules.AutoRename;
                        }
                    }

                    if (conflictRule == ConflictRules.AutoRename)
                    {
                        int index = 0;
                        string pathWithNoExt = GetFilePathNoExt(targetPath);
                        string ext = targetFile.Extension;

                        targetPath = string.Format("{0} [{1}]{2}", pathWithNoExt, index, ext);
                        while (File.Exists(targetPath) && Directory.Exists(targetPath))
                        {
                            index++;
                            targetPath = string.Format("{0} [{1}]{2}", pathWithNoExt, index, ext);
                        }
                    }
                }

                file.MoveTo(targetPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the directory (dir=D:\KO,dest=D:\Big  ==>  D:\Big\KO).
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="destinationDir">The destination dir.</param>
        /// <param name="conflictRule">The conflict rule.</param>
        /// <returns></returns>
        public static bool MoveDirectory(DirectoryInfo dir, string destinationDir, ConflictRules conflictRule)
        {
            try
            {
                string targetPath = AddSlash(destinationDir) + dir.Name;
                DirectoryInfo targetDir = new DirectoryInfo(targetPath);

                if (Directory.Exists(targetPath))
                {
                    if (conflictRule == ConflictRules.Ignore)
                    {
                        return false;
                    }

                    if (conflictRule == ConflictRules.Overwrite || conflictRule == ConflictRules.OverwriteIfIdentical)
                    {
                        bool result = MoveDirectoryContents(dir, targetPath, conflictRule);
                        if (result && dir.GetFiles().Length + dir.GetDirectories().Length == 0)
                        {
                            dir.Delete();
                            return true;
                        }
                        return result;
                    }

                    if (conflictRule == ConflictRules.AutoRename)
                    {
                        // If the existed target path is the source, then it's okey to merge.
                        // Do not need to rename.
                        if (dir.FullName.ToLower().StartsWith(targetPath.ToLower()))
                        {
                            bool result = MoveDirectoryContents(dir, targetPath, conflictRule);
                            if (result && dir.GetFiles().Length + dir.GetDirectories().Length == 0)
                            {
                                dir.Delete();
                                return true;
                            }
                            return result;
                        }

                        int index = 0;
                        string destinationNoSlash = StripSlash(targetPath);

                        targetPath = string.Format("{0} [{1}]", destinationNoSlash, index);
                        while (Directory.Exists(targetPath))
                        {
                            index++;
                            targetPath = string.Format("{0} [{1}]", destinationNoSlash, index);
                        }
                    }
                }

                dir.MoveTo(targetPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The folder is empty.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns></returns>
        public static bool FolderIsEmpty(DirectoryInfo dir)
        {
            int folders = dir.GetDirectories().Length;
            int files = dir.GetFiles().Length;

            return folders + files == 0;
        }

        /// <summary>
        /// The folder is empty.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns></returns>
        public static bool FolderIsEmpty(string dir)
        {
            return FolderIsEmpty(new DirectoryInfo(dir));
        }
        #endregion

        #region [ Append To File ]
        /// <summary>
        /// Append text to a dir, if not exist it will create a new dir.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool AppendToFile(string path, string text)
        {
            try
            {
                FileStream fs;

                if (File.Exists(path))
                {
                    fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                }
                else
                {
                    fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                }
                StreamWriter sw = new StreamWriter(fs);

                sw.Write(text);

                sw.Close();
                fs.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region [ Get Files List ]
        /// <summary>
        /// Get the files in the a folder using the filter
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(string dir, string filter, bool recursive)
        {
            return GetFiles(new DirectoryInfo(dir), filter, recursive);
        }

        /// <summary>
        /// Get the files in the a folder using the filter
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(DirectoryInfo dir, string filter, bool recursive)
        {
            List<FileInfo> list = new List<FileInfo>();

            if (Str.IsEmpty(filter))
            {
                filter = "*.*";
            }

            if (!recursive)
            {
                list.AddRange(dir.GetFiles(filter));
                return list;
            }

            if (filter.Length > 0)
            {
                list.AddRange(dir.GetFiles(filter));
            }
            else
            {
                list.AddRange(dir.GetFiles());
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                list.AddRange(GetFiles(d, filter, true));
            }

            return list;
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <param name="exclusionList">The exclusion list.</param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(string dir, string filter, bool recursive, params string[] exclusionList)
        {
            if (!Directory.Exists(dir))
            {
                return new List<FileInfo>();
            }
            return GetFiles(new DirectoryInfo(dir), filter, recursive, exclusionList);
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <param name="exclusionList">The exclusion list.</param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(DirectoryInfo dir, string filter, bool recursive, params string[] exclusionList)
        {
            //List<FileInfo> list = GetFiles(dir, filter, recursive);
            //List<FileInfo> found = new List<FileInfo>();

            //foreach (FileInfo f in list)
            //{
            //    if (!MagicU.ArraySubContainsAny(new string[] {f.Name}, exclusionList))
            //    {
            //        found.Add(f);
            //    }
            //}
            //return found;

            return null;
        }

        /// <summary>
        /// Get any dir that smaller than the limit.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="maxSize">Return any dir has a smaller size than this limit</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesBySmallerSize(DirectoryInfo dir, long maxSize, string filter, bool recursive)
        {
            List<FileInfo> list = GetFiles(dir, filter, recursive);
            List<FileInfo> found = new List<FileInfo>();

            foreach (FileInfo f in list)
            {
                if (f.Length < maxSize)
                {
                    found.Add(f);
                }
            }

            return found;
        }

        /// <summary>
        /// Get any dir that bigger than the limit.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="minSize">Minimum limit.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByBiggerSize(DirectoryInfo dir, long minSize, string filter, bool recursive)
        {
            List<FileInfo> list = GetFiles(dir, filter, recursive);
            List<FileInfo> found = new List<FileInfo>();

            foreach (FileInfo f in list)
            {
                if (f.Length > minSize)
                {
                    found.Add(f);
                }
            }

            return found;
        }

        /// <summary>
        /// Get all the files with LastWriteTime greater than the current minus the TimeSpan limit
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="maxCount">The max count.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByDate(DirectoryInfo dir, TimeSpan limit, int maxCount, string filter, bool recursive)
        {
            DateTime limitDate = DateTime.Now.Subtract(limit);
            return GetFilesByDate(dir, limitDate, maxCount, filter, recursive);
        }

        /// <summary>
        /// Get all the files that are NEWER than the limit date
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="maxCount">The max count.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByDate(DirectoryInfo dir, DateTime limit, int maxCount, string filter, bool recursive)
        {
            List<FileInfo> list = GetFiles(dir, filter, recursive);
            List<FileInfo> found = new List<FileInfo>();

            foreach (FileInfo f in list)
            {
                if (f.LastWriteTime.CompareTo(limit) > 0)
                {
                    found.Add(f);

                    if (found.Count + 1 > maxCount)
                    {
                        break;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Get all the files with LastWriteTime less than than the current minus the TimeSpan limit
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByOlderDate(DirectoryInfo dir, TimeSpan limit, string filter, bool recursive)
        {
            DateTime limitDate = DateTime.Now.Subtract(limit);
            return GetFilesByOlderDate(dir, limitDate, filter, recursive);
        }

        /// <summary>
        /// Get all the files that are OLDER than the limit date
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesByOlderDate(DirectoryInfo dir, DateTime limit, string filter, bool recursive)
        {
            List<FileInfo> list = GetFiles(dir, filter, recursive);
            List<FileInfo> found = new List<FileInfo>();

            foreach (FileInfo f in list)
            {
                if (f.LastWriteTime.CompareTo(limit) < 0)
                {
                    found.Add(f);
                }
            }

            return found;
        }

        /// <summary>
        /// Delete any dir that older then the current time minus the TimeSpan
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static int DeleteFilesByOlderDate(DirectoryInfo dir, TimeSpan limit, string filter, bool recursive)
        {
            DateTime limitDate = DateTime.Now.Subtract(limit);
            return DeleteFilesByOlderDate(dir, limitDate, filter, recursive);
        }

        /// <summary>
        /// Delete any dir that older then the date limit
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public static int DeleteFilesByOlderDate(DirectoryInfo dir, DateTime limit, string filter, bool recursive)
        {
            List<FileInfo> list = GetFilesByOlderDate(dir, limit, filter, recursive);
            int count = 0;

            try
            {
                foreach (FileInfo f in list)
                {
                    f.Delete();
                    count++;
                }

                return count;
            }
            catch
            {
                // Error
                return -1;
            }
        }
        #endregion

        #region [ Image Stream ]
        /// <summary>
        /// Similar to Image.FromFile(""), but this one release the dir lock
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns></returns>
        public static Image MakeImageFromFile(string fullpath)
        {
            try
            {
                Image img;
                using (Stream s = new FileStream(fullpath, FileMode.Open))
                {
                    try
                    {
                        img = Image.FromStream(s);
                    }
                    catch (Exception)
                    {
                        img = null;
                    }

                    s.Close();
                }
                return img;
            }
            catch (Exception xpt)
            {
                return null;
            }
        }

        /// <summary>
        /// Makes the image from a dir.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns></returns>
        public static Image MakeImageFromFile(FileInfo fullpath)
        {
            return MakeImageFromFile(fullpath.FullName);
        }
        #endregion

        #region [ Path Extraction/Modification ]

        /// <summary>
        /// Create all directory in the path.
        /// </summary>
        /// <param name="filepath">Full dir path, must have filename.</param>
        /// <returns>false if failed, otherwise return true</returns>
        public static bool ConstructFilepath(string filepath)
        {
            // if not a full path
            if (!filepath.Contains(":"))
            {
                filepath = FolderCurrent + filepath;
            }
            filepath = GetPath(filepath);
            if (Directory.Exists(filepath))
            {
                return true;
            }
            return ConstructPath(filepath);
        }

        /// <summary>
        /// Create all directory in the path.
        /// </summary>
        /// <param name="folderPath">Path must NOT contains the filename.</param>
        /// <returns>false if failed, otherwise return true</returns>
        public static bool ConstructPath(string folderPath)
        {
            try
            {
                folderPath = StripSlash(folderPath);

                string existed_dir = PathFinder(folderPath);
                if (StripSlash(existed_dir) == folderPath)
                {
                    return true;
                }
                string[] remainding_path = folderPath.Substring(existed_dir.Length).Split('\\');
                string extending_path = "";
                foreach (string dir in remainding_path)
                {
                    extending_path += dir + "\\";
                    Directory.CreateDirectory(existed_dir + extending_path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get filename from a dir path (C:\File.avi =&gt; File.avi)
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns></returns>
        static public string GetFilename(string fullpath)
        {
            FileInfo file = new FileInfo(fullpath);
            return file.Name;
        }

        /// <summary>
        /// Extention with the dot (C:\File.avi =&gt; .avi)
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns></returns>
        static public string GetExtension(string fullpath)
        {
            FileInfo file = new FileInfo(fullpath);
            return file.Extension;
        }

        /// <summary>
        /// Filepath, without the extension (C:\File.avi => C:\File)
        /// </summary>
        /// <param name="fullpath">C:\File.avi</param>
        /// <returns>C:\File</returns>
        static public string GetFilePathNoExt(string fullpath)
        {
            return fullpath.Substring(0, fullpath.LastIndexOf("."));
        }

        /// <summary>
        /// Return the executable folder path with ending slashes.
        /// </summary>
        /// <param name="withSlashes">if set to <c>true</c> [with_slashes].</param>
        /// <returns></returns>
        static public string GetCurrentPath(bool withSlashes)
        {
            string path = GetCurrentPath();

            if (!withSlashes)
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        }

        /// <summary>
        /// Return the executable FOLDER path with ending slashes.
        /// </summary>
        /// <returns></returns>
        static public string GetCurrentPath()
        {
            return GetPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Assign the specified path or the upper path if the original path is not found.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="browser">The browser.</param>
        public static void PathFinder(string path, ref FolderBrowserDialog browser)
        {
            browser.SelectedPath = PathFinder(path);
        }

        /// <summary>
        /// Return the specified path or the upper path if the original path is not found.
        /// </summary>
        /// <param name="path">A win32 folder fullpath. Must NOT contains filename.</param>
        /// <returns>The specified path or the upper path if the original path is not found. End slash is included</returns>
        public static string PathFinder(string path)
        {
            path = StripSlash(path);
            string[] parts = path.Split('\\');
            string finder_path;

            if (parts.Length == 1)
            {
                return AddSlash(path);
            }
            finder_path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (Directory.Exists(finder_path + "\\" + parts[i]))
                {
                    finder_path += "\\" + parts[i];
                }
                else
                {
                    break;
                }
            }
            if (finder_path.Length == 2)
            {
                return AddSlash(finder_path);
            }
            return Directory.Exists(finder_path) ? AddSlash(finder_path) : "";
        }

        /// <summary>
        /// Break a path into different parts
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns>
        /// Array of string, use ExtractPathIndex as an index. 0: fullpath, 1: path without filename without endded slash, 2: like 1 but with slash, 3: filename, 4: filename without extension, 5: extension without the dot
        /// </returns>
        public static string[] ExtractPath(string fullpath)
        {
            string[] data = new string[7];
            FileInfo file = new FileInfo(fullpath);


            //Fullpath, 
            data[0] = file.FullName;
            //NoNameNoSlash, 
            data[1] = Path.GetDirectoryName(fullpath);
            //NoNameWithSlash, 
            data[2] = data[1] + "\\";
            //Filename, 
            data[3] = file.Name;
            //FilenameNoExt, 
            data[4] = Path.GetFileNameWithoutExtension(fullpath);
            //Extension, 
            data[5] = file.Extension;
            //ExtensionNoDot
            data[6] = Str.SubString(file.Extension, 1);

            return data;
        }

        /// <summary>
        /// Get folder name with no ended slash, "D:\Home" => "Home"
        /// </summary>
        /// <param name="folderPath">Full folder path or a dir path</param>
        /// <returns>Foldername</returns>
        public static string GetFolderName(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                folderPath = folderPath.TrimEnd('\\');
                string folderName = folderPath.Substring(folderPath.LastIndexOf("\\") + 1);
                return folderName.TrimEnd('\\');
            }
            if (File.Exists(folderPath))
            {
                return GetFolderName(GetPath(folderPath));
            }
            return "";
        }

        /// <summary>
        /// Get full parent folder path with ended slash, "D:\Home\Desktop" => "D:\Home\"
        /// </summary>
        /// <param name="folderPath">Full folder path or a dir path</param>
        /// <returns>Parent path</returns>
        public static string GetParentFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                folderPath = folderPath.TrimEnd('\\');
                return folderPath.Substring(0, folderPath.LastIndexOf("\\") + 1);
            }
            if (File.Exists(folderPath))
            {
                return GetParentFolder(GetPath(folderPath));
            }
            return "";
        }

        /// <summary>
        /// Removed the ended slash from the folder path, "D:\Home\" =&gt; "D:\Home"
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public static string StripSlash(string folderPath)
        {
            return folderPath.TrimEnd('\\');
        }

        /// <summary>
        /// Append text to file name and still keep the extension
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="appendText">The append text.</param>
        /// <returns></returns>
        public static string AppendFilename(string filepath, string appendText)
        {
            return AppendFilename(filepath, appendText, true);
        }

        /// <summary>
        /// Append text to file name and still keep the extension
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="appendText">The append text.</param>
        /// <param name="addToFront">if set to <c>true</c> [add to front].</param>
        /// <returns></returns>
        public static string AppendFilename(string filepath, string appendText, bool addToFront)
        {
            ConstructFilepath(filepath);
            string[] path = ExtractPath(filepath);
            if (addToFront)
            {
                return path[(int)ExtractPathIndex.NoNameWithSlash] + appendText + path[(int)ExtractPathIndex.Filename];
            }
            return path[(int)ExtractPathIndex.NoNameWithSlash] + path[(int)ExtractPathIndex.FilenameNoExt] + appendText + path[(int)ExtractPathIndex.Extension];
        }
        #endregion

        #region [ Advanced IO Functions: Recycle Del ]
        /// <summary>
        /// Move the specified dir to The Window Recycle Bin.
        /// </summary>
        /// <param name="path">Fullpath to the dir you want to move.</param>
        public static void MoveToRecycleBin(FileInfo path)
        {
            MoveToRecycleBin(path.FullName);
        }

        /// <summary>
        /// Move the specified dir to The Window Recycle Bin.
        /// </summary>
        /// <param name="path">Fullpath to the dir you want to move.</param>
        public static void MoveToRecycleBin(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            InteropSHFileOperation fo = new InteropSHFileOperation();
            fo.wFunc = InteropSHFileOperation.FO_Func.FO_DELETE;
            fo.fFlags.FOF_ALLOWUNDO = true;
            fo.pFrom = path;
            fo.Execute();
        }
        #endregion

        #region [ Special IO Functions: ExecSilence, FilenameEncoding ]
        /// <summary>
        /// Execute a command using dos (without the window)
        /// </summary>
        /// <param name="cmd">The command (ie unrar.exe)</param>
        /// <param name="arguments">The arguments that follows the command</param>
        /// <returns>String output, if there any</returns>
        public static string ExecSilence(string cmd, string arguments = "")
        {
            cmd = cmd.Trim(' ', '"');
            if (cmd.Contains(" "))
            {
                cmd = "\"" + cmd + "\"";
            }

            Process process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = arguments;
            process.Start();

            process.WaitForExit();

            string result = "";
            string output = process.StandardOutput.ReadToEnd();
            string error = "\r\n" + process.StandardError.ReadToEnd();

            result = string.Format("[COMMAND:] {0}\r\n[ARGS:] {1}\r\n\r\n", cmd, arguments); 

            if (Str.IsNotEmpty(output))
            {
                result += string.Format("[OUTPUT:] {0}\r\n", output);
            }

            if (Str.IsNotEmpty(error))
            {
                result += string.Format("[ERROR:] {0}\r\n", error);
            }
            return result;
        }        

        /// <summary>
        /// Remove invalid characters in the filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FilenameEncoding(string filename)
        {
            filename = Regex.Replace(filename, @"[/\\:*?<>|]", "-");
            filename = filename.Replace('"', '\'');
            return filename;
        }
        #endregion

        #region [ Advanced Functions: Drive Space, Sync ]
        /// <summary>
        /// Determines whether the two files are the same dir (using FileSize and LastWriteTime to compare).
        /// </summary>
        /// <param name="firstFile">The first dir.</param>
        /// <param name="secondFile">The second dir.</param>
        /// <returns>
        /// 	<c>true</c> if the two files are the same; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSameFile(string firstFile, string secondFile)
        {
            if (!File.Exists(firstFile) || !File.Exists(secondFile))
            {
                return false;
            }

            FileInfo ifile_a = new FileInfo(firstFile);
            FileInfo ifile_b = new FileInfo(secondFile);

            if (ifile_a.Length != ifile_b.Length)
            {
                return false;
            }

            if (ifile_a.LastWriteTime != ifile_b.LastWriteTime)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get Drive Total Space.
        /// </summary>
        /// <param name="driveLetter">The drive letter.</param>
        /// <returns></returns>
        public static long DriveTotalspace(string driveLetter)
        {
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + driveLetter[0] + ":\"");
            disk.Get();
            return Convert.ToInt64(disk["Size"]);
        }

        /// <summary>
        /// Get Hard Dish Free Drive Space
        /// </summary>
        /// <param name="driveLetter">The drive letter.</param>
        /// <returns></returns>
        public static long DriveFreespace(string driveLetter)
        {
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + driveLetter[0] + ":\"");
            disk.Get();
            return Convert.ToInt64(disk["FreeSpace"]);
        }

        /// <summary>
        /// Synchronize two folders (one way sync)
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="desti">The destination.</param>
        /// <returns></returns>
        public static int SyncDir(string src, string desti)
        {
            if (Directory.Exists(src))
            {
                DirectoryInfo isrc = new DirectoryInfo(src);
                return SyncDir(isrc, AddSlash(desti) + isrc.Name);
            }
            return 0;
        }

        /// <summary>
        /// Synchronize two folders (one way sync)
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="desti">The destination.</param>
        /// <returns></returns>
        private static int SyncDir(DirectoryInfo src, string desti)
        {
            int changed_files = 0;

            desti = AddSlash(desti);
            ConstructPath(desti);
            string desti_subfolder = "";
            foreach (DirectoryInfo dir in src.GetDirectories())
            {
                desti_subfolder = desti + dir.Name;
                changed_files += SyncDir(dir, desti_subfolder);
            }

            string desti_filepath = "";
            foreach (FileInfo file in src.GetFiles())
            {
                desti_filepath = desti + file.Name;

                if (!IsSameFile(file.FullName, desti_filepath))
                {
                    try
                    {
                        File.Copy(file.FullName, desti_filepath, true);
                    }
                    catch { }
                    changed_files++;
                }
            }

            return changed_files;
        }
        #endregion

        #region [ Processes ]
        /// <summary>
        /// Process function mapping
        /// </summary>
        /// <returns></returns>
        public static List<Process> GetProcesses()
        {
            List<Process> list = new List<Process>(Process.GetProcesses());
            return list;
        }
        #endregion

        #region [ Clipboard Functions ]
        /// <summary>
        /// Send the data to the clipboard.
        /// </summary>
        /// <param name="text">The data.</param>
        public static void ClipboardSet(string text)
        {
            Clipboard.SetText(text);
        }

        /// <summary>
        /// Read the data in the clipboard.
        /// </summary>
        /// <returns></returns>
        public static string ClipboardGet()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData != null && iData.GetDataPresent(DataFormats.Text))
            {
                string data = (String)iData.GetData(DataFormats.Text);
                return data;
            }
            return "";
        }

        /// <summary>
        /// Get the text from the clipboard, trim it and put it back.
        /// </summary>
        /// <returns>The text that has been trimmed.</returns>
        public static string ClipboardTrim()
        {
            string data = ClipboardGet();
            if (data.Length > 0)
            {
                data = data.Trim();
                ClipboardSet(data);
                return data;
            }
            return "";
        }
        #endregion

        #region [ Serialize SortedList ]
        /// <summary>
        /// Serialize the sortedlist to a dir.
        /// </summary>
        /// <param name="sortedList">The sorted list.</param>
        /// <param name="targetPath">The target path.</param>
        public static void SortedListSerialize(SortedList sortedList, string targetPath)
        {
            try
            {
                var bf = new BinaryFormatter();
                using (FileStream fs = File.OpenWrite(targetPath))
                {
                    bf.Serialize(fs, sortedList);
                }
            }
            catch (Exception xpt)
            {
            }
        }

        /// <summary>
        /// Load the SortedList with serialized data.
        /// </summary>
        /// <param name="sortedList">The sorted list.</param>
        /// <param name="sourcePath">The source path.</param>
        public static void SortedListDeserialize(out SortedList sortedList, string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                sortedList = new SortedList();
                return;
            }

            try
            {
                var bf = new BinaryFormatter();
                using (FileStream fs = File.OpenRead(sourcePath))
                {
                    sortedList = (SortedList)bf.Deserialize(fs);
                }
            }
            catch (Exception xpt)
            {
                sortedList = new SortedList();
            }
        }
        #endregion

        #region [ JPortion ]
                

        /// <summary>
        /// Writes to a text dir on the desktop.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        public static void WriteToDesktop(string filename, string content, bool append)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string logPath = path + "\\" + filename;

                if (append)
                {
                    File.AppendAllText(logPath, content);
                }
                else
                {
                    File.WriteAllText(logPath, content);
                }
            }
            catch { }
        }

        /// <summary>
        /// Forces Windows to shutdown.
        /// </summary>
        public static void ForceShutdown()
        {
            try
            {
                // C:\WINDOWS\system32\shutdown.exe -s -f
                Process p = new Process();
                p.StartInfo.FileName = AddSlash(Environment.SystemDirectory) + "shutdown.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = " -s -f -t 0";
                p.Start();
            }
            catch (Exception xpt)
            {
                //WriteToDesktop("Shutdown Error.txt", xpt.Message, true);
            }
        }
        #endregion

        /// <summary>
        /// Cleans the name of the dir, remove all invalid characters.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string CleanFileName(string name)
        {
            name = name.Replace(":", " - ");

            char[] chr = Path.GetInvalidFileNameChars();
            foreach (char c in chr)
            {
                name = name.Replace(c, ' ');
            }
            name = Regex.Replace(name, @"\s+", " ").Trim('-', ' ');
            return name;
        }

        /// <summary>
        /// Determines whether the specified path is file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if the specified path is file; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFile(string path)
        {
            if (Directory.Exists(path))
            {
                return false;
            }

            return File.Exists(path);
        }

        /// <summary>
        /// Determines whether the specified path is folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if the specified path is folder; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFolder(string path)
        {
            return Directory.Exists(path);
        }
    }



    /// <summary>
    /// ExtractPathIndex
    /// </summary>
    public enum ExtractPathIndex : int
    { Fullpath, NoNameNoSlash, NoNameWithSlash, Filename, FilenameNoExt, Extension, ExtensionNoDot };

    #region [ Delete to Recycle Bin Function ]
    /// <summary>
    /// Use conjunction with "MoveToRecycleBin" function.
    /// </summary>
    public class InteropSHFileOperation
    {
#pragma warning disable 1591
        public enum FO_Func : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FO_Func wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;

        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        private SHFILEOPSTRUCT _ShFile;
        public FILEOP_FLAGS fFlags;

        public IntPtr hwnd
        {
            set
            {
                this._ShFile.hwnd = value;
            }
        }
        public FO_Func wFunc
        {
            set
            {
                this._ShFile.wFunc = value;
            }
        }

        public string pFrom
        {
            set
            {
                this._ShFile.pFrom = value + '\0' + '\0';
            }
        }
        public string pTo
        {
            set
            {
                this._ShFile.pTo = value + '\0' + '\0';
            }
        }

        public bool fAnyOperationsAborted
        {
            set
            {
                this._ShFile.fAnyOperationsAborted = value;
            }
        }
        public IntPtr hNameMappings
        {
            set
            {
                this._ShFile.hNameMappings = value;
            }
        }
        public string lpszProgressTitle
        {
            set
            {
                this._ShFile.lpszProgressTitle = value + '\0';
            }
        }

        public InteropSHFileOperation()
        {
            this.fFlags = new FILEOP_FLAGS();
            this._ShFile = new SHFILEOPSTRUCT();
            this._ShFile.hwnd = IntPtr.Zero;
            this._ShFile.wFunc = FO_Func.FO_COPY;
            this._ShFile.pFrom = "";
            this._ShFile.pTo = "";
            this._ShFile.fAnyOperationsAborted = false;
            this._ShFile.hNameMappings = IntPtr.Zero;
            this._ShFile.lpszProgressTitle = "";
        }

        public bool Execute()
        {
            this._ShFile.fFlags = this.fFlags.Flag;
            int ReturnValue = SHFileOperation(ref this._ShFile);
            if (ReturnValue == 0)
            {
                return true;
            }
            return false;
        }

        public class FILEOP_FLAGS
        {
            [Flags]
            private enum FILEOP_FLAGS_ENUM : ushort
            {
                FOF_MULTIDESTFILES = 0x0001,
                FOF_CONFIRMMOUSE = 0x0002,
                FOF_SILENT = 0x0004,  // don't create progress/report
                FOF_RENAMEONCOLLISION = 0x0008,
                FOF_NOCONFIRMATION = 0x0010,  // Don't prompt the user.
                FOF_WANTMAPPINGHANDLE = 0x0020,  // Fill in SHFILEOPSTRUCT.hNameMappings
                // Must be freed using SHFreeNameMappings
                FOF_ALLOWUNDO = 0x0040,
                FOF_FILESONLY = 0x0080,  // on *.*, do only files
                FOF_SIMPLEPROGRESS = 0x0100,  // means don't show names of files
                FOF_NOCONFIRMMKDIR = 0x0200,  // don't confirm making any needed dirs
                FOF_NOERRORUI = 0x0400,  // don't put up error UI
                FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT dir Security Attributes
                FOF_NORECURSION = 0x1000,  // don't recurse into directories.
                FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
                FOF_WANTNUKEWARNING = 0x4000,  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
                FOF_NORECURSEREPARSE = 0x8000,  // treat reparse points as objects, not containers
            }

            public bool FOF_MULTIDESTFILES = false;
            public bool FOF_CONFIRMMOUSE = false;
            public bool FOF_SILENT = false;
            public bool FOF_RENAMEONCOLLISION = false;
            public bool FOF_NOCONFIRMATION = false;
            public bool FOF_WANTMAPPINGHANDLE = false;
            public bool FOF_ALLOWUNDO = false;
            public bool FOF_FILESONLY = false;
            public bool FOF_SIMPLEPROGRESS = false;
            public bool FOF_NOCONFIRMMKDIR = false;
            public bool FOF_NOERRORUI = false;
            public bool FOF_NOCOPYSECURITYATTRIBS = false;
            public bool FOF_NORECURSION = false;
            public bool FOF_NO_CONNECTED_ELEMENTS = false;
            public bool FOF_WANTNUKEWARNING = false;
            public bool FOF_NORECURSEREPARSE = false;
            public ushort Flag
            {
                get
                {
                    ushort ReturnValue = 0;

                    if (this.FOF_MULTIDESTFILES)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_MULTIDESTFILES;
                    if (this.FOF_CONFIRMMOUSE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_CONFIRMMOUSE;
                    if (this.FOF_SILENT)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_SILENT;
                    if (this.FOF_RENAMEONCOLLISION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_RENAMEONCOLLISION;
                    if (this.FOF_NOCONFIRMATION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCONFIRMATION;
                    if (this.FOF_WANTMAPPINGHANDLE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_WANTMAPPINGHANDLE;
                    if (this.FOF_ALLOWUNDO)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_ALLOWUNDO;
                    if (this.FOF_FILESONLY)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_FILESONLY;
                    if (this.FOF_SIMPLEPROGRESS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_SIMPLEPROGRESS;
                    if (this.FOF_NOCONFIRMMKDIR)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCONFIRMMKDIR;
                    if (this.FOF_NOERRORUI)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOERRORUI;
                    if (this.FOF_NOCOPYSECURITYATTRIBS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCOPYSECURITYATTRIBS;
                    if (this.FOF_NORECURSION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NORECURSION;
                    if (this.FOF_NO_CONNECTED_ELEMENTS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NO_CONNECTED_ELEMENTS;
                    if (this.FOF_WANTNUKEWARNING)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_WANTNUKEWARNING;
                    if (this.FOF_NORECURSEREPARSE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NORECURSEREPARSE;

                    return ReturnValue;
                }
            }
        }
#pragma warning restore 1591
    }
    #endregion

    #region [ File Sort Comparer ]
    /// <summary>
    /// FileInfo sort order.
    /// </summary>
    public enum FileInfoOrder
    {
        /// <summary>
        /// 
        /// </summary>
        SortByFilename,
        /// <summary>
        /// 
        /// </summary>
        SortByExtension,
        /// <summary>
        /// 
        /// </summary>
        SortBySize,
        /// <summary>
        /// 
        /// </summary>
        SortByModifiedDate
    }

    // Usage: Array.sort(array, new FileComparer())
    /// <summary>
    /// FileInfo comparer.
    /// </summary>
    public class FileInfoComparer : IComparer
    {
        /// <summary>
        /// The sort order
        /// </summary>
        public FileInfoOrder SortOrder = FileInfoOrder.SortByFilename;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoComparer"/> class.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        public FileInfoComparer(FileInfoOrder sortOrder)
        {
            SortOrder = sortOrder;
        }

        /// <summary>
        /// Compares the specified obj1.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns></returns>
        public int Compare(object obj1, object obj2)
        {
            FileInfo a = (FileInfo)obj1;
            FileInfo b = (FileInfo)obj2;

            if (SortOrder == FileInfoOrder.SortByFilename)
            {
                return a.Name.CompareTo(b.Name);
            }
            if (SortOrder == FileInfoOrder.SortBySize)
            {
                return a.Length.CompareTo(b.Length);
            }
            if (SortOrder == FileInfoOrder.SortByModifiedDate)
            {
                return a.LastWriteTime.CompareTo(b.LastWriteTime);
            }

            return a.FullName.CompareTo(b.FullName);
        }
    }
    #endregion
}
