using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevLib.ModernUI.Forms;
using DevLib.ModernUI.ComponentModel;

namespace DevLib.Samples
{
    public partial class DemoForm : ModernForm
    {
        public DemoForm()
        {
            InitializeComponent();
            
            
        }

        private void modernToggle1_CheckedChanged(object sender, EventArgs e)
        {
           this.StyleManager.ThemeStyle = modernToggle1.Checked ? ModernThemeStyle.Dark : ModernThemeStyle.Light;
           this.modernProgressSpinner1.Spinning = modernToggle1.Checked;
        }

        private void modernTabPage1_Click(object sender, EventArgs e)
        {
            
        }

        private void modernButton1_Click(object sender, EventArgs e)
        {
            switch (ModernMessageBox.Show("Start a ModernTaskWindow?", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk))
            {
                case DialogResult.Abort:
                case DialogResult.Cancel:
                case DialogResult.Ignore:
                case DialogResult.No:
                case DialogResult.None:
                    break;
                case DialogResult.OK:
                case DialogResult.Retry:
                case DialogResult.Yes:
                    ModernTaskWindow.Show("SubControl in TaskWindow", int.Parse(modernTextBox1.Text));
                    break;
                default:
                    break;
            }
        }
    }
}
