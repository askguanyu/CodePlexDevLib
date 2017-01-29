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
using DevLib.ExtensionMethods;
using DevLib.Web;
using System.Threading;

namespace DevLib.Samples
{
    public partial class DemoForm : ModernForm
    {
        public DemoForm()
        {
            InitializeComponent();

            this.SizeChanged += new EventHandler(DemoForm_SizeChanged);

            modernPropertyGrid1.SelectedObject = new List<Person> {  new Person()  };
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
                    ModernTaskWindow.Show("SubControl in TaskWindow", long.Parse(modernTextBox1.Text) * 1000);
                    break;
                default:
                    break;
            }
        }

        private void modernButton2_Click(object sender, EventArgs e)
        {
            try
            {
                modernRichTextBoxResponse.Text = SoapClient.SendRequestString(modernTextBoxUri.Text, modernRichTextBoxRequest.Text, "a", "b").Content;
            }
            catch (Exception ex)
            {
                modernRichTextBoxResponse.Text = ex.ToString();
            }
        }

        private void modernButton3_Click(object sender, EventArgs e)
        {
            var obj = modernPropertyGrid1.SelectedObject;
        }

        private void modernPropertyGrid1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void modernPropertyGrid1_Validating(object sender, CancelEventArgs e)
        {

        }

        private void modernPropertyGrid1_MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void modernPropertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void modernPropertyGrid1_Layout(object sender, LayoutEventArgs e)
        {

        }

        private void modernPropertyGrid1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void modernPropertyGrid1_ChangeUICues(object sender, UICuesEventArgs e)
        {

        }

        private void modernPropertyGrid1_Enter(object sender, EventArgs e)
        {

        }

        private void modernPropertyGrid1_Leave(object sender, EventArgs e)
        {

        }

        private void modernPropertyGrid1_PropertyTabChanged(object s, PropertyTabChangedEventArgs e)
        {

        }

        private void modernPropertyGrid1_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.performanceChart1.AddValue(decimal.Parse(textBox1.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.performanceChart1.AddValue(new Random().Next(0, 100));
        }

        private void modernButton4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

            Thread.Sleep(3000);

            ModernMessageBox.Show("Test", "Title", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //modernRichTextBox1.Text = @"d:\a.txt".ReadTextFile();
            this.ShowInTaskbar = true;
        }

        private void modernButton5_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
        }

        void DemoForm_SizeChanged(object sender, EventArgs e)
        {

        }
    }
}
