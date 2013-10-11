namespace LittleUmph.GUI.Controls
{
    partial class SignaturePad
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            try
            {
                _graphicHandler.Dispose();
                _signatureImage.Dispose();
            }
            catch 
            {
                
            }
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SignaturePad
            // 
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SignaturePad_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SignaturePad_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SignaturePad_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
