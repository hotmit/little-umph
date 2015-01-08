using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

using System.Xml;

namespace LittleUmph {
    
    /// <summary>
    /// 
    /// </summary>
    public class XMLSaver
    {
        #region [ Private Variables ]
        private string _Path;
        private XmlNode _Root;
        private XmlDocument _Doc;
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Save setting to xml dir.
        /// </summary>
        /// <param name="path">Fullpath with filename to the setting dir location.</param>
        public XMLSaver(string path)
        {
            Init(path);
        }

        private void Init(string path)
        {
            try
            {
                _Path = path;
                _Doc = new XmlDocument();

                if (File.Exists(path))
                {
                    _Doc.Load(_Path);
                    // Select root node
                    _Root = _Doc.SelectSingleNode(@"//XMLSaver");
                }
                else
                {
                    if (!Directory.Exists(IOFunc.GetPath(path)))
                    {
                        Directory.CreateDirectory(IOFunc.GetPath(path));
                    }

                    // Create xml start tag
                    XmlNode node = _Doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                    _Doc.AppendChild(node);

                    // Create root node
                    _Root = _Doc.CreateNode(XmlNodeType.Element, "XMLSaver", "");

                    // Assign author attribute
                    XmlAttribute author = _Doc.CreateAttribute("CreatedBy");
                    author.Value = "Hột Mít";
                    _Root.Attributes.Append(author);

                    // Assign webpage attribute
                    XmlAttribute webpage = _Doc.CreateAttribute("Webpage");
                    webpage.Value = "http://www.hotmit.com";
                    _Root.Attributes.Append(webpage);

                    _Doc.AppendChild(_Root);
                }
            }
            catch (Exception xpt)
            {
                MessageBox.Show(xpt.Message, "XMLSaver Constructor");
            }
        }

        /// <summary>
        /// Save the setting to "setting.xml" in executed Path.
        /// </summary>
        public XMLSaver()
            : this(IOFunc.GetCurrentPath() + "Setting.xml")
        {
        }

        /// <summary>
        /// Save the setting to the system application data folder.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <param name="location">SaveFormLocation Enum</param>
        public XMLSaver(string appName, SaveLocation location)            
        {
            string filepath = IOFunc.GetCurrentPath() + "Setting.xml";

            if (location == SaveLocation.SysAppDir)
            {
                string sys = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string name = IOFunc.CleanFileName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
                appName = IOFunc.CleanFileName(appName);
                filepath = string.Format("{0}\\XMLSaver\\{1}-{2}_Setting.xml", sys, name, appName);
            }

            Init(filepath);
        }
        #endregion

        #region [ Delete Data ]
        /// <summary>
        /// Deletes the data dir and start a new one.
        /// </summary>
        public void DeleteData()
        {
            try
            {
                File.Delete(_Path);
                Init(_Path);
            }
            catch (Exception xpt)
            {
                Gs.Log.Error("XMLSaver.DeleteData()", xpt);
            }
        }
        #endregion

        #region [ Write Functions ]
        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, string data)
        {
            name = CleanName(name);

            XmlNode node = _Doc.CreateNode(XmlNodeType.Element, name, "");
            node.InnerText = data;

            XmlNode oldNode = _Root.SelectSingleNode(@"//" + name);

            if (oldNode == null)
            {
                _Root.AppendChild(node);
            }
            else
            {
                _Root.ReplaceChild(node, oldNode);
            }

            // Save to disk
            _Doc.Save(_Path);
        }

        /// <summary>
        /// Remove all non valid characters from the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private string CleanName(string name)
        {
            return Regex.Replace(name, @"\W", "");
        }

        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, int data)
        {
            Write(name, data.ToString());
        }

        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, double data)
        {
            Write(name, data.ToString());
        }

        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, decimal data)
        {
            Write(name, data.ToString());
        }

        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, float data)
        {
            Write(name, data.ToString());
        }

        /// <summary>
        /// Save value to xml dir.
        /// </summary>
        /// <param name="name">Name of the value you want to save.</param>
        /// <param name="data">The value you want to save.</param>
        public void Write(string name, bool data)
        {
            Write(name, data.ToString());
        }
        #endregion

        #region [ Read Functions ]
        /// <summary>
        /// Get the string value from the saved xml dir.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <param name="defaultData">The default value to return on error.</param>
        /// <returns>Return "defaulData" if not found or error.</returns>
        public string GetString(string name, string defaultData)
        {
            name = CleanName(name);

            try
            {
                XmlNode node = _Root.SelectSingleNode(@"//" + name);
                if (node == null)
                {
                    return defaultData;
                }
                else
                {
                    return node.InnerText;
                }
            }
            catch
            {
                return defaultData;
            }
        }

        /// <summary>
        /// Get the string value from the saved xml dir. Return empty string on error.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <returns>Return empty string on error.</returns>
        public string GetString(string name)
        {
            return GetString(name, "");
        }

        /// <summary>
        /// Get the int value from the saved xml dir.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <param name="defaultData">The default value to return on error.</param>
        /// <returns>Return "defaulData" if not found or error.</returns>
        public int GetInt(string name, int defaultData)
        {
            try
            {
                string result = GetString(name, defaultData.ToString());
                int intValue = Convert.ToInt32(result);
                return intValue;
            }
            catch
            {
                return defaultData;
            }
        }

        /// <summary>
        /// Get int value of the saved data. Return -1 on error.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <returns>Return -1 if not found or on error.</returns>
        public int GetInt(string name)
        {
            return GetInt(name, -1);
        }

        /// <summary>
        /// Get boolean value of the saved data.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <param name="defaultData">if set to <c>true</c> [default data].</param>
        /// <returns>
        /// Return defaultData if not found or on error.
        /// </returns>
        public bool GetBool(string name, bool defaultData)
        {
            try
            {
                string result = GetString(name, defaultData.ToString());
                bool boolValue = Convert.ToBoolean(result);
                return boolValue;
            }
            catch
            {
                return defaultData;
            }
        }

        /// <summary>
        /// Get boolean value of the saved data. Return false on error.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <returns>Return false if not found or on error.</returns>
        public bool GetBool(string name)
        {
            return GetBool(name, false);
        }

        /// <summary>
        /// Get double value of the saved data.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <param name="defaultData">The default data.</param>
        /// <returns>
        /// Return defaultData if not found or on error.
        /// </returns>
        public double GetDouble(string name, double defaultData)
        {
            try
            {
                string result = GetString(name, defaultData.ToString());
                double doubleValue = Convert.ToDouble(result);
                return doubleValue;
            }
            catch
            {
                return defaultData;
            }
        }

        /// <summary>
        /// Get double value of the saved data. Return -1.0 on error.
        /// </summary>
        /// <param name="name">Name of the saved data.</param>
        /// <returns>Return -1.0 if not found or on error.</returns>
        public double GetDouble(string name)
        {
            return GetDouble(name, -1.0);
        }

        /// <summary>
        /// Gets the safe path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetSafePath(string name)
        {
            return IOFunc.PathFinder(GetString(name));
        }
        #endregion

        #region [ Generic Get ]
        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Get<T>(string name, T defaultValue)
        {
            string result = GetString(name, null);
            if (result == null)
            {
                return defaultValue;
            }

            try
            {
                T value = (T)Convert.ChangeType(result, typeof(T));
                return value;
            }
            catch (Exception xpt)
            {
                return defaultValue;
            }
        }
        #endregion

        #region [ Save & Load Winform Location and Size ]
        /// <summary>
        /// Restore the form position.
        /// </summary>
        /// <param name="winform"></param>
        public void LoadFormLocation(Form winform)
        {
            this.LoadFormLocation(winform, false);
        }

        /// <summary>
        /// Restore the form position.
        /// </summary>
        /// <param name="winform">The winform.</param>
        /// <param name="loadFormSize">if set to <c>true</c> [load form size].</param>
        public void LoadFormLocation(Form winform, bool loadFormSize)
        {
            int savedX = GetInt(winform.Name + "_FormPositionX");
            int savedY = GetInt(winform.Name + "_FormPositionY");
            
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
                this.LoadFormSize(winform);
            }
        }

        /// <summary>
        /// Save winform location, do not save the size of the winform.
        /// </summary>
        /// <param name="winform"></param>
        public void SaveFormLocation(Form winform)
        {
            this.SaveFormLocation(winform, false);
        }

        /// <summary>
        /// Save the winform location.
        /// </summary>
        /// <param name="winform">The winform.</param>
        /// <param name="saveFormSize">if set to <c>true</c> [save form size].</param>
        public void SaveFormLocation(Form winform, bool saveFormSize)
        {
            Point location = winform.Location;
            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            if ((location.X > 0) && (location.Y > 0) && (location.X + 40 < screenWidth) && (location.Y + 40 < screenHeight))
            {
                Write(winform.Name + "_FormPositionX", location.X);
                Write(winform.Name + "_FormPositionY", location.Y);
            }

            if (saveFormSize)
            {
                this.SaveFormSize(winform);
            }
        }

        /// <summary>
        /// Save form size.
        /// </summary>
        /// <param name="winform"></param>
        public void SaveFormSize(Form winform)
        {
            this.Write(winform.Name + "_FormSizeWidth", winform.Size.Width);
            this.Write(winform.Name + "_FormSizeHeight", winform.Size.Height);
        }

        /// <summary>
        /// Restore form size.
        /// </summary>
        /// <param name="winform"></param>
        public void LoadFormSize(Form winform)
        {
            this.LoadFormSize(winform, winform.MinimumSize.Width, winform.MinimumSize.Height);
        }

        /// <summary>
        /// Restore form size
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="defaultWidth">If no value were save from prior session, then use this value as width.</param>
        /// <param name="defaultHeight">If no value were save from prior session, then use this value as height.</param>
        public void LoadFormSize(Form winform, int defaultWidth, int defaultHeight)
        {
            int w = this.GetInt(winform.Name + "_FormSizeWidth", defaultWidth);
            int h = this.GetInt(winform.Name + "_FormSizeHeight", defaultHeight);

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
            Write(winform.Name + "_TopMost", winform.TopMost);
        }

        /// <summary>
        /// Load TopMost, default is false
        /// </summary>
        /// <param name="winform"></param>
        public void LoadFormTopMost(Form winform)
        {
            LoadFormTopMost(winform, false);
        }

        /// <summary>
        /// Load TopMost, if not found use default
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="defaultTopMost"></param>
        /// <returns>Winform TopMost</returns>
        public bool LoadFormTopMost(Form winform, bool defaultTopMost)
        {
            bool topMost = GetBool(winform.Name + "_TopMost", defaultTopMost);
            winform.TopMost = topMost;
            return topMost;
        }

        /// <summary>
        /// Load TopMost and check the ToolStripMenuItem accordantly
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="control"></param>
        public void LoadFormTopMost(Form winform, ref ToolStripMenuItem control)
        {
            bool topMost = LoadFormTopMost(winform, false);
            control.Checked = topMost;
        }

        /// <summary>
        /// Load TopMost and check the ToolStripMenuItem accordantly, false if not found
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="control"></param>
        /// <param name="defaultTopMost"></param>
        public void LoadFormTopMost(Form winform, ref ToolStripMenuItem control, bool defaultTopMost)
        {
            bool topMost = LoadFormTopMost(winform, defaultTopMost);
            control.Checked = topMost;
        }

        /// <summary>
        /// Load TopMost and check the CheckBox accordantly
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="control"></param>
        public void LoadFormTopMost(Form winform, ref CheckBox control)
        {
            bool topMost = LoadFormTopMost(winform, false);
            control.Checked = topMost;
        }

        /// <summary>
        /// Load TopMost and check the CheckBox accordantly, false if not found
        /// </summary>
        /// <param name="winform"></param>
        /// <param name="control"></param>
        /// <param name="defaultTopMost"></param>
        public void LoadFormTopMost(Form winform, ref CheckBox control, bool defaultTopMost)
        {
            bool topMost = LoadFormTopMost(winform, defaultTopMost);
            control.Checked = topMost;
        }
        #endregion

        #region [ AutoSave & AutoLoad ]
        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(TextBox control)
        {
            string name = control.Name;
            Write(name, control.Text);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(TextBox control)
        {
            string name = control.Name;
            control.Text = GetString(name, control.Text);
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(ToolStripMenuItem control)
        {
            string name = control.Name;
            Write(name, control.Checked);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(ToolStripMenuItem control)
        {
            string name = control.Name;
            control.Checked = GetBool(name);
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(RichTextBox control)
        {
            string name = control.Name;
            Write(name, control.Text);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(RichTextBox control)
        {
            string name = control.Name;
            control.Text = GetString(name);
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(CheckBox control)
        {
            string name = control.Name;
            Write(name, control.Checked);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(CheckBox control)
        {
            string name = control.Name;
            control.Checked = GetBool(name);
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(RadioButton control)
        {
            string name = control.Name;
            Write(name, control.Checked);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(RadioButton control)
        {
            string name = control.Name;
            control.Checked = GetBool(name);
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(ComboBox control)
        {
            string name = control.Name;
            Write(name, control.SelectedIndex);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(ComboBox control)
        {
            string name = control.Name;
            int index = GetInt(name);

            if ((control.Items.Count > index) && (index >= 0))
            {
                control.SelectedIndex = index;
            }
            else if (control.Items.Count > 0)
            {
                control.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(ListBox control)
        {
            string name = control.Name;
            Write(name, control.SelectedIndex);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(ListBox control)
        {
            string name = control.Name;
            int index = GetInt(name);

            if ((control.Items.Count > index) && (index >= 0))
            {
                control.SelectedIndex = index;
            }
            else if (control.Items.Count > 0)
            {
                control.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(FolderBrowserDialog control)
        {
            Write("FolderBrowserDialog", control.SelectedPath);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(FolderBrowserDialog control)
        {
            control.SelectedPath = IOFunc.PathFinder(GetString("FolderBrowserDialog", ""));
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(OpenFileDialog control)
        {
            Write("OpenFileDialog", control.InitialDirectory);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(OpenFileDialog control)
        {
            control.InitialDirectory = IOFunc.PathFinder(GetString("OpenFileDialog", ""));
        }

        /// <summary>
        /// Autoes the save.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(NumericUpDown control)
        {
            Write(control.Name, control.Value);
        }

        /// <summary>
        /// Autoes the load.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(NumericUpDown control)
        {
            control.Value = Get<decimal>(control.Name, control.Value);   
        }


        /// <summary>
        /// Save the setting associate with the control
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoSave(Control control)
        {
            /*
            TextBox
            ToolStripMenuItem   // not a control
            RichTextBox
            CheckBox
            RadioButton
            ComboBox
            ListBox
            FolderBrowserDialog // not a control
            OpenFileDialog      // not a control
            NumericUpDown
             */

            if (control is TextBox)
            {
                AutoSave((TextBox)control);
            }
            else if (control is RichTextBox)
            {
                AutoSave((RichTextBox)control);
            }
            else if (control is CheckBox)
            {
                AutoSave((CheckBox)control);
            }
            else if (control is RadioButton)
            {
                AutoSave((RadioButton)control);
            }
            else if (control is ComboBox)
            {
                AutoSave((ComboBox)control);
            }
            else if (control is ListBox)
            {
                AutoSave((ListBox)control);
            }
            else if (control is NumericUpDown)
            {
                AutoSave((NumericUpDown)control);
            }
            else
            {
                Write(control.Name, control.Text);
            }
        }

        /// <summary>
        /// Load the setting associate with the control.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AutoLoad(Control control)
        {
            if (control is TextBox)
            {
                AutoLoad((TextBox)control);
            }
            else if (control is RichTextBox)
            {
                AutoLoad((RichTextBox)control);
            }
            else if (control is CheckBox)
            {
                AutoLoad((CheckBox)control);
            }
            else if (control is RadioButton)
            {
                AutoLoad((RadioButton)control);
            }
            else if (control is ComboBox)
            {
                AutoLoad((ComboBox)control);
            }
            else if (control is ListBox)
            {
                AutoLoad((ListBox)control);
            }
            else if (control is NumericUpDown)
            {
                AutoLoad((NumericUpDown)control);
            }
            else
            {
                control.Text = GetString(control.Name, "");
            }
        }

        /// <summary>
        /// Autoes the save collection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        public void AutoSaveCollection(params Control[] controls)
        {
            foreach (var c in controls)
            {
                AutoSave(c);
            }
        }

        public void AutoSaveCollection(params object[] controls)
        {
            foreach (var o in controls)
            {
                if (o is Control)
                {
                    AutoSave((Control)o);
                }
                else if (o is ToolStripMenuItem)
                {
                    AutoSave((ToolStripMenuItem)o);
                }
                else if (o is FolderBrowserDialog)
                {
                    AutoSave((FolderBrowserDialog)o);
                }
            }
        }

        /// <summary>
        /// Autoes the load collection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        public void AutoLoadCollection(params Control[] controls)
        {
            foreach (var c in controls)
            {
                AutoLoad(c);
            }
        }

        public void AutoLoadCollection(params object[] controls)
        {
            foreach (var o in controls)
            {
                if (o is Control)
                {
                    AutoLoad((Control)o);
                }
                else if (o is ToolStripMenuItem)
                {
                    AutoLoad((ToolStripMenuItem)o);
                }
                else if (o is FolderBrowserDialog)
                {
                    AutoLoad((FolderBrowserDialog)o);
                }
            }
        }
        #endregion        

    }
}
