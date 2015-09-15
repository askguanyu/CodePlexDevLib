namespace DevLib.WinForms
{
    partial class WinFormRibbon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinFormRibbon));
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.RibbonTabContainer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ribbonPageFile = new System.Windows.Forms.ToolStrip();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ribbonPageHome = new System.Windows.Forms.ToolStrip();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ribbonPageAbout = new System.Windows.Forms.ToolStrip();
            this.RibbonPanel = new System.Windows.Forms.Panel();
            this.toolStripButtonFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonHome = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAbout = new System.Windows.Forms.ToolStripButton();
            this.statusStripMain.SuspendLayout();
            this.RibbonTabContainer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.ribbonPageFile.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.ribbonPageHome.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.ribbonPageAbout.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStripMain.Location = new System.Drawing.Point(0, 540);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(784, 22);
            this.statusStripMain.TabIndex = 2;
            this.statusStripMain.Text = "statusStripMain";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // RibbonTabContainer
            // 
            this.RibbonTabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RibbonTabContainer.Controls.Add(this.tabPage1);
            this.RibbonTabContainer.Controls.Add(this.tabPage2);
            this.RibbonTabContainer.Controls.Add(this.tabPage3);
            this.RibbonTabContainer.ItemSize = new System.Drawing.Size(65, 20);
            this.RibbonTabContainer.Location = new System.Drawing.Point(0, 0);
            this.RibbonTabContainer.Name = "RibbonTabContainer";
            this.RibbonTabContainer.SelectedIndex = 0;
            this.RibbonTabContainer.Size = new System.Drawing.Size(785, 100);
            this.RibbonTabContainer.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.RibbonTabContainer.TabIndex = 3;
            this.RibbonTabContainer.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.RibbonTabContainer_Selecting);
            this.RibbonTabContainer.Selected += new System.Windows.Forms.TabControlEventHandler(this.RibbonTabContainer_Selected);
            this.RibbonTabContainer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RibbonTabContainer_MouseClick);
            this.RibbonTabContainer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RibbonTabContainer_MouseDoubleClick);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ribbonPageFile);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(777, 72);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "File";
            this.tabPage1.ToolTipText = "File";
            // 
            // ribbonPageFile
            // 
            this.ribbonPageFile.BackColor = System.Drawing.SystemColors.Control;
            this.ribbonPageFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ribbonPageFile.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ribbonPageFile.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.ribbonPageFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFile});
            this.ribbonPageFile.Location = new System.Drawing.Point(3, 3);
            this.ribbonPageFile.Name = "ribbonPageFile";
            this.ribbonPageFile.Size = new System.Drawing.Size(771, 70);
            this.ribbonPageFile.TabIndex = 0;
            this.ribbonPageFile.Text = "toolStripFile";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Transparent;
            this.tabPage2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tabPage2.Controls.Add(this.ribbonPageHome);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(777, 72);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Home";
            // 
            // ribbonPageHome
            // 
            this.ribbonPageHome.BackColor = System.Drawing.SystemColors.Control;
            this.ribbonPageHome.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ribbonPageHome.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.ribbonPageHome.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonHome});
            this.ribbonPageHome.Location = new System.Drawing.Point(3, 3);
            this.ribbonPageHome.Name = "ribbonPageHome";
            this.ribbonPageHome.Size = new System.Drawing.Size(771, 70);
            this.ribbonPageHome.TabIndex = 1;
            this.ribbonPageHome.Text = "toolStripHome";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.ribbonPageAbout);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(777, 72);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "About";
            // 
            // ribbonPageAbout
            // 
            this.ribbonPageAbout.BackColor = System.Drawing.SystemColors.Control;
            this.ribbonPageAbout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ribbonPageAbout.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ribbonPageAbout.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.ribbonPageAbout.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAbout});
            this.ribbonPageAbout.Location = new System.Drawing.Point(3, 3);
            this.ribbonPageAbout.Name = "ribbonPageAbout";
            this.ribbonPageAbout.Size = new System.Drawing.Size(771, 70);
            this.ribbonPageAbout.TabIndex = 0;
            this.ribbonPageAbout.Text = "toolStripAbout";
            // 
            // RibbonPanel
            // 
            this.RibbonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RibbonPanel.Location = new System.Drawing.Point(0, 100);
            this.RibbonPanel.Name = "RibbonPanel";
            this.RibbonPanel.Size = new System.Drawing.Size(784, 440);
            this.RibbonPanel.TabIndex = 4;
            // 
            // toolStripButtonFile
            // 
            this.toolStripButtonFile.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFile.Image")));
            this.toolStripButtonFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFile.Name = "toolStripButtonFile";
            this.toolStripButtonFile.Size = new System.Drawing.Size(52, 67);
            this.toolStripButtonFile.Text = "File";
            this.toolStripButtonFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // toolStripButtonHome
            // 
            this.toolStripButtonHome.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHome.Image")));
            this.toolStripButtonHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHome.Name = "toolStripButtonHome";
            this.toolStripButtonHome.Size = new System.Drawing.Size(52, 67);
            this.toolStripButtonHome.Text = "Home";
            this.toolStripButtonHome.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // toolStripButtonAbout
            // 
            this.toolStripButtonAbout.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAbout.Image")));
            this.toolStripButtonAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAbout.Name = "toolStripButtonAbout";
            this.toolStripButtonAbout.Size = new System.Drawing.Size(52, 67);
            this.toolStripButtonAbout.Text = "About";
            this.toolStripButtonAbout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // WinFormRibbon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.RibbonPanel);
            this.Controls.Add(this.RibbonTabContainer);
            this.Controls.Add(this.statusStripMain);
            this.Name = "WinFormRibbon";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinFormRibbon";
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.RibbonTabContainer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ribbonPageFile.ResumeLayout(false);
            this.ribbonPageFile.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ribbonPageHome.ResumeLayout(false);
            this.ribbonPageHome.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ribbonPageAbout.ResumeLayout(false);
            this.ribbonPageAbout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.TabControl RibbonTabContainer;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ToolStrip ribbonPageFile;
        private System.Windows.Forms.ToolStrip ribbonPageAbout;
        private System.Windows.Forms.ToolStrip ribbonPageHome;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel RibbonPanel;
        private System.Windows.Forms.ToolStripButton toolStripButtonFile;
        private System.Windows.Forms.ToolStripButton toolStripButtonHome;
        private System.Windows.Forms.ToolStripButton toolStripButtonAbout;
    }
}