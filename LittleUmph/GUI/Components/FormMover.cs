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

namespace LittleUmph.GUI.Components
{
    [ToolboxBitmap(typeof(FormMover), "Images.FormMover.png")]
    public partial class FormMover : Component, ISupportInitialize
    {
        #region [ PInvoke Functions ]
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        #endregion

        #region [ Private Variables ]
        private Form _movingForm;
        private Control _mover;
        private bool _flexGrabber = true;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// The parent form Handle.
        /// </summary>
        [Category("[ FormMover ]")]
        [Description("The parent form.")]
        [Browsable(false)]
        public Form MovingForm
        {
            get { return _movingForm; }
            set 
            {
                _movingForm = value; 
            }
        }

        /// <summary>
        /// The control to apply the Drag function to. Ie drag this control to move the form.
        /// </summary>
        [Category("[ FormMover ]")]
        [Description("The control to apply the Drag function to. Ie drag this control to move the form.")]
        public Control Mover
        {
            get { return _mover; }
            set 
            {
                if (_mover != null)
                {
                    _mover.MouseDown -= mover_MouseDown;
                
                    foreach (Control c in Mover.Controls)
                    {
                        foreach (Control cInner in c.Controls)
                        {
                            cInner.MouseDown -= mover_MouseDown;
                        }
                        c.MouseDown -= mover_MouseDown;
                    }
                }

                _mover = value;
                bindToMover();
            }
        }


       
        /// <summary>
        /// Allow moving the form by grabbing anywhere inside the "Mover" control instead of just on the control it self.
        /// </summary>
        [Category("[ FormMover ]")]
        [Description("Allow moving the form by grabbing anywhere inside the \"Mover\" control instead of just on the control it self.")]
        [DefaultValue(true)]
        public bool FlexGrabber
        {
            get { return _flexGrabber; }
            set { _flexGrabber = value; }
        }

        void mover_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(MovingForm.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        #region [ Constructors ]
        public FormMover()
            : this(null)
        {
        }

        public FormMover(IContainer container)
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
                        MovingForm = form;

                        if (Mover == null)
                        {
                            Mover = form;
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
            bindToMover();
        }

        private void bindToMover()
        {
            if (Mover != null)
            {
                Mover.MouseDown += mover_MouseDown;

                if (FlexGrabber)
                {
                    foreach (Control c in Mover.Controls)
                    {
                        foreach (Control cInner in c.Controls)
                        {
                            if (cInner is Panel || cInner is Label || cInner is GroupBox || cInner is UserControl)
                            {
                                cInner.MouseDown += mover_MouseDown;
                            }
                        }

                        if (c is Panel || c is Label || c is GroupBox || c is UserControl)
                        {
                            c.MouseDown += mover_MouseDown;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
