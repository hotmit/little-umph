using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LittleUmph.GUI.Forms
{
    public partial class FrmInput : Form
    {
        /// <summary>
        /// Gets or sets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public string Prompt
        {
            get
            {
                return lblPrompt.Text;
            }
            set
            {
                lblPrompt.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the inital input.
        /// </summary>
        /// <value>
        /// The input.
        /// </value>
        public string Input
        {
            get
            {
                return txtInput.Text;
            }
            set
            {
                txtInput.Text = value;
                txtInput.SelectAll();
            }
        }

        public FrmInput()
        {
            InitializeComponent();
        }
    }
}
