using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace LittleUmph.GUI.Controls
{
    /// <summary>
    /// Signature capture control.
    /// </summary>
    public partial class SignaturePad : PictureBox, ISupportInitialize
    {
        #region [ Private Variables ]
        private Bitmap _signatureImage;
        private Graphics _graphicHandler;
        private Point _lastMouseCoord;

        private List<Point> _path;
        private Rectangle _border;
        private bool _mouseDown = false;
        
        private Color _penColor = Color.Black;
        private float _PenSize = 2;
        private bool _signed;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Whether the user has signed the pad.
        /// </summary>
        /// <value>
        /// <c>true</c> if signed; otherwise, <c>false</c>.</value>
        public bool Signed
        {
            get
            {
                return _signed;
            }
            private set
            {
                bool oldValue = _signed;
                _signed = value;

                #region [ Send Signature Captured Event ]
                if (_signed != oldValue)
                {
                    if (SigStatusChanged != null)
                    {
                        SigStatusChanged(this);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Gets or sets bytes value of the image.
        /// Note: CF database image field is only limited to 8000 byte in length.
        /// </summary>
        /// <value>The bytes.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] Bytes
        {
            get
            {
                if (Image == null)
                {
                    return null;
                }
                return Img.ToBytes(SignatureImage);
            }
            set
            {
                _signatureImage = (Bitmap)Img.FromBytes(value);
            }
        }

        /// <summary>
        /// Gets or sets the compressed JPEG image bytes array.
        /// </summary>
        /// <value>The JPEG bytes.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] JpegBytes
        {
            get
            {                
                using (MemoryStream ms = new MemoryStream())
                {
                    // this does work, it doesn't compress the image :(
                    Image.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            set
            {
                Bytes = value;
            }
        }


        /// <summary>
        /// Gets the signature image.
        /// </summary>
        /// <value>The signature image.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image SignatureImage
        {
            get { return (Image)_signatureImage; }
        }

        /// <summary>
        /// Gets or sets the thickness of the pen stroke.
        /// </summary>
        /// <value>The size of the pen.</value>
        public float PenThickness
        {
            get { return _PenSize; }
            set { _PenSize = value; }
        }

        /// <summary>
        /// The paint use for signature capturing.
        /// </summary>
        public Color PenColor
        {
            get { return _penColor; }
            set { _penColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the border.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor { get; set; }

        /// <summary>
        /// Gets or sets the border thickness.
        /// </summary>
        /// <value>The border thickness.</value>
        public float BorderThickness { get; set; }

        /// <summary>
        /// Gets or sets the text display before the user signed on the signature pad.
        /// </summary>
        /// <value>The display text.</value>
        public string DisplayText { get; set; }
        #endregion

        #region [ Events & Delegates ]
        /// <summary>
        /// Event handler for SignatureCaptured event.
        /// </summary>
        public delegate void SigStatusChangedHandler(SignaturePad pad);

        /// <summary>
        /// Occurs when the signature is captured.
        /// </summary>
        public event SigStatusChangedHandler SigStatusChanged;
        #endregion

        #region [ Constructors ]
        public SignaturePad()
        {
            InitializeComponent();
        }
        #endregion

        #region [ Clear ]
        /// <summary>
        /// Clears the signature from the pad.
        /// </summary>
        public void Clear()
        {
            initializeImage();
        }
        #endregion

        #region [ Helper ]
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (BorderThickness > 0)
            {
                pe.Graphics.DrawRectangle(new Pen(BorderColor, BorderThickness), _border);
            }
        }

        private void initializeImage()
        {
            #region [ Dispose Old Vars ]
            if (_graphicHandler != null)
            {
                try
                {
                    _graphicHandler.Dispose();
                    _signatureImage.Dispose();
                }
                catch (Exception xpt)
                {
                    Console.WriteLine(xpt.Message);
                }
            }
            #endregion

            _signatureImage = new Bitmap(Size.Width, Size.Height);
            _graphicHandler = Graphics.FromImage(_signatureImage);

            _graphicHandler.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
            _lastMouseCoord = new Point();

            Signed = false;
            Image = GetSignHereImage();
        }

        /// <summary>
        /// Returns the "Sign Here" image.
        /// </summary>
        /// <returns></returns>
        private Image GetSignHereImage()
        {
            Bitmap bitmap = new Bitmap(Size.Width, Size.Height);
            try
            {
                Graphics graphics = Graphics.FromImage(bitmap);

                graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);

                Font font = new Font("Tahoma", 14, FontStyle.Bold);
                SolidBrush brush = new SolidBrush(PenColor);
                Rectangle rec = new Rectangle(0, 0, Width, Height);

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                DisplayText = Str.IsEmpty(DisplayText) ? "Please Sign Here" : DisplayText;
                graphics.DrawString(DisplayText, font, brush, rec, sf);

                graphics.Dispose();

            }
            catch (Exception xpt)
            {
                Console.WriteLine(xpt.Message);
            }
            return bitmap;
        }
        #endregion

        #region [ ISupportInitialize Members ]
        public void BeginInit()
        {            
        }

        public void EndInit()
        {
            int topOffset = Convert.ToInt32(Math.Ceiling(BorderThickness));
            int bottomOffset = topOffset * 2;
            _border = new Rectangle(topOffset, topOffset, Width - bottomOffset, Height - bottomOffset);

            initializeImage();
        }        
        #endregion

        #region [ Movement Capture ]
        private void SignaturePad_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Signed)
            {
                Image = _signatureImage;
                Signed = true;
            }

            _path = new List<Point>();
            _lastMouseCoord.X = e.X;
            _lastMouseCoord.Y = e.Y;
            _path.Add(_lastMouseCoord);

            _mouseDown = true;
        }

        private void SignaturePad_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                Pen signaturePen = new Pen(PenColor, PenThickness);

                _lastMouseCoord.X = e.X;
                _lastMouseCoord.Y = e.Y;
                _path.Add(_lastMouseCoord);

                _graphicHandler.DrawLines(signaturePen, _path.ToArray());
                Invalidate(_border);
            }
        }

        private void SignaturePad_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            _path = null;
        }
        #endregion  
    }
}
