using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Design;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

using LittleUmph;

namespace LittleUmph.GUI.Components
{
    /// <summary>
    /// Save common settings automatically.
    /// </summary>
    [ProvideProperty("AutoSave", typeof(Control))]
    public partial class SettingSaver : Component, ISupportInitialize, IExtenderProvider
    {
        #region [ Private Variables ]
        SpreadPattern _spread = new SpreadPattern(10000);
        #endregion

        #region [ Events ]
        public delegate void DataLoadedHandler(SettingSaver saver);
        /// <summary>
        /// Occurs when data is loaded from the setting dir.
        /// </summary>
        [Description("Occurs when data is loaded from the setting dir.")]
        public event DataLoadedHandler DataLoaded;
        #endregion

        #region [ Private Variables ]
        #endregion

        #region [ Properties ]
        /// <summary>
        /// The form where this component belong to.
        /// </summary>
        [Category("[ SettingSaver ]")]
        [Description("The form where this component belong to.")]
        public Form Form { get; set; }

        /// <summary>
        /// Save form location.
        /// </summary>
        [Category("[ SettingSaver ]")]
        [Description("Save form location.")]
        [DefaultValue(true)]
        public bool SaveFormLocation { get; set; }

        /// <summary>
        /// Save form size.
        /// </summary>
        [Category("[ SettingSaver ]")]
        [Description("Save form size.")]
        [DefaultValue(false)]
        public bool SaveFormSize { get; set; }

        /// <summary>
        /// Save top most status.
        /// </summary>
        [Category("[ SettingSaver - Top Most ]")]
        [Description("Save top most status.")]
        [DefaultValue(false)]
        public bool SaveTopMost { get; set; }

        /// <summary>
        /// The top most checkbox.
        /// </summary>
        [Category("[ SettingSaver - Top Most ]")]
        [Description("The top most checkbox.")]
        public CheckBox TopMostCheckBox { get; set; }

        /// <summary>
        /// The top most menu checkbox.
        /// </summary>
        [Category("[ SettingSaver - Top Most ]")]
        [Description("The top most menu checkbox.")]
        public ToolStripMenuItem TopMostMenuItem { get; set; }

        /// <summary>
        /// Gets or sets the XML saver.
        /// </summary>
        /// <value>
        /// The XML saver.
        /// </value>
        [Browsable(false)]
        public DataStore DataStore { get ; private set; }

        /// <summary>
        /// Where to save the setting dir.
        /// </summary>
        [Category("[ SettingSaver ]")]
        [Description("Where to save the setting file.")]
        [DefaultValue(SaveLocation.CurrentFolder)]
        public SaveLocation SaveLocation { get; set; }
        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingSaver" /> class.
        /// </summary>
        public SettingSaver()
            : this(null)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingSaver" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public SettingSaver(IContainer container)
        {
            SaveFormLocation = true;
            SaveLocation = SaveLocation.CurrentFolder;

            if (container != null)
            {
                container.Add(this);
            }
            InitializeComponent();
        }
        #endregion

        #region [ Delete Settings ]
        /// <summary>
        /// Remove the settings dir and start a new one.
        /// </summary>
        public void DeleteSettings()
        {
            DataStore.RemoveGroup(DataStore.DefaultGroup);
        }
        #endregion

        #region [ Get The Host Form ]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;

                IDesignerHost host = null;
                if (base.Site != null)
                {
                    host = (IDesignerHost)base.Site.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        Form form = (Form)host.RootComponent;
                        Form = form;
                    }
                }
            }
        }
        #endregion 
    
        #region [ ISupportInitialize ]
        public void BeginInit()
        {

        }

        public void EndInit()
        {
            string name = SaveLocation == LittleUmph.SaveLocation.SysAppDir ? "Setting Saver" : "";
            DataStore = new DataStore(name, SaveLocation);
            DataStore.AutoSave = true;
            DataStore.DefaultGroup = Form.Name;

            DataStore.LoadCollection(AutoSaverList.ToArray());

            Form.Load += Form_Load;
            Form.FormClosing += Form_Closing;

            if (DataLoaded != null)
            {
                DataLoaded(this);
            }
        }        
        #endregion

        #region [ Loading ]
        void Form_Load(object sender, EventArgs e)
        {
            if (SaveFormLocation)
            {
                DataStore.LoadFormLocation(Form);
                Form.LocationChanged += (s, evt) =>
                {
                    _spread.Execute(() =>
                    {
                        DataStore.SaveFormLocation(Form);
                    }, true);
                };
            }

            if (SaveFormSize)
            {
                DataStore.LoadFormSize(Form);
                Form.SizeChanged += (s, evt) =>
                {
                    _spread.Execute(() =>
                    {
                        DataStore.SaveFormSize(Form);
                    }, true);
                };
            }

            if (SaveTopMost)
            {
                DataStore.LoadFormTopMost(Form, false);

                if (TopMostCheckBox != null)
                {
                    TopMostCheckBox.Checked = Form.TopMost;
                    TopMostCheckBox.CheckedChanged += (s, evt) =>
                    {
                        Form.TopMost = TopMostCheckBox.Checked;
                        _spread.Execute(() =>
                        {
                            DataStore.SaveFormTopMost(Form);
                        }, true);
                    };
                }

                if (TopMostMenuItem != null)
                {
                    TopMostMenuItem.Checked = Form.TopMost;
                    TopMostMenuItem.CheckedChanged += (s, evt) =>
                    {
                        Form.TopMost = TopMostMenuItem.Checked;
                        _spread.Execute(() =>
                        {
                            DataStore.SaveFormTopMost(Form);
                        }, true);
                    };
                }
            }
        }
        #endregion

        #region [ Closing ]
        void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (SaveFormLocation)
            {
                DataStore.SaveFormLocation(Form);
            }

            if (SaveFormSize)
            {
                DataStore.SaveFormSize(Form);
            }

            if (SaveTopMost)
            {
                DataStore.SaveFormTopMost(Form);
            }

            DataStore.SaveCollection(AutoSaverList.ToArray());
        }
        #endregion


        #region [ IExtenderProvider ]
        List<Control> _AutoSaverList = new List<Control>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Control> AutoSaverList
        {
            get
            {
                return _AutoSaverList;
            }
            set
            {
                _AutoSaverList = value;
            }
        }

        public bool CanExtend(object o)
        {
            return o is Control;
        }

        [Category("[ Setting Saver ]")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool GetAutoSave(Control c)
        {
            if (AutoSaverList.Contains(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Category("[ Setting Saver ]")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public void SetAutoSave(Control c, bool value)
        {
            if (_AutoSaverList.Contains(c))
            {
                _AutoSaverList.Remove(c);
            }

            if (value)
            {
                _AutoSaverList.Add(c);
            }
        }

        public bool ShouldSerializeAutoSave(Control c)
        {
            return true;
        }

        public void ResetAutoSave(Control c)
        {
            if (_AutoSaverList.Contains(c))
            {
                _AutoSaverList.Remove(c);
            }
        }
        #endregion
    }
}
