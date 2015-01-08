using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LittleUmph.GUI.Controls
{
    #region [ LightboxModal Form ]
    /// <summary>
    /// Transparent form to use with the LightBox class
    /// </summary>
    /// <see cref="Lightbox"/>
    public partial class LightboxModal : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public LightboxModal()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();

            InitializeComponent();
        }
    } 
    #endregion

    ///Todo: clean this up, the code is hurt to look at :)

    /// <summary>
    /// Show a transparent screen over the user interface to 
    /// give the pop up dialog more focus
    /// </summary>
    public class Lightbox
    {
        #region [ Properties ]
        private static LightboxModal _preloadLightbox;
        private static double _modalOpacity = 0.70;

        /// <summary>
        /// Default modal opacity (intialized with 0.70)
        /// </summary>
        public static double ModalOpacity
        {
            get { return _modalOpacity; }
            set { _modalOpacity = value; }
        }

        private static Color _ModalColor = Color.Black;

        /// <summary>
        /// The modal background color (default as black)
        /// </summary>
        public static Color ModalColor
        {
            get { return _ModalColor; }
            set { _ModalColor = value; }
        } 
        #endregion

        #region [ ShowModal Functions ]
        /// <summary>
        /// Show the form above the modal dialog box
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="f"></param>
        /// <param name="pos"></param>
        /// <param name="opacity"></param>
        public static DialogResult ShowModal(Form parent, Form f, Rectangle pos, double opacity)
        {
            if (_preloadLightbox == null)
            {
                _preloadLightbox = new LightboxModal();
            }
            _preloadLightbox.Location = pos.Location;
            _preloadLightbox.Width = pos.Width;
            _preloadLightbox.Height = pos.Height;
            _preloadLightbox.Opacity = opacity;
            _preloadLightbox.BackColor = ModalColor;
            _preloadLightbox.TopMost = true;
            _preloadLightbox.Show();
            _preloadLightbox.BringToFront();
            
            f.StartPosition = FormStartPosition.CenterParent;
            DialogResult result = f.ShowDialog(_preloadLightbox);

            _preloadLightbox.Hide();
            return result;
        }

        /// <summary>
        /// Show the form above the modal box
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="f"></param>
        /// <param name="pos"></param>
        public static DialogResult ShowModal(Form parent, Form f, Rectangle pos)
        {
            return ShowModal(parent, f, pos, ModalOpacity);
        }

        /// <summary>
        /// Show the form above the modal box
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="f"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static DialogResult ShowModal(Form parent, Form f, double opacity)
        {
            Rectangle pos = parent.Bounds;
            return ShowModal(parent, f, pos, opacity);
        }

        /// <summary>
        /// Show the form above the modal box
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="f"></param>
        public static DialogResult ShowModal(Form parent, Form f)
        {
            Rectangle pos = parent.Bounds;
            return ShowModal(parent, f, pos);
        }      
        #endregion   

        #region [ Old Function ]
        ///// <summary>
        ///// ShowModal the form above the modal box
        ///// </summary>
        ///// <param name="f"></param>
        //public static void ShowModal(Form f)
        //{
        //    Lightbox md = new Lightbox();
        //    md.StartPosition = FormStartPosition.CenterScreen;
        //    md.Opacity = ModalOpacity;
        //    md.Show();

        //    f.StartPosition = FormStartPosition.CenterParent;
        //    f.ShowDialog(md);

        //    md.Close();
        //}

        ///// <summary>
        ///// ShowModal the form above the modal box
        ///// </summary>
        ///// <param name="f"></param>
        ///// <param name="pos"></param>
        ///// <example>.ShowModalDimmer(modalForm, this.Bounds) //this -> is the parent form</example>
        //public static void ShowModal(Form f, Rectangle pos)
        //{
        //    Lightbox md = new Lightbox();
        //    md.Location = pos.Location;
        //    md.Width = pos.Width;            
        //    md.Height = pos.Height;
        //    md.Opacity = ModalOpacity;
        //    md.Show();

        //    f.StartPosition = FormStartPosition.CenterParent;
        //    f.ShowDialog(md);

        //    md.Close();
        //}
        #endregion
    }
}