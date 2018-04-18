using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;


namespace LittleUmph
{
    public enum SaveLocation { SysAppDir, CurrentFolder };

    /// <summary>
    /// Shortcut to DataStore default instances.
    /// </summary>
    public class Ds
    {
        #region [ Static ]
        /// <summary>
        ///The default instance (Save "Settings.xml" to working directory).
        /// </summary>
        /// <value>The default.</value>
        public static DataStore Default { get; set; }

        /// <summary>
        ///The default instance (Save to current user profile directory).
        /// </summary>
        /// <value>The profile.</value>
        public static DataStore Profile { get; set; }

        /// <summary>
        /// Initializes the <see cref="DataStore"/> class.
        /// </summary>
        static Ds()
        {
            Default = new DataStore();
            Default.AutoSave = true;

            string profileDataPath = DataStore.GetProfileSettingPath("Settings");
            Profile = new DataStore(profileDataPath);
            Profile.AutoSave = true;
        }
        #endregion
    }

    /// <summary>
    /// Save and retreive settings from a xml dir.
    /// </summary>
    public class DataStore
    {
        #region [ Constants ]
        private const string STR_HASH = "Qm4RSFTU1qWCiXP2K";
        #endregion

        #region [ Indexer ]
        /// <summary>
        /// Gets or sets the setting value with the specified name.
        /// </summary>
        /// <value></value>
        public object this[string settingName]
        {
            get { return Get<object>(settingName); }
            set { Set(settingName, value); }
        }
        #endregion

        #region [ Private ]
        private XmlDocument _xmlDoc;
        #endregion

        #region [ Properties ]
        private string _DefaultGroup = "Default";
        /// <summary>
        /// Gets or sets the default group name.
        /// </summary>
        /// <value>
        /// The default group.
        /// </value>
        public string DefaultGroup
        {
            get { return _DefaultGroup; }
            set { _DefaultGroup = value; }
        }

        private string _filepath;
        /// <summary>
        /// Gets or sets the full filepath to the xml file.
        /// </summary>
        /// <value>The filepath.</value>
        public string Filepath
        {
            get
            {                
                return _filepath;
            }
            set
            {
                try
                {
                    _filepath = value;
                    if (!Path.IsPathRooted(_filepath))
                    {
                        _filepath = Path.Combine(IOFunc.ExecutableDirectory, _filepath);

                        string dir = Path.GetDirectoryName(_filepath);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }
                }
                catch (Exception xpt)
                {
                    Gs.Log.Error("DataStore.Filepath", xpt.Message);
                }
            }
        }

        /// <summary>
        /// Set this to true to automatically save the settings to 
        /// xml file on every modification to the settings data.
        /// 
        /// If this is set to false, you must manually call Save() to 
        /// get the data to persist.
        /// </summary>
        /// <value><c>true</c> if [auto save]; otherwise, <c>false</c>.</value>
        public bool AutoSave { get; set; }

        private string _commonKey;
        private string _commonKey_Key;

        /// <summary>
        /// If this value is assigned, when the passphrase parameter on the function is emptied,
        ///  all setting will be encrypted using this key. 
        /// </summary>
        /// <value>The common key.</value>
        public string CommonKey
        {
            private get
            {
                if (Str.IsEmpty(_commonKey))
                {
                    return string.Empty;
                }
                else
                {
                    return _commonKey;
                }
            }
            set
            {
                if (Str.IsNotEmpty(value))
                {

                    _commonKey_Key = SimpleEncryption.MD5Hash(value + STR_HASH);
                    _commonKey = SimpleEncryption.Encrypt(value, _commonKey_Key);
                }
                else
                {
                    _commonKey = string.Empty;
                    _commonKey_Key = string.Empty;
                }
            }
        }

        /// <summary>
        /// List of all the groups in the setting file.
        /// </summary>
        public List<string> Groups
        {
            get
            {
                var list = new List<string>();

                var nodes = _xmlDoc.DocumentElement.SelectNodes("Group");
                foreach (XmlNode n in nodes)
                {
                    var name = n.Attributes["Name"].Value;
                    list.Add(name);
                }

                return list;
            }
        }
        #endregion
        
        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        public DataStore()
        {
            string path = Path.Combine(IOFunc.FolderCurrent, "Setting.xml");
            InitSettingFile(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public DataStore(string filepath)
        {
            filepath = Path.IsPathRooted(filepath) ? filepath : Path.Combine(IOFunc.FolderCurrent, filepath);
            InitSettingFile(filepath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore" /> class.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="location">The location.</param>
        public DataStore(string appName, SaveLocation location)
        {
            string filename = Str.IsEmpty(appName) ? "Settings.xml" : appName.Trim() + " Settings.xml";
            string filepath = IOFunc.GetCurrentPath() + filename;

            if (location == SaveLocation.SysAppDir)
            {
                filepath = GetProfileSettingPath(appName);
            }

            InitSettingFile(filepath);
        }

        private void InitSettingFile(string filepath)
        {
            try
            {
                Filepath = filepath;

                if (File.Exists(Filepath))
                {
                    try
                    {
                        _xmlDoc = new XmlDocument();
                        _xmlDoc.Load(Filepath);
                    }
                    catch (Exception xpt)
                    {
                        Gs.Log.Error("DataStore()", xpt.Message);
                        CreateNewDocument();
                    }
                }
                else
                {
                    CreateNewDocument();
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.DataStore()", xpt.Message);
            }
        }
        #endregion

        #region [ Set ]
        /// <summary>
        /// Store the setting with the specified name in group "Default".
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        public void Set(string name, object data)
        {
            Set(_DefaultGroup, name, data);
        }

        /// <summary>
        /// Store the setting with the specified name in specified group name.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        public void Set(string groupName, string name, object data)
        {
            EncryptSet(groupName, name, data, string.Empty);            
        }
        #endregion

        #region [ Set With Encryption ]
        /// <summary>
        /// Store the setting with the specified name in group "Default".
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        public void EncryptSet(string name, object data, string passPhrase)
        {
            EncryptSet(_DefaultGroup, name, data, passPhrase);
        }

        /// <summary>
        /// Store the setting with the specified name in specified group name.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        public void EncryptSet(string groupName, string name, object data, string passPhrase)
        {
            try
            {
                #region [ Name Cleanup ]
                groupName = CleanName(groupName);
                if (Str.IsEmpty(groupName))
                {
                    return;
                }

                name = CleanName(name);
                if (Str.IsEmpty(name))
                {
                    return;
                }
                #endregion

                #region [ Node Building & Null ]
                XmlNode node = CreateSetting(groupName, name);
                if (node.Attributes.Count > 0)
                {
                    node.Attributes.RemoveAll();
                }

                if (data == null)
                {
                    node.InnerText = "";

                    XmlAttribute nullAttrib = _xmlDoc.CreateAttribute("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance");
                    nullAttrib.Value = "true";
                    node.Attributes.Append(nullAttrib);

                    if (AutoSave)
                    {
                        SaveToFile();
                    }
                    return;
                }
                #endregion

                if (Str.IsEmpty(passPhrase) && Str.IsNotEmpty(CommonKey))
                {
                    passPhrase = SimpleEncryption.Decrypt(CommonKey, _commonKey_Key);
                }

                string strData = DType.ObjToString(data);

                if (Str.IsNotEmpty(passPhrase))
                {
                    strData = SimpleEncryption.Encrypt(strData, passPhrase);
                }

                node.InnerText = strData;

                if (AutoSave)
                {
                    SaveToFile();
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.EncryptSet()", xpt.Message);
            }
        }
        #endregion                

        #region [ Get ]
        /// <summary>
        /// Gets the value for specified name (supported data type: string, int, bool, double, date).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            return Get<T>(_DefaultGroup, name, default(T));
        }

        /// <summary>
        /// Gets the value for specified name (supported data type: string, int, bool, double, date).        
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Get<T>(string name, T defaultValue)
        {
            return Get<T>(_DefaultGroup, name, defaultValue);
        }

        /// <summary>
        /// Gets the value for specified name from the specified group (supported data type: string, int, bool, double, date).        
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Get<T>(string groupName, string name, T defaultValue)
        {
            return EncryptGet<T>(groupName, name, defaultValue, string.Empty);           
        }

        /// <summary>
        /// Craw up each folder in the path and return the first level that exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string GetExistedPath(string name, string defaultValue)
        {
            return GetExistedPath(DefaultGroup, name, defaultValue);
        }

        /// <summary>
        /// Craw up each folder in the path and return the first level that exists.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string GetExistedPath(string groupName, string name, string defaultValue)
        {
            string path = Get(groupName, name, defaultValue);
            if (path != defaultValue)
            {
                path = IOFunc.PathFinder(path);
                return path;
            }
            return defaultValue;
        }

        #endregion

        #region [ Get Encrypted Data ]
        /// <summary>
        /// Gets the value for specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns></returns>
        public T EncryptGet<T>(string name, string passPhrase)
        {
            return EncryptGet<T>(_DefaultGroup, name, default(T), passPhrase);
        }

        /// <summary>
        /// Gets the value for specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns></returns>
        public T EncryptGet<T>(string name, T defaultValue, string passPhrase)
        {
            return EncryptGet<T>(_DefaultGroup, name, defaultValue, passPhrase);
        }

        /// <summary>
        /// Gets the value for specified name from the specified group.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns></returns>
        public T EncryptGet<T>(string groupName, string name, T defaultValue, string passPhrase)
        {
            try
            {
                #region [ Name Cleanup ]
                groupName = CleanName(groupName);
                if (Str.IsEmpty(groupName))
                {
                    return defaultValue;
                }

                name = CleanName(name);
                if (Str.IsEmpty(name))
                {
                    return defaultValue;
                }
                #endregion

                #region [ Node Creation & Null ]
                XmlNode node = GetSetting(groupName, name);

                if (node == null)
                {
                    return defaultValue;
                }
                #endregion                

                if (Str.IsEmpty(passPhrase) && Str.IsNotEmpty(_commonKey))
                {
                    passPhrase = SimpleEncryption.Decrypt(CommonKey, _commonKey_Key);
                }
                
                string value = node.InnerText;

                if (!Str.IsEmpty(passPhrase))
                {
                    value = SimpleEncryption.Decrypt(value, passPhrase);
                }

                return DType.Get<T>(value, defaultValue);                                                
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.EncryptGet()", xpt.Message);
                return defaultValue;
            }
        }
        #endregion


        #region [ Group Functions ]
        /// <summary>
        /// Removes the group from the settings collection.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        public void RemoveGroup(string groupName)
        {
            try
            {
                XmlNode groupNode = GetGroup(groupName);
                if (groupNode != null)
                {
                    groupNode.ParentNode.RemoveChild(groupNode);

                    if (AutoSave)
                    {
                        SaveToFile();
                    }
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.RemoveGroup()", xpt.Message);
            }
        }

        /// <summary>
        /// Groups the exist.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        public bool GroupExist(string groupName)
        {
            return GetGroup(groupName) != null;
        }

        /// <summary>
        /// Get the display text of the group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns groupName if title is not found.
        /// </returns>
        public string GetGroupTitle(string groupName, string defaultValue = null)
        {
            try
            {
                XmlNode groupNode = GetGroup(groupName);
                if (groupNode != null)
                {
                    return groupNode.Attributes["title"].Value;
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.GetGroupTitle()", xpt.Message);
            }

            return defaultValue;
        }

        /// <summary>
        /// Set the display title for the group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="title"></param>
        public void SetGroupTitle(string groupName, string title)
        {
            try
            {
                XmlNode groupNode = GetGroup(groupName);
                if (groupNode != null)
                {
                    CreateOrReplaceAttribute(ref groupNode, "Title", title);

                    if (AutoSave)
                    {
                        SaveToFile();
                    }
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.SetGroupTitle()", xpt.Message);
            }
        }
        #endregion


        #region [ SaveToFile ]
        /// <summary>
        /// Saves all the current settings to xml.
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                _xmlDoc.Save(Filepath);
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.Save()", xpt.Message);
            }
        }
        #endregion

        #region [ Remove ]
        /// <summary>
        /// Removes the specified name from the settings collection.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Remove(string name)
        {
            Remove(_DefaultGroup, name);
        }

        /// <summary>
        /// Removes the specified name from the settings collection.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="name">The name.</param>
        public void Remove(string groupName, string name)
        {
            try
            {
                XmlNode groupNode = GetGroup(groupName);
                if (groupNode != null)
                {
                    XmlNode node = groupNode.SelectSingleNode(name);
                    if (node != null)
                    {
                        groupNode.RemoveChild(node);

                        if (AutoSave)
                        {
                            SaveToFile();
                        }
                    }
                }
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.Remove()", xpt.Message);
            }
        }
        #endregion
        
        #region [ Delete & View File ]
        /// <summary>
        /// Deletes the xml settings dir.
        /// </summary>
        public void DeleteFile()
        {
            if (File.Exists(Filepath))
            {
                try
                {
                    File.Delete(Filepath);
                }
                catch (Exception xpt)
                {
                    Gs.Log.Error("DataStore.DeleteFile()", xpt.Message);
                }
            }
            CreateNewDocument();
        }

        /// <summary>
        /// Open the xml setting dir using the system default xml viewer.
        /// </summary>
        public void ViewFile()
        {
            try
            {
                IOFunc.RunCmd(Path.GetFullPath(Filepath));
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.ViewFile()", xpt.Message);
            }
        }
        #endregion

        #region [ Static Functions ]
        /// <summary>
        /// Gets the profile setting dir path in the current user application data folder.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string GetProfileSettingPath(string name)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DataStore\\";
            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }            

            string exeName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            exeName = CleanName(name) + "_" + CleanName(exeName) + ".xml";

            return Path.Combine(appData, exeName);
        }

        /// <summary>
        /// Generate unique profile group name based on the value specified.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        public string GetProfileGroupName(string groupName)
        {
            string userName = Environment.UserName;
            string domainName = Environment.UserDomainName;

            return string.Format("{0}.{1}.{2}", domainName, userName, groupName);
        }
        #endregion

        #region [ Helper Function ]
        /// <summary>
        /// When the data is being change by an external source,
        /// call this function to reload the data from the xml file.
        /// </summary>
        public void RefreshData()
        {
            InitSettingFile(Filepath);
        }

        private void CreateNewDocument()
        {
            try
            {
                _xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = _xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlNode root = _xmlDoc.CreateNode(XmlNodeType.Element, "Settings", null);

                _xmlDoc.AppendChild(root);
                _xmlDoc.InsertBefore(xmlDeclaration, root);
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.CreateNewDocument()", xpt.Message);
            }
        }

        private XmlNode CreateSetting(string groupName, string name)
        {
            try
            {
                XmlNode groupNode = CreateGroup(groupName);

                XmlNode node = GetSetting(groupName, name);
                if (node == null)
                {
                    XmlNode settingNode = _xmlDoc.CreateNode(XmlNodeType.Element, name, null);
                    groupNode.AppendChild(settingNode);
                    return settingNode;
                }
                return node;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.CreateSetting()", xpt.Message);
                return null;
            }
        }

        private XmlNode CreateGroup(string groupName)
        {
            try
            {
                XmlNode node = GetGroup(groupName);
                if (node == null)
                {
                    XmlNode newGroup = _xmlDoc.CreateNode(XmlNodeType.Element, "Group", null);
                    XmlAttribute attrib = _xmlDoc.CreateAttribute("Name");
                    attrib.Value = groupName;
                    newGroup.Attributes.Append(attrib);

                    _xmlDoc.DocumentElement.AppendChild(newGroup);
                    return newGroup;
                }
                return node;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.CreateGroup()", xpt.Message);
                return null;
            }
        }

        private XmlNode GetSetting(string groupName, string name)
        {
            try
            {
                XmlNode groupNode = GetGroup(groupName);
                if (groupNode == null)
                {
                    return null;
                }

                XmlNode node = groupNode.SelectSingleNode(name);
                return node;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.GetSetting()", xpt.Message);
                return null;
            }
        }

        private XmlNode GetGroup(string groupName)
        {
            try
            {
                XmlNode node = _xmlDoc.DocumentElement.SelectSingleNode(string.Format("Group[@Name='{0}']", groupName));
                return node;
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("DataStore.GetGroup()", xpt.Message);
                return null;
            }
        }

        private void CreateOrReplaceAttribute(ref XmlNode node, string key, string value)
        {
            XmlAttribute attrib = _xmlDoc.CreateAttribute(key);
            attrib.Value = value;

            if (node.Attributes[attrib.Name] != null)
            {
                node.Attributes[attrib.Name].Value = attrib.Value;
            }
            else
            {
                node.Attributes.Append(attrib);
            }
        }

        /// <summary>
        /// Remove invalid characters from group names or key names.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CleanName(string name)
        {
            name = Regex.Replace(name, @"[^a-zA-Z0-9_\.\-]", "").Trim();
            return name;
        }
        #endregion


        #region [ Save & Load Winform Location and Size ]
        /// <summary>
        /// Restore the form position.
        /// </summary>
        /// <param name="winform">The winform.</param>
        /// <param name="loadFormSize">if set to <c>true</c> [load form size].</param>
        public void LoadFormLocation(Form winform, bool loadFormSize = false)
        {
            int savedX = Get<int>(winform.Name + "_FormPositionX", -1);
            int savedY = Get<int>(winform.Name + "_FormPositionY", -1);

            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            Size formSize = winform.Size;
            int w = formSize.Width;
            int h = formSize.Height;

            winform.StartPosition = FormStartPosition.Manual;


            // If less than zero or max length minus 40px then reset location
            if (savedX < 0 || savedY < 0 || savedX + 40 > screenWidth || savedY + 40 > screenHeight
                || savedX + w > screenWidth || savedY + h > screenHeight)
            {
                Layout.CenterScreen(winform);
            }
            else
            {
                winform.Location = new Point(savedX, savedY);
            }

            if (loadFormSize)
            {
                LoadFormSize(winform);
            }
        }

        /// <summary>
        /// Save the winform location.
        /// </summary>
        /// <param name="winform">The winform.</param>
        /// <param name="saveFormSize">if set to <c>true</c> [save form size].</param>
        public void SaveFormLocation(Form winform, bool saveFormSize = false)
        {
            Point location = winform.Location;
            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            if ((location.X > 0) && (location.Y > 0) && (location.X + 40 < screenWidth) && (location.Y + 40 < screenHeight))
            {
                Set(winform.Name + "_FormPositionX", location.X);
                Set(winform.Name + "_FormPositionY", location.Y);
            }

            if (saveFormSize)
            {
                SaveFormSize(winform);
            }
        }

        /// <summary>
        /// Save form size.
        /// </summary>
        /// <param name="winform"></param>
        public void SaveFormSize(Form winform)
        {
            Set(winform.Name + "_FormSizeWidth", winform.Size.Width);
            Set(winform.Name + "_FormSizeHeight", winform.Size.Height);
        }

        /// <summary>
        /// Restore form size.
        /// </summary>
        /// <param name="winform"></param>
        public void LoadFormSize(Form winform)
        {
            LoadFormSize(winform, winform.MinimumSize.Width, winform.MinimumSize.Height);
        }

        /// <summary>
        /// Restore form size
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="defaultWidth">If no value were save from prior session, then use this value as width.</param>
        /// <param name="defaultHeight">If no value were save from prior session, then use this value as height.</param>
        public void LoadFormSize(Form winform, int defaultWidth, int defaultHeight)
        {
            int w = Get(winform.Name + "_FormSizeWidth", defaultWidth);
            int h = Get(winform.Name + "_FormSizeHeight", defaultHeight);

            w = (w < defaultWidth) ? defaultWidth : w;
            h = (h < defaultHeight) ? defaultHeight : h;

            winform.Size = new Size(w, h);
        }

        /// <summary>
        /// Save the TopMost setting of a form
        /// </summary>
        /// <param name="winform"></param>
        public void SaveFormTopMost(Form winform)
        {
            Set(winform.Name + "_TopMost", winform.TopMost);
        }

        /// <summary>
        /// Load TopMost, if not found use default
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="defaultTopMost"></param>
        /// <returns>Winform TopMost</returns>
        public bool LoadFormTopMost(Form winform, bool defaultTopMost = false)
        {
            var topMost = GetFormTopMost(winform, defaultTopMost);
            winform.TopMost = topMost;
            return topMost;
        }

        /// <summary>
        /// Gets the form top most of the setting saved for the specified form.
        /// </summary>
        /// <param name="winform">The winform.</param>
        /// <param name="defaultTopMost">if set to <c>true</c> [default top most].</param>
        /// <returns></returns>
        public bool GetFormTopMost(Form winform, bool defaultTopMost = false)
        {
            bool topMost = Get(winform.Name + "_TopMost", defaultTopMost);
            return topMost;
        }
        #endregion

        #region [ Save & Load ]
        public void Save(object obj)
        {
            Save(_DefaultGroup, obj);
        }

        /// <summary>
        /// Save the setting associate with the control or a component
        /// </summary>
        /// <param name="obj">The obj.</param>
        public void Save(string groupName, object obj)
        {
            if (obj is CheckBox)
            {
                CheckBox c = (CheckBox)obj;
                Set(groupName, c.Name, c.Checked);
            }
            else if (obj is RadioButton)
            {
                RadioButton c = (RadioButton)obj;
                Set(groupName, c.Name, c.Checked);
            }
            else if (obj is ComboBox)
            {
                ComboBox c = (ComboBox)obj;
                Set(groupName, c.Name, c.SelectedIndex);
            }
            else if (obj is ListBox)
            {
                ListBox c = (ListBox)obj;

                if (c.SelectionMode == SelectionMode.One || c.SelectedIndices.Count == 1)
                {
                    Set(groupName, c.Name, c.SelectedIndex);
                }
                // multi & extended
                else if (c.SelectionMode != SelectionMode.None)
                {
                    if (c.SelectedIndices.Count > 0)
                    {
                        var list = Arr.ToString(c.SelectedIndices);
                        string arr = Arr.Implode(",", list);

                        Set(groupName, c.Name, arr);
                    }
                }
                else
                {
                    Set(groupName, c.Name, "-1");
                }
            }
            else if (obj is NumericUpDown)
            {
                NumericUpDown c = (NumericUpDown)obj;
                Set(groupName, c.Name, c.Value);
            }
            else if (obj is Control)
            {
                Control control = (Control)obj;
                Set(groupName, control.Name + "_Text", control.Text);
            }
            else if (obj is ToolStripMenuItem)
            {
                var c = (ToolStripMenuItem)obj;
                Set(groupName, c.Name, c.Checked);
            }
            else if (obj is MenuItem)
            {
                var c = (MenuItem)obj;
                Set(groupName, "MenuItem" + c.Index, c.Checked);
            }
            else if (obj is FolderBrowserDialog)
            {
                var c = (FolderBrowserDialog)obj;
                Set(groupName, "FolderBrowserDialog", c.SelectedPath);
            }
            else if (obj is OpenFileDialog)
            {
                var c = (OpenFileDialog)obj;
                Set(groupName, "OpenFileDialog", c.InitialDirectory);
            }
        }

        /// <summary>
        /// Loads the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Load(object obj)
        {
            Load(_DefaultGroup, obj);
        }

        /// <summary>
        /// Load the setting associate with the control.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="obj">The obj.</param>
        public void Load(string groupName, object obj)
        {
            if (obj is Control)
            {
                if (obj is CheckBox)
                {
                    CheckBox c = (CheckBox)obj;
                    c.Checked = Get(groupName, c.Name, false);
                }
                else if (obj is RadioButton)
                {
                    RadioButton c = (RadioButton)obj;
                    c.Checked = Get(groupName, c.Name, false);
                }
                else if (obj is ComboBox)
                {
                    ComboBox c = (ComboBox)obj;
                    var index = Get(groupName, c.Name, -1);
                    if (index < c.Items.Count)
                    {
                        c.SelectedIndex = index;
                    }
                }
                else if (obj is ListBox)
                {
                    ListBox c = (ListBox)obj;

                    if (c.SelectionMode == SelectionMode.One)
                    {
                        int index = Get(groupName, c.Name, -1);
                        index = Num.Filter(index, -1, c.Items.Count);
                        if (index != -1)
                        {
                            c.SelectedIndex = index;
                        }
                    }
                    // multi & extended
                    else if (c.SelectionMode != SelectionMode.None)
                    {
                        string arr = Get(groupName, c.Name, "-1");
                        if (arr.Contains(","))
                        {
                            string[] split = arr.Split(',');
                            foreach (var s in split)
                            {
                                int index = Str.IntVal(s);
                                index = Num.Filter(index, 0, c.Items.Count);
                                c.SetSelected(index, true);
                            }
                        }
                        else
                        {
                            c.SelectedIndex = Str.IntVal(arr, -1);
                        }
                    }
                }
                else if (obj is NumericUpDown)
                {
                    NumericUpDown c = (NumericUpDown)obj;
                    c.Value = Get(groupName, c.Name, c.Minimum);
                }

                Control control = (Control)obj;
                control.Enabled = Get(groupName, control.Name + "_Enabled", true);
                control.Visible = Get(groupName, control.Name + "_Visible", true);
                control.Text = Get(groupName, control.Name + "_Text", control.Text);
            }
            else if (obj is ToolStripMenuItem)
            {
                var c = (ToolStripMenuItem)obj;
                c.Checked = Get(groupName, c.Name, false);
            }
            else if (obj is MenuItem)
            {
                var c = (MenuItem)obj;
                c.Checked = Get(groupName, "MenuItem" + c.Index, false);
            }
            else if (obj is FolderBrowserDialog)
            {
                var c = (FolderBrowserDialog)obj;
                c.SelectedPath = GetExistedPath("FolderBrowserDialog", "");
            }
            else if (obj is OpenFileDialog)
            {
                var c = (OpenFileDialog)obj;
                c.InitialDirectory = GetExistedPath("OpenFileDialog", "");
            }
        }

        /// <summary>
        /// Saves the collection control or gui components.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void SaveCollection(IList objects)
        {
            SaveCollection(_DefaultGroup, objects);
        }

        public void SaveCollection(string groupName, IList objects)
        {
            foreach (object o in objects)
            {
                Save(groupName, o);
            }
        }

        

        /// <summary>
        /// Load the properties of the collection control or gui components
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void LoadCollection(IList objects)
        {
            LoadCollection(_DefaultGroup, objects);
        }

        /// <summary>
        /// Load the properties of the collection control or gui components
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="objects">The objects.</param>
        public void LoadCollection(string groupName, IList objects)
        {
            foreach (object o in objects)
            {
                Load(groupName, o);
            }
        }
        #endregion        

    }
}
