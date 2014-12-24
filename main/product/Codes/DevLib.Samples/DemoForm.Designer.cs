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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DemoForm));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node1");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node3");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Node4");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Node5");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Node6");
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
            this.modernRichTextBox1 = new DevLib.ModernUI.Forms.ModernRichTextBox();
            this.modernTabPage3 = new DevLib.ModernUI.Forms.ModernTabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.modernRichTextBoxRequest = new DevLib.ModernUI.Forms.ModernRichTextBox();
            this.modernRichTextBoxResponse = new DevLib.ModernUI.Forms.ModernRichTextBox();
            this.modernButton2 = new DevLib.ModernUI.Forms.ModernButton();
            this.modernTextBoxUri = new DevLib.ModernUI.Forms.ModernTextBox();
            this.modernTabPage4 = new DevLib.ModernUI.Forms.ModernTabPage();
            this.modernTreeView1 = new DevLib.ModernUI.Forms.ModernTreeView();
            this.modernPropertyGrid1 = new DevLib.ModernUI.Forms.ModernPropertyGrid();
            this.modernProgressSpinner1 = new DevLib.ModernUI.Forms.ModernProgressSpinner();
            this.modernLabel1 = new DevLib.ModernUI.Forms.ModernLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.modernTile2 = new DevLib.ModernUI.Forms.ModernTile();
            this.button1 = new System.Windows.Forms.Button();
            this.modernButton3 = new DevLib.ModernUI.Forms.ModernButton();
            this.StatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernStyleManager1)).BeginInit();
            this.modernTabControl1.SuspendLayout();
            this.modernTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernDataGridView1)).BeginInit();
            this.modernTabPage2.SuspendLayout();
            this.modernTabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.modernTabPage4.SuspendLayout();
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
            this.modernTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modernTabControl1.Controls.Add(this.modernTabPage1);
            this.modernTabControl1.Controls.Add(this.modernTabPage2);
            this.modernTabControl1.Controls.Add(this.modernTabPage3);
            this.modernTabControl1.Controls.Add(this.modernTabPage4);
            this.modernTabControl1.Location = new System.Drawing.Point(35, 148);
            this.modernTabControl1.Multiline = true;
            this.modernTabControl1.Name = "modernTabControl1";
            this.modernTabControl1.SelectedIndex = 3;
            this.modernTabControl1.Size = new System.Drawing.Size(606, 438);
            this.modernTabControl1.TabIndex = 11;
            this.modernTabControl1.UseSelectable = true;
            // 
            // modernTabPage1
            // 
            this.modernTabPage1.Controls.Add(this.modernDataGridView1);
            this.modernTabPage1.HorizontalScrollBarSize = 10;
            this.modernTabPage1.Location = new System.Drawing.Point(4, 38);
            this.modernTabPage1.Name = "modernTabPage1";
            this.modernTabPage1.Size = new System.Drawing.Size(598, 396);
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
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.modernDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.modernDataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.modernDataGridView1.Size = new System.Drawing.Size(598, 396);
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
            this.modernTabPage2.Controls.Add(this.modernRichTextBox1);
            this.modernTabPage2.HorizontalScrollBarSize = 10;
            this.modernTabPage2.Location = new System.Drawing.Point(4, 38);
            this.modernTabPage2.Name = "modernTabPage2";
            this.modernTabPage2.Size = new System.Drawing.Size(598, 396);
            this.modernTabPage2.TabIndex = 1;
            this.modernTabPage2.Text = "modernTabPage2";
            this.modernTabPage2.UseHorizontalBarColor = true;
            this.modernTabPage2.UseStyleColors = false;
            this.modernTabPage2.UseVerticalBarColor = true;
            this.modernTabPage2.VerticalScrollBarSize = 10;
            // 
            // modernRichTextBox1
            // 
            this.modernRichTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modernRichTextBox1.Lines = new string[] {
        "modernRichTextBox1"};
            this.modernRichTextBox1.Location = new System.Drawing.Point(26, 29);
            this.modernRichTextBox1.MaxLength = 2147483647;
            this.modernRichTextBox1.Multiline = true;
            this.modernRichTextBox1.Name = "modernRichTextBox1";
            this.modernRichTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.modernRichTextBox1.SelectedText = "";
            this.modernRichTextBox1.Size = new System.Drawing.Size(569, 346);
            this.modernRichTextBox1.TabIndex = 15;
            this.modernRichTextBox1.Text = "modernRichTextBox1";
            this.modernRichTextBox1.UseSelectable = true;
            this.modernRichTextBox1.UseStyleColors = false;
            // 
            // modernTabPage3
            // 
            this.modernTabPage3.Controls.Add(this.splitContainer1);
            this.modernTabPage3.Controls.Add(this.modernButton2);
            this.modernTabPage3.Controls.Add(this.modernTextBoxUri);
            this.modernTabPage3.HorizontalScrollBarSize = 10;
            this.modernTabPage3.Location = new System.Drawing.Point(4, 38);
            this.modernTabPage3.Name = "modernTabPage3";
            this.modernTabPage3.Size = new System.Drawing.Size(598, 396);
            this.modernTabPage3.TabIndex = 2;
            this.modernTabPage3.Text = "modernTabPage3";
            this.modernTabPage3.UseHorizontalBarColor = true;
            this.modernTabPage3.UseStyleColors = false;
            this.modernTabPage3.UseVerticalBarColor = true;
            this.modernTabPage3.VerticalScrollBarSize = 10;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 23);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.modernRichTextBoxRequest);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.modernRichTextBoxResponse);
            this.splitContainer1.Size = new System.Drawing.Size(598, 350);
            this.splitContainer1.SplitterDistance = 175;
            this.splitContainer1.TabIndex = 4;
            // 
            // modernRichTextBoxRequest
            // 
            this.modernRichTextBoxRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modernRichTextBoxRequest.Lines = new string[] {
        "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">",
        "  <s:Header>",
        "    <Action s:mustUnderstand=\"1\" xmlns=\"http://schemas.microsoft.com/ws/2005/05/a" +
            "ddressing/none\">http://ws.cdyne.com/WeatherWS/GetCityWeatherByZIP</Action>",
        "  </s:Header>",
        "  <s:Body>",
        "    <GetCityWeatherByZIP xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmln" +
            "s=\"http://ws.cdyne.com/WeatherWS/\">",
        "      <ZIP>33133</ZIP>",
        "    </GetCityWeatherByZIP>",
        "  </s:Body>",
        "</s:Envelope>"};
            this.modernRichTextBoxRequest.Location = new System.Drawing.Point(0, 0);
            this.modernRichTextBoxRequest.MaxLength = 2147483647;
            this.modernRichTextBoxRequest.Multiline = true;
            this.modernRichTextBoxRequest.Name = "modernRichTextBoxRequest";
            this.modernRichTextBoxRequest.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.modernRichTextBoxRequest.SelectedText = "";
            this.modernRichTextBoxRequest.Size = new System.Drawing.Size(598, 175);
            this.modernRichTextBoxRequest.TabIndex = 0;
            this.modernRichTextBoxRequest.Text = resources.GetString("modernRichTextBoxRequest.Text");
            this.modernRichTextBoxRequest.UseSelectable = true;
            this.modernRichTextBoxRequest.UseStyleColors = false;
            // 
            // modernRichTextBoxResponse
            // 
            this.modernRichTextBoxResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modernRichTextBoxResponse.Lines = new string[] {
        "modernRichTextBox3"};
            this.modernRichTextBoxResponse.Location = new System.Drawing.Point(0, 0);
            this.modernRichTextBoxResponse.MaxLength = 2147483647;
            this.modernRichTextBoxResponse.Multiline = true;
            this.modernRichTextBoxResponse.Name = "modernRichTextBoxResponse";
            this.modernRichTextBoxResponse.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.modernRichTextBoxResponse.SelectedText = "";
            this.modernRichTextBoxResponse.Size = new System.Drawing.Size(598, 171);
            this.modernRichTextBoxResponse.TabIndex = 0;
            this.modernRichTextBoxResponse.Text = "modernRichTextBox3";
            this.modernRichTextBoxResponse.UseSelectable = true;
            this.modernRichTextBoxResponse.UseStyleColors = false;
            // 
            // modernButton2
            // 
            this.modernButton2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.modernButton2.Location = new System.Drawing.Point(0, 373);
            this.modernButton2.Name = "modernButton2";
            this.modernButton2.Size = new System.Drawing.Size(598, 23);
            this.modernButton2.TabIndex = 3;
            this.modernButton2.Text = "modernButton2";
            this.modernButton2.UseSelectable = true;
            this.modernButton2.UseStyleColors = false;
            this.modernButton2.UseVisualStyleBackColor = true;
            this.modernButton2.Click += new System.EventHandler(this.modernButton2_Click);
            // 
            // modernTextBoxUri
            // 
            this.modernTextBoxUri.Dock = System.Windows.Forms.DockStyle.Top;
            this.modernTextBoxUri.Lines = new string[] {
        "http://wsf.cdyne.com/WeatherWS/Weather.asmx"};
            this.modernTextBoxUri.Location = new System.Drawing.Point(0, 0);
            this.modernTextBoxUri.MaxLength = 2147483647;
            this.modernTextBoxUri.Name = "modernTextBoxUri";
            this.modernTextBoxUri.PasswordChar = '\0';
            this.modernTextBoxUri.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.modernTextBoxUri.SelectedText = "";
            this.modernTextBoxUri.Size = new System.Drawing.Size(598, 23);
            this.modernTextBoxUri.TabIndex = 2;
            this.modernTextBoxUri.Text = "http://wsf.cdyne.com/WeatherWS/Weather.asmx";
            this.modernTextBoxUri.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.modernTextBoxUri.UseSelectable = true;
            this.modernTextBoxUri.UseStyleColors = false;
            this.modernTextBoxUri.UseSystemPasswordChar = false;
            this.modernTextBoxUri.WordWrap = true;
            // 
            // modernTabPage4
            // 
            this.modernTabPage4.Controls.Add(this.modernButton3);
            this.modernTabPage4.Controls.Add(this.modernTreeView1);
            this.modernTabPage4.Controls.Add(this.modernPropertyGrid1);
            this.modernTabPage4.HorizontalScrollBarSize = 10;
            this.modernTabPage4.Location = new System.Drawing.Point(4, 38);
            this.modernTabPage4.Name = "modernTabPage4";
            this.modernTabPage4.Size = new System.Drawing.Size(598, 396);
            this.modernTabPage4.TabIndex = 3;
            this.modernTabPage4.Text = "modernTabPage4";
            this.modernTabPage4.UseHorizontalBarColor = true;
            this.modernTabPage4.UseStyleColors = false;
            this.modernTabPage4.UseVerticalBarColor = true;
            this.modernTabPage4.VerticalScrollBarSize = 10;
            // 
            // modernTreeView1
            // 
            this.modernTreeView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernTreeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.modernTreeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.modernTreeView1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernTreeView1.FullRowSelect = true;
            this.modernTreeView1.HideSelection = false;
            this.modernTreeView1.HotTracking = true;
            this.modernTreeView1.Location = new System.Drawing.Point(62, 94);
            this.modernTreeView1.Name = "modernTreeView1";
            treeNode1.Name = "Node1";
            treeNode1.Text = "Node1";
            treeNode2.Name = "Node3";
            treeNode2.Text = "Node3";
            treeNode3.Name = "Node4";
            treeNode3.Text = "Node4";
            treeNode4.Name = "Node2";
            treeNode4.Text = "Node2";
            treeNode5.Name = "Node0";
            treeNode5.Text = "Node0";
            treeNode6.Name = "Node5";
            treeNode6.Text = "Node5";
            treeNode7.Name = "Node6";
            treeNode7.Text = "Node6";
            this.modernTreeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6,
            treeNode7});
            this.modernTreeView1.Size = new System.Drawing.Size(269, 269);
            this.modernTreeView1.TabIndex = 6;
            this.modernTreeView1.UseSelectable = true;
            this.modernTreeView1.UseStyleColors = false;
            // 
            // modernPropertyGrid1
            // 
            this.modernPropertyGrid1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernPropertyGrid1.CategoryForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernPropertyGrid1.CommandsActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.modernPropertyGrid1.CommandsBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernPropertyGrid1.CommandsDisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.modernPropertyGrid1.CommandsForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernPropertyGrid1.CommandsLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernPropertyGrid1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernPropertyGrid1.HelpBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernPropertyGrid1.HelpForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.modernPropertyGrid1.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            this.modernPropertyGrid1.Location = new System.Drawing.Point(379, 65);
            this.modernPropertyGrid1.Name = "modernPropertyGrid1";
            this.modernPropertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.modernPropertyGrid1.Size = new System.Drawing.Size(198, 269);
            this.modernPropertyGrid1.TabIndex = 5;
            this.modernPropertyGrid1.UseSelectable = true;
            this.modernPropertyGrid1.ViewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.modernPropertyGrid1.ViewForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
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
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(657, 559);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // modernButton3
            // 
            this.modernButton3.Location = new System.Drawing.Point(492, 340);
            this.modernButton3.Name = "modernButton3";
            this.modernButton3.Size = new System.Drawing.Size(85, 23);
            this.modernButton3.TabIndex = 19;
            this.modernButton3.Text = "modernButton3";
            this.modernButton3.UseSelectable = true;
            this.modernButton3.UseStyleColors = false;
            this.modernButton3.UseVisualStyleBackColor = true;
            this.modernButton3.Click += new System.EventHandler(this.modernButton3_Click);
            // 
            // DemoForm
            // 
            this.AcceptButton = this.modernButton1;
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
            this.Controls.Add(this.button1);
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
            this.Controls.SetChildIndex(this.button1, 0);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modernStyleManager1)).EndInit();
            this.modernTabControl1.ResumeLayout(false);
            this.modernTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modernDataGridView1)).EndInit();
            this.modernTabPage2.ResumeLayout(false);
            this.modernTabPage3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.modernTabPage4.ResumeLayout(false);
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
        private System.Windows.Forms.Button button1;
        private ModernTabPage modernTabPage3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ModernRichTextBox modernRichTextBoxRequest;
        private ModernRichTextBox modernRichTextBoxResponse;
        private ModernButton modernButton2;
        private ModernTextBox modernTextBoxUri;
        private ModernTabPage modernTabPage4;
        private ModernPropertyGrid modernPropertyGrid1;
        private ModernButton modernButton3;
        private ModernTreeView modernTreeView1;
    }
}