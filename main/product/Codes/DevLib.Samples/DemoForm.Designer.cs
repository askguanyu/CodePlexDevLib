using DevLib.ModernUI.ComponentModel;
using DevLib.ModernUI.Forms;
namespace DevLib.Samples
{
    partial class DemoForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.modernStyleManager1 = new DevLib.ModernUI.ComponentModel.ModernStyleManager(this.components);
            this.modernToggle1 = new DevLib.ModernUI.Forms.ModernToggle();
            this.modernButton1 = new DevLib.ModernUI.Forms.ModernButton();
            this.modernDateTime1 = new DevLib.ModernUI.Forms.ModernDateTimePicker();
            this.modernComboBox1 = new DevLib.ModernUI.Forms.ModernComboBox();
            this.modernTextBox1 = new DevLib.ModernUI.Forms.ModernTextBox();
            this.modernTabControl1 = new DevLib.ModernUI.Forms.ModernTabControl();
            this.modernTabPage1 = new DevLib.ModernUI.Forms.ModernTabPage();
            this.modernDataGridView1 = new DevLib.ModernUI.Forms.ModernDataGridView();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modernTabPage2 = new DevLib.ModernUI.Forms.ModernTabPage();
            this.modernProgressSpinner1 = new DevLib.ModernUI.Forms.ModernProgressSpinner();
            this.modernLabel1 = new DevLib.ModernUI.Forms.ModernLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.modernTile2 = new DevLib.ModernUI.Forms.ModernTile();
            this.modernRichTextBox1 = new DevLib.ModernUI.Forms.ModernRichTextBox();
            this.modernTextBox2 = new DevLib.ModernUI.Forms.ModernTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernStyleManager1)).BeginInit();
            this.modernTabControl1.SuspendLayout();
            this.modernTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernDataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusStrip
            // 
            this.StatusStrip.Controls.Add(this.label1);
            this.StatusStrip.Location = new System.Drawing.Point(0, 592);
            this.StatusStrip.Size = new System.Drawing.Size(732, 20);
            // 
            // modernStyleManager1
            // 
            this.modernStyleManager1.ColorStyle = DevLib.ModernUI.Forms.ModernColorStyle.Pink;
            this.modernStyleManager1.Owner = this;
            this.modernStyleManager1.ThemeStyle = DevLib.ModernUI.Forms.ModernThemeStyle.Light;
            // 
            // modernToggle1
            // 
            this.modernToggle1.AutoSize = true;
            this.modernToggle1.FontWeight = DevLib.ModernUI.Drawing.ModernFontWeight.Light;
            this.modernToggle1.Location = new System.Drawing.Point(247, 125);
            this.modernToggle1.Name = "modernToggle1";
            this.modernToggle1.Size = new System.Drawing.Size(80, 17);
            this.modernToggle1.StatusTextRightToLeft = true;
            this.modernToggle1.TabIndex = 4;
            this.modernToggle1.Text = "Off";
            this.modernToggle1.UseSelectable = true;
            this.modernToggle1.UseStyleColors = false;
            this.modernToggle1.UseVisualStyleBackColor = true;
            this.modernToggle1.CheckedChanged += new System.EventHandler(this.modernToggle1_CheckedChanged);
            // 
            // modernButton1
            // 
            this.modernButton1.Location = new System.Drawing.Point(45, 79);
            this.modernButton1.Name = "modernButton1";
            this.modernButton1.Size = new System.Drawing.Size(175, 44);
            this.modernButton1.TabIndex = 5;
            this.modernButton1.Text = "modernButton1";
            this.modernButton1.UseSelectable = true;
            this.modernButton1.UseStyleColors = false;
            this.modernButton1.UseVisualStyleBackColor = true;
            this.modernButton1.Click += new System.EventHandler(this.modernButton1_Click);
            // 
            // modernDateTime1
            // 
            this.modernDateTime1.FontSize = DevLib.ModernUI.Drawing.ModernFontSize.Small;
            this.modernDateTime1.FontWeight = DevLib.ModernUI.Drawing.ModernFontWeight.Light;
            this.modernDateTime1.Location = new System.Drawing.Point(108, 59);
            this.modernDateTime1.MinimumSize = new System.Drawing.Size(0, 29);
            this.modernDateTime1.Name = "modernDateTime1";
            this.modernDateTime1.Size = new System.Drawing.Size(200, 29);
            this.modernDateTime1.TabIndex = 0;
            this.modernDateTime1.UseSelectable = true;
            this.modernDateTime1.UseStyleColors = false;
            // 
            // modernComboBox1
            // 
            this.modernComboBox1.FontSize = DevLib.ModernUI.Drawing.ModernFontSize.Medium;
            this.modernComboBox1.FormattingEnabled = true;
            this.modernComboBox1.ItemHeight = 23;
            this.modernComboBox1.Location = new System.Drawing.Point(127, 68);
            this.modernComboBox1.Name = "modernComboBox1";
            this.modernComboBox1.Size = new System.Drawing.Size(196, 29);
            this.modernComboBox1.TabIndex = 0;
            this.modernComboBox1.UseSelectable = true;
            this.modernComboBox1.UseStyleColors = false;
            // 
            // modernTextBox1
            // 
            this.modernTextBox1.Lines = new string[] {
        "3"};
            this.modernTextBox1.Location = new System.Drawing.Point(247, 84);
            this.modernTextBox1.MaxLength = 32767;
            this.modernTextBox1.Name = "modernTextBox1";
            this.modernTextBox1.PasswordChar = '\0';
            this.modernTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.modernTextBox1.SelectedText = "";
            this.modernTextBox1.Size = new System.Drawing.Size(123, 23);
            this.modernTextBox1.TabIndex = 9;
            this.modernTextBox1.Text = "3";
            this.modernTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.modernTextBox1.UseSelectable = true;
            this.modernTextBox1.UseStyleColors = false;
            this.modernTextBox1.UseSystemPasswordChar = false;
            this.modernTextBox1.WordWrap = true;
            // 
            // modernTabControl1
            // 
            this.modernTabControl1.Controls.Add(this.modernTabPage1);
            this.modernTabControl1.Controls.Add(this.modernTabPage2);
            this.modernTabControl1.Location = new System.Drawing.Point(35, 148);
            this.modernTabControl1.Multiline = true;
            this.modernTabControl1.Name = "modernTabControl1";
            this.modernTabControl1.SelectedIndex = 0;
            this.modernTabControl1.Size = new System.Drawing.Size(83, 438);
            this.modernTabControl1.TabIndex = 11;
            this.modernTabControl1.UseSelectable = true;
            // 
            // modernTabPage1
            // 
            this.modernTabPage1.Controls.Add(this.modernDataGridView1);
            this.modernTabPage1.HorizontalScrollBarSize = 10;
            this.modernTabPage1.Location = new System.Drawing.Point(4, 72);
            this.modernTabPage1.Name = "modernTabPage1";
            this.modernTabPage1.Size = new System.Drawing.Size(75, 362);
            this.modernTabPage1.TabIndex = 0;
            this.modernTabPage1.Text = "modernTabPage1";
            this.modernTabPage1.UseHorizontalBarColor = true;
            this.modernTabPage1.UseStyleColors = false;
            this.modernTabPage1.UseVerticalBarColor = true;
            this.modernTabPage1.VerticalScrollBarSize = 10;
            // 
            // modernDataGridView1
            // 
            this.modernDataGridView1.AllowUserToResizeRows = false;
            this.modernDataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernDataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.modernDataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.modernDataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.modernDataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.modernDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.modernDataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column4,
            this.Column5,
            this.Column6});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.modernDataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.modernDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modernDataGridView1.EnableHeadersVisualStyles = false;
            this.modernDataGridView1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.modernDataGridView1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernDataGridView1.Location = new System.Drawing.Point(0, 0);
            this.modernDataGridView1.Name = "modernDataGridView1";
            this.modernDataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.modernDataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.modernDataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.modernDataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.modernDataGridView1.Size = new System.Drawing.Size(75, 362);
            this.modernDataGridView1.TabIndex = 2;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Column4";
            this.Column4.Name = "Column4";
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Column5";
            this.Column5.Name = "Column5";
            // 
            // Column6
            // 
            this.Column6.HeaderText = "Column6";
            this.Column6.Name = "Column6";
            // 
            // modernTabPage2
            // 
            this.modernTabPage2.HorizontalScrollBarSize = 10;
            this.modernTabPage2.Location = new System.Drawing.Point(4, 72);
            this.modernTabPage2.Name = "modernTabPage2";
            this.modernTabPage2.Size = new System.Drawing.Size(75, 362);
            this.modernTabPage2.TabIndex = 1;
            this.modernTabPage2.Text = "modernTabPage2";
            this.modernTabPage2.UseHorizontalBarColor = true;
            this.modernTabPage2.UseStyleColors = false;
            this.modernTabPage2.UseVerticalBarColor = true;
            this.modernTabPage2.VerticalScrollBarSize = 10;
            // 
            // modernProgressSpinner1
            // 
            this.modernProgressSpinner1.Location = new System.Drawing.Point(503, 84);
            this.modernProgressSpinner1.Minimum = 1;
            this.modernProgressSpinner1.Name = "modernProgressSpinner1";
            this.modernProgressSpinner1.Size = new System.Drawing.Size(65, 63);
            this.modernProgressSpinner1.TabIndex = 12;
            this.modernProgressSpinner1.Text = "modernProgressSpinner1";
            this.modernProgressSpinner1.UseSelectable = true;
            this.modernProgressSpinner1.UseStyleColors = false;
            // 
            // modernLabel1
            // 
            this.modernLabel1.AutoSize = true;
            this.modernLabel1.Location = new System.Drawing.Point(23, 1);
            this.modernLabel1.Name = "modernLabel1";
            this.modernLabel1.Size = new System.Drawing.Size(94, 19);
            this.modernLabel1.TabIndex = 3;
            this.modernLabel1.Text = "modernLabel1";
            this.modernLabel1.UseStyleColors = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(45, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // modernTile2
            // 
            this.modernTile2.ActiveControl = null;
            this.modernTile2.Location = new System.Drawing.Point(405, 84);
            this.modernTile2.Name = "modernTile2";
            this.modernTile2.ShowTileCount = true;
            this.modernTile2.Size = new System.Drawing.Size(75, 68);
            this.modernTile2.TabIndex = 14;
            this.modernTile2.Text = "modernTile2";
            this.modernTile2.TileCount = 8;
            this.modernTile2.UseSelectable = true;
            this.modernTile2.UseStyleColors = false;
            // 
            // modernRichTextBox1
            // 
            this.modernRichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modernRichTextBox1.Lines = new string[] {
        "modernRichTextBox1"};
            this.modernRichTextBox1.Location = new System.Drawing.Point(0, 0);
            this.modernRichTextBox1.MaxLength = 2147483647;
            this.modernRichTextBox1.Multiline = true;
            this.modernRichTextBox1.Name = "modernRichTextBox1";
            this.modernRichTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.modernRichTextBox1.SelectedText = "";
            this.modernRichTextBox1.Size = new System.Drawing.Size(480, 306);
            this.modernRichTextBox1.TabIndex = 15;
            this.modernRichTextBox1.Text = "modernRichTextBox1";
            this.modernRichTextBox1.UseSelectable = true;
            this.modernRichTextBox1.UseStyleColors = false;
            // 
            // modernTextBox2
            // 
            this.modernTextBox2.Lines = new string[] {
        "modernTextBox2"};
            this.modernTextBox2.Location = new System.Drawing.Point(298, 171);
            this.modernTextBox2.MaxLength = 2147483647;
            this.modernTextBox2.Name = "modernTextBox2";
            this.modernTextBox2.PasswordChar = '\0';
            this.modernTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.modernTextBox2.SelectedText = "";
            this.modernTextBox2.Size = new System.Drawing.Size(182, 46);
            this.modernTextBox2.TabIndex = 17;
            this.modernTextBox2.Text = "modernTextBox2";
            this.modernTextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.modernTextBox2.UseSelectable = true;
            this.modernTextBox2.UseStyleColors = false;
            this.modernTextBox2.UseSystemPasswordChar = false;
            this.modernTextBox2.WordWrap = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(657, 559);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(220, 237);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.modernRichTextBox1);
            this.splitContainer1.Size = new System.Drawing.Size(509, 306);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 19;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackImage = global::DevLib.Samples.Properties.Resources.VS_logo;
            this.BackImageMaxSize = 40;
            this.BackImagePadding = new System.Windows.Forms.Padding(150, 16, 0, 0);
            this.ClientSize = new System.Drawing.Size(752, 612);
            this.ColorStyle = DevLib.ModernUI.Forms.ModernColorStyle.Pink;
            this.ControlBox = false;
            this.ControlBoxUseCustomBackColor = true;
            this.ControlBoxUseCustomForeColor = true;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.modernTextBox2);
            this.Controls.Add(this.modernTile2);
            this.Controls.Add(this.modernProgressSpinner1);
            this.Controls.Add(this.modernTabControl1);
            this.Controls.Add(this.modernTextBox1);
            this.Controls.Add(this.modernButton1);
            this.Controls.Add(this.modernToggle1);
            this.Name = "DemoForm";
            this.ShowBorder = false;
            this.ShowStatusStrip = true;
            this.StyleManager = this.modernStyleManager1;
            this.Text = "DemoForm";
            this.Controls.SetChildIndex(this.modernToggle1, 0);
            this.Controls.SetChildIndex(this.modernButton1, 0);
            this.Controls.SetChildIndex(this.modernTextBox1, 0);
            this.Controls.SetChildIndex(this.modernTabControl1, 0);
            this.Controls.SetChildIndex(this.modernProgressSpinner1, 0);
            this.Controls.SetChildIndex(this.StatusStrip, 0);
            this.Controls.SetChildIndex(this.modernTile2, 0);
            this.Controls.SetChildIndex(this.modernTextBox2, 0);
            this.Controls.SetChildIndex(this.button1, 0);
            this.Controls.SetChildIndex(this.splitContainer1, 0);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernStyleManager1)).EndInit();
            this.modernTabControl1.ResumeLayout(false);
            this.modernTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modernDataGridView1)).EndInit();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ModernStyleManager modernStyleManager1;
        private ModernToggle modernToggle1;
        private ModernButton modernButton1;


        private ModernDateTimePicker modernDateTime1;

        private ModernComboBox modernComboBox1;
        private ModernTextBox modernTextBox1;
        private ModernTabControl modernTabControl1;
        private ModernTabPage modernTabPage1;
        private ModernDataGridView modernDataGridView1;
        private ModernProgressSpinner modernProgressSpinner1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private ModernTabPage modernTabPage2;
        private ModernLabel modernLabel1;
        private System.Windows.Forms.Label label1;
        private ModernTile modernTile2;
        private ModernRichTextBox modernRichTextBox1;
        private ModernTextBox modernTextBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}