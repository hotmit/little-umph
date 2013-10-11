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
using System.Linq;

namespace LittleUmph.GUI.Components
{
    [ToolboxBitmap(typeof(DropCatcher), "Images.DropCatcher.png")]
    public partial class DropCatcher : Component, ISupportInitialize
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">The file.</param>
        public delegate void SingleFileDroppedHandler(FileInfo file);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files">The files.</param>
        public delegate void FilesDroppedHandler(FileInfo[] files);
        public delegate void SingleDirectoryDroppedHandler(DirectoryInfo dir);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirs">The dirs.</param>
        public delegate void DirectoriesDroppedHandler(DirectoryInfo[] dirs);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirs">The dirs.</param>
        /// <param name="files">The files.</param>
        public delegate void FilesAndDirectoriesDroppedHandler(DirectoryInfo[] dirs, FileInfo[] files);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="dirs">The dirs.</param>
        /// <param name="files">The files.</param>
        public delegate void FilesAndDirectoriesDroppedWithRootHandler(DirectoryInfo root, DirectoryInfo[] dirs, FileInfo[] files);

        [Category("[ File Handler ]")]
        [Description("Get the first file drop.")]
        public event SingleFileDroppedHandler SingleFileDropped;

        [Category("[ File Handler ]")]
        [Description("Only get file listing from the drop.")]
        public event FilesDroppedHandler FilesDropped;

        [Category("[ Directory Handler ]")]
        [Description("Get the first directory from the drop.")]
        public event SingleDirectoryDroppedHandler SingleDirectoryDropped;
        [Category("[ Directory Handler ]")]
        [Description("Get a list of directories from the drop.")]
        public event DirectoriesDroppedHandler DirectoriesDropped;

        [Category("[ Both Files & Directories ]")]
        [Description("Get both files and directories from the drop.")]
        public event FilesAndDirectoriesDroppedHandler FilesAndDirectoriesDropped;
        
        [Category("[ Both Files & Directories ]")]
        [Description("Get both files and directories from the drop. Also provide root folder.")]
        public event FilesAndDirectoriesDroppedWithRootHandler FilesAndDirectoriesDroppedWithRoot;


        #region [ Private Variables ]
        private Form _FormDrop;
        private Control _DropContainer;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// The parent form Handle.
        /// </summary>
        [Category("[ DropCatcher ]")]
        [Description("The parent form.")]
        [Browsable(false)]
        public Form FormDrop
        {
            get { return _FormDrop; }
            set 
            {
                _FormDrop = value; 
            }
        }

        /// <summary>
        /// The control to apply the Drag function to. Ie drag this control to move the form.
        /// </summary>
        [Category("[ DropCatcher ]")]
        [Description("The control to apply the Drag function to. Ie drag this control to move the form.")]
        public Control DropContainer
        {
            get { return _DropContainer; }
            set 
            {
                _DropContainer = value;
                bindToDropContainer();
            }
        }

        /// <summary>
        /// Enable or disable the drop.
        /// </summary>
        private bool _AllowDrop = true;

        /// <summary>
        /// Enable or disable the drop.
        /// </summary>
        [Category("[ DropCatcher ]")]
        [Description("Enable or disable the drop.")]
        [DefaultValue(true)]
        public bool AllowDrop
        {
            get { return _AllowDrop; }
            set { _AllowDrop = value; }
        }
        #endregion

        #region [ Constructors ]
        public DropCatcher()
            : this(null)
        {
        }

        public DropCatcher(IContainer container)
        {
            if (container != null)
            {
                container.Add(this);
            }
            InitializeComponent();
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
                        FormDrop = form;

                        if (DropContainer == null)
                        {
                            DropContainer = form;
                        }
                    }
                }
            }
        }
        #endregion     
        
        #region [ ISupportInitialize Members ]
        public void BeginInit()
        {
        }

        public void EndInit()
        {
            bindToDropContainer();
        }

        private void bindToDropContainer()
        {
            if (DropContainer != null)
            {
                bindToDropContainer(DropContainer);

                foreach (Control c in DropContainer.Controls)
                {
                    foreach (Control cInner in c.Controls)
                    {
                        bindToDropContainer(cInner);
                    }

                    bindToDropContainer(c);
                }
            }
        }

        private void bindToDropContainer(Control c)
        {
            //if (c is Form)
            //{
            //    var f = (Form)c;
            //    f.AllowDrop = true;
            //}

            c.AllowDrop = true;
            c.DragEnter += DropContainer_DragEnter;
            c.DragDrop += DropContainer_DragDrop;
        }

        void DropContainer_DragEnter(object sender, DragEventArgs e)
        {
            if (!AllowDrop)
            {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else 
            {
                e.Effect = DragDropEffects.None;
            }
        }

        void DropContainer_DragDrop(object sender, DragEventArgs e)
        {
            if (!AllowDrop)
            {
                return;
            }

            string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);

            List<DirectoryInfo> dirs = new List<DirectoryInfo>();
            List<FileInfo> files = new List<FileInfo>();

            foreach (string p in path)
            {
                if (File.Exists(p))
                {
                    files.Add(new FileInfo(p));
                }
                else if (Directory.Exists(p))
                {
                    dirs.Add(new DirectoryInfo(p));
                }
            }

            // Put it on a timer so that it release the user
            // cursor from the form (ie when you in exlorer you won't see the 
            // [+] cursor while the form is processing the drop, the explorer are free to move around)
            Tmr.Run(() =>
            {
                if (files.Count > 0)
                {
                    files.Sort(new FileInfoNaturalSortComparer());

                    Dlgt.Invoke(SingleFileDropped, files[0]);
                    Dlgt.Invoke(FilesDropped, (object)files.ToArray());
                }

                if (dirs.Count > 0)
                {
                    dirs.Sort(new DirectoryInfoNaturalSortComparer());

                    Dlgt.Invoke(SingleDirectoryDropped, dirs[0]);
                    Dlgt.Invoke(DirectoriesDropped, (object)dirs.ToArray());
                }

                if (files.Count > 0 || dirs.Count > 0)
                {
                    Dlgt.Invoke(FilesAndDirectoriesDropped, dirs.ToArray(), files.ToArray());

                    DirectoryInfo root = GetRoot(files, dirs);
                    Dlgt.Invoke(FilesAndDirectoriesDroppedWithRoot, root, dirs.ToArray(), files.ToArray());
                }
            }, 10);
        }

        private DirectoryInfo GetRoot(List<FileInfo> files, List<DirectoryInfo> dirs)
        {
            if (files == null || dirs == null)
            {
                return null;
            }

            if (files.Count > 0)
            {
                // when drag file & together 
                // the only way is both sare the same parent
                // there is no way you can drag drop file and dir
                // from a diff folder
                return files[0].Directory;
            }

            if (dirs.Count > 0)
            {
                return dirs[0].Parent;
            }

            return null;
        }
        #endregion
    }
}
