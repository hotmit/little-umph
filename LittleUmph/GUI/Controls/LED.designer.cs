namespace LittleUmph.GUI.Controls
{
    partial class LED
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
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picLED = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picLED)).BeginInit();
            this.SuspendLayout();
            // 
            // picLED
            // 
            this.picLED.BackgroundImage = global::LittleUmph.Properties.Resources.LEDOff;
            this.picLED.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picLED.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picLED.Location = new System.Drawing.Point(0, 0);
            this.picLED.Name = "picLED";
            this.picLED.Size = new System.Drawing.Size(50, 50);
            this.picLED.TabIndex = 0;
            this.picLED.TabStop = false;
            // 
            // LED
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picLED);
            this.Name = "LED";
            this.Size = new System.Drawing.Size(50, 50);
            ((System.ComponentModel.ISupportInitialize)(this.picLED)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picLED;
    }
}
