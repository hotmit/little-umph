using System;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using System.Security.AccessControl;
using System.Security.Principal;
using System.Security;

namespace LittleUmph
{
    /// <summary>
    /// Save and retrieve data from the registry under the CurrentUser hive.
    /// </summary>
	public class RegWriter {

        public enum RegistryHive { CurrentUserSoftware, CurrentUser, LocalMachineSoftware, LocalMachine, ClassesRoot, Users, CurrentConfig };

		/// <summary>
		/// Contains the registry Path.
		/// </summary>
		private string _path;
		
		/// <summary>
		/// RegWriter constructor.
		/// </summary>
        /// <param name="path">The Path of registry in the CurrentUser hive.
		/// <example>
		///     <code>RegWriter reg = new RegWriter("Software\\BitWise\\Settings");</code>
		/// </example>
		/// </param>
		public RegWriter(String path) {
			_path = path;	
		}

		/// <summary>
		/// Get or set registry Path.
		/// </summary>
		public string Path {
			get {
				return _path;
			}
			set {
				_path = value;
			}
		}

		#region [ Write Registry ]

		/// <summary>
		/// Write a string to the registry.
		/// </summary>
		/// <param name="name">Name of the string value in the registry.</param>
		/// <param name="data">The value</param>
		public void Write (string name, string data)
		{
		    RegistryKey key = Registry.CurrentUser.CreateSubKey(_path);
		    if (key != null)
		    {
		        key.SetValue(name, data);
		    }
		}

	    /// <summary>
		/// Write a integer to the registry.
		/// </summary>
		/// <param name="name">Name of the string value in the registry.</param>
		/// <param name="data">The value</param>
		public void Write (string name, int data){
			Write(name, data.ToString());
		}		

		/// <summary>
		/// Write a boolean to the registry.
		/// </summary>
		/// <param name="name">Name of the string value in the registry.</param>
		/// <param name="data">The value</param>
		public void Write (string name, bool data){
			Write(name, data.ToString());
		}

		#endregion

		#region [ Read Registry ]

		/// <summary>
		/// Read the string value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <returns>Return the value of the 'string value' or an zero length string if an error occured.</returns>
		public string ReadString (string name){
			return ReadString(name, "");
		}

		/// <summary>
		/// Read the string value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <param name="defaultOnError">The default value to return if an error occured.</param>
		/// <returns>Return the value of the 'string value' or the value of 'defaultOnError' variable if an error occured.</returns>
		public string ReadString (string name, string defaultOnError){
			RegistryKey key = Registry.CurrentUser.OpenSubKey(_path);

			if (key != null){
				Object val = key.GetValue(name);
				string result = (val == null) ? null : val.ToString();

				if (result == null){
					return defaultOnError;
				}
			    return result;
			}
		    return defaultOnError;
		}

		/// <summary>
		/// Read the integer value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <returns>Return the value of the 'string value' or return 0 if an error occured.</returns>
		public int ReadInteger (string name){
			return ReadInteger(name, 0);
		}
		
		/// <summary>
		/// Read the interger value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <param name="defaultOnError">The default value to return if an error occured.</param>
		/// <returns>Return the value of the 'string value' or the value of 'defaultOnError' variable if an error occured.</returns>
		public int ReadInteger (string name, int defaultOnError){
			string result = ReadString(name);

			if (result.Length == 0){
				return defaultOnError;
			}
            
            try {
		        return Convert.ToInt32(result);
		    }
		    catch {
		        return defaultOnError;
		    }
		}		

		/// <summary>
		/// Read the decimal value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <returns>Return the value of the 'string value' or return 0 if an error occured.</returns>
		public decimal ReadDecimal (string name){
			return ReadDecimal(name, 0);
		}
		
		/// <summary>
		/// Read the decimal value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <param name="defaultOnError">The default value to return if an error occured.</param>
		/// <returns>Return the value of the 'string value' or the value of 'defaultOnError' variable if an error occured.</returns>
		public decimal ReadDecimal (string name, decimal defaultOnError){
			string result = ReadString(name);

			if (result.Length == 0){
				return defaultOnError;
			}
		
            try {
		        return Convert.ToDecimal(result);
		    }
		    catch {
		        return defaultOnError;
		    }
		}	

		/// <summary>
		/// Read the double value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <returns>Return the value of the 'string value' or return 0 if an error occured.</returns>
		public double ReadDouble (string name){
			return ReadDouble(name, 0);
		}
		
		/// <summary>
		/// Read the double value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <param name="defaultOnError">The default value to return if an error occured.</param>
		/// <returns>Return the value of the 'string value' or the value of 'defaultOnError' variable if an error occured.</returns>
		public double ReadDouble (string name, double defaultOnError){
			string result = ReadString(name);

			if (result.Length == 0){
				return defaultOnError;
			}
		
            try {
		        return Convert.ToDouble(result);
		    }
		    catch {
		        return defaultOnError;
		    }
		}
		
		/// <summary>
		/// Read the boolean value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <returns>Return the boolean value of the 'string value', or return false if an error occured.</returns>
		public bool ReadBoolean (string name){
			return ReadBoolean(name, false);
		}
		
		/// <summary>
		/// Read the boolean value of the 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'.</param>
		/// <param name="defaultOnError">The default value to return if an error occured.</param>
		/// <returns>Return the boolean value of the 'string value', or the value of 'defaultOnError' variable if an error occured.</returns>
		public bool ReadBoolean (string name, bool defaultOnError){
			string result = ReadString(name);

			if (result.Length == 0){
				return defaultOnError;
			}
		
            try {
		        return Convert.ToBoolean(result);
		    }
		    catch {
		        return defaultOnError;
		    }
		}

		#endregion

		#region [ Remove Registry ]
		/// <summary>
		/// Remove a 'string value' in the registry.
		/// </summary>
		/// <param name="name">The name of the 'string value'</param>
		public bool Remove (string name){
			return Remove(_path, name);
		}

		/// <summary>
		/// Remove a 'string value' in the registry.
		/// <example><code>Remove("Software\\BitWise\\Settings", "TopMost");</code></example>
		/// </summary>
		/// <param name="path">The Path within the CurrentUser registry hive.</param>
		/// <param name="name">The name of the 'string value'.</param>
		public static bool Remove (string path, string name){
			RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);

			if (key != null){
				try {
					key.DeleteValue(name);
					return true;
				}
				catch {
					return false;
				}
			}
		    return true;
		}

		/// <summary>
		/// Remove a key in the registry under the CurrentUser hive.
		/// <example>Example delete "BitWise" key:
		/// <code>RemoveKey("Software\\", "BitWise");</code></example>
		/// </summary>
		/// <param name="path">The Path to the key in the CurrentUser hive.</param>		
		/// <param name="key">The name of the key.</param>
		public static bool RemoveKey (string path, string key){		
			RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path, true);
			
			if (subkey != null){
				try {					
					subkey.DeleteSubKeyTree(key);
					return true;
				}
				catch {
					return false;
				}
			}
		    return true;
		}
		#endregion

		#region [ Save and Load Winform's Location ]
		/// <summary>
		/// Save the form's location to the registry.
		/// </summary>
		/// <param name="winform">The reference to the form.</param>		
		public void SaveFormLocation (Form winform){
			SaveFormLocation(winform.Location);
		}

		/// <summary>
		/// Save the form's location to the registry.
		/// <example>Example: <code>RegWriter reg = new RegWriter("Software\\BitWise\\Settings");
		/// reg.SaveFormLocation(Location);</code></example>
		/// </summary>
		/// <param name="savedX">X coordinate of the form.</param>
		/// <param name="savedY">Y coordinate of the form.</param>		
		public void SaveFormLocation (int x, int y){
			SaveFormLocation(new Point(x, y));
		}

		/// <summary>
		/// Save the form's location to the registry.
		/// <example>Example: <code>RegWriter reg = new RegWriter("Software\\BitWise\\Settings");
		/// reg.SaveFormLocation(Location);</code></example>
		/// </summary>
		/// <param name="location">The location of the form.</param>		
		public void SaveFormLocation (Point location){
			int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
			int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

			if ((location.X > 0) && (location.Y > 0) && (location.X + 40 < screenWidth) && (location.Y + 40 < screenHeight)){
				Write("X", location.X);
				Write("Y", location.Y);			
			}
		}		

		/// <summary>
		/// Move the form to the saved location, or move to the center if error occured.
		/// </summary>
		/// <param name="winform">The reference of the form.</param>
		public void LoadFormLocation (Form winform){
			int x = ReadInteger("X", -1);
			int y = ReadInteger("Y", -1);

			int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
			int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

		    // If less than zero or max length minus 40px then reset location
			if ((x < 0) || (y < 0) || (x + 40 > screenWidth) || (y + 40 > screenHeight)){
				winform.StartPosition = FormStartPosition.CenterScreen;
			}
			else {
				winform.StartPosition = FormStartPosition.Manual;
				winform.Location = new Point(x, y);
			}
		}				
		#endregion

		#region [ Save and Load Winform's Size ]
		/// <summary>
		/// Save winform's size
		/// </summary>
		/// <param name="winform"></param>
		public void SaveFormSize (Form winform){		
			if ((winform.Width > 160) && (winform.Height > 24)){
				Write("FormWidth", winform.Width.ToString());
				Write("FormHeight", winform.Height.ToString());
			}
		}

		/// <summary>
		/// Load winform's size
		/// </summary>
		/// <param name="winform"></param>
		public void LoadFormSize (Form winform){			
			winform.Width = ReadInteger("FormWidth", winform.Width);
			winform.Height = ReadInteger("FormHeight", winform.Height);			
		}
		#endregion

		#region [ Save and Load Winform's Size and Location ]
		/// <summary>
		/// Save winform's size and location
		/// </summary>
		/// <param name="winform">The reference of the form.</param>
		public void SaveFormSizeAndLocation (Form winform){
			SaveFormSize(winform);
			SaveFormLocation(winform);
		}

		/// <summary>
		/// Load winform's size and location
		/// </summary>
		/// <param name="winform"></param>
		public void LoadFormSizeAndLocation (Form winform){
			LoadFormSize(winform);
			LoadFormLocation(winform);
		}
		#endregion

        #region [ Static Functions ]
        private static Dictionary<RegistryHive, RegistryKey> _HiveList = new Dictionary<RegistryHive, RegistryKey>();

        /// <summary>
        /// Static Constructor
        /// </summary>
        static RegWriter()
        {
            _HiveList[RegistryHive.CurrentUserSoftware] = Registry.CurrentUser.OpenSubKey("Software", true);
            _HiveList[RegistryHive.CurrentUser] = Registry.CurrentUser;
            _HiveList[RegistryHive.LocalMachineSoftware] = Registry.LocalMachine.OpenSubKey("Software", true);
            _HiveList[RegistryHive.LocalMachine] = Registry.LocalMachine;
            _HiveList[RegistryHive.ClassesRoot] = Registry.ClassesRoot;
            _HiveList[RegistryHive.Users] = Registry.Users;
            _HiveList[RegistryHive.CurrentConfig] = Registry.CurrentConfig;
        }

        /// <summary>
        /// Get string value from the sub path
        /// </summary>
        /// <param name="hive"></param>
        /// <param name="subpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetValue(RegistryHive hive, string subpath, string defaultValue)
        {
            try
            {
                int index = subpath.LastIndexOf("\\");

                string path = subpath.Substring(0, index);
                string key = subpath.Substring(index + 1);

                using (RegistryKey regkey = _HiveList[hive].OpenSubKey(path))
                {                    
                    if (regkey != null)
                    {
                        string value = regkey.GetValue(key).ToString();
                        return value;
                    }
                    return defaultValue;
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="subpath">The subpath.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int GetValue(RegistryHive hive, string subpath, int defaultValue)
        {
            string value = GetValue(hive, subpath, defaultValue.ToString());
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="subpath">The subpath.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static bool GetValue(RegistryHive hive, string subpath, bool defaultValue)
        {
            string value = GetValue(hive, subpath, defaultValue.ToString());
            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Assign value to the hive
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="subpath">The subpath.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static bool SetValue(RegistryHive hive, string subpath, object data)
        {
            try
            {
                int index = subpath.LastIndexOf("\\");
                string path = subpath.Substring(0, index);
                string key = subpath.Substring(index + 1);

                using (RegistryKey regkey = _HiveList[hive].CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (regkey != null)
                    {
                        regkey.SetValue(key, data.ToString());
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
