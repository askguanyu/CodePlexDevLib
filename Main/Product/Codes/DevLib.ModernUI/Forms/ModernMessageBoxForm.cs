//-----------------------------------------------------------------------
// <copyright file="ModernMessageBoxForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernMessageBoxForm class.
    /// </summary>
    [ToolboxBitmap(typeof(Form))]
    public class ModernMessageBoxForm : Form
    {
        /// <summary>
        /// Field DefaultColor.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Color DefaultColor = Color.FromArgb(57, 179, 215);

        /// <summary>
        /// Field ErrorColor.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Color ErrorColor = Color.FromArgb(210, 50, 45);

        /// <summary>
        /// Field WarningColor.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Color WarningColor = Color.FromArgb(237, 156, 40);

        /// <summary>
        /// Field SuccessColor.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Color SuccessColor = Color.FromArgb(71, 164, 71);

        /// <summary>
        /// Field QuestionColor.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Color QuestionColor = Color.FromArgb(71, 164, 71);

        /// <summary>
        /// Field _components.
        /// </summary>
        private IContainer _components = null;

        /// <summary>
        /// Field _panelBody.
        /// </summary>
        private Panel _panelBody;

        /// <summary>
        /// Field _titleLabel.
        /// </summary>
        private Label _titleLabel;

        /// <summary>
        /// Field _messageLabel.
        /// </summary>
        private Label _messageLabel;

        /// <summary>
        /// Field _modernButton1.
        /// </summary>
        private ModernButton _modernButton1;

        /// <summary>
        /// Field _modernButton2.
        /// </summary>
        private ModernButton _modernButton2;

        /// <summary>
        /// Field _modernButton3.
        /// </summary>
        private ModernButton _modernButton3;

        /// <summary>
        /// Field _tableLayoutPanelBody.
        /// </summary>
        private TableLayoutPanel _tableLayoutPanelBody;

        /// <summary>
        /// Field _panelBottom.
        /// </summary>
        private Panel _panelBottom;

        /// <summary>
        /// Field _properties.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ModernMessageBoxProperties _properties = null;

        /// <summary>
        /// Field _dialogResult.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DialogResult _dialogResult = DialogResult.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernMessageBoxForm"/> class.
        /// </summary>
        public ModernMessageBoxForm()
        {
            this.InitializeComponent();

            this._properties = new ModernMessageBoxProperties(this);

            this.StylizeButton(this._modernButton1);
            this.StylizeButton(this._modernButton2);
            this.StylizeButton(this._modernButton3);

            this._modernButton1.Click += this.ButtonClick;
            this._modernButton2.Click += this.ButtonClick;
            this._modernButton3.Click += this.ButtonClick;
        }

        /// <summary>
        /// Gets the top body section of the control.
        /// </summary>
        public Panel Body
        {
            get
            {
                return this._panelBody;
            }
        }

        /// <summary>
        /// Gets the message box display properties.
        /// </summary>
        public ModernMessageBoxProperties Properties
        {
            get
            {
                return this._properties;
            }
        }

        /// <summary>
        /// Gets the dialog result that the user have chosen.
        /// </summary>
        public DialogResult Result
        {
            get
            {
                return this._dialogResult;
            }
        }

        /// <summary>
        /// Arranges the appearance of the message box overlay.
        /// </summary>
        public void ArrangeAppearance()
        {
            this._titleLabel.Text = this._properties.Caption;
            this._messageLabel.Text = this._properties.Text;

            switch (this._properties.Icon)
            {
                case MessageBoxIcon.Exclamation:
                    this._panelBody.BackColor = WarningColor;
                    break;

                case MessageBoxIcon.Error:
                    this._panelBody.BackColor = ErrorColor;
                    break;

                default:
                    break;
            }

            switch (this._properties.Buttons)
            {
                case MessageBoxButtons.OK:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&OK";
                    this._modernButton1.Location = this._modernButton3.Location;
                    this._modernButton1.Tag = DialogResult.OK;

                    this.EnableButton(this._modernButton2, false);
                    this.EnableButton(this._modernButton3, false);
                    break;

                case MessageBoxButtons.OKCancel:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&OK";
                    this._modernButton1.Location = this._modernButton2.Location;
                    this._modernButton1.Tag = DialogResult.OK;

                    this.EnableButton(this._modernButton2);

                    this._modernButton2.Text = "&Cancel";
                    this._modernButton2.Location = this._modernButton3.Location;
                    this._modernButton2.Tag = DialogResult.Cancel;

                    this.EnableButton(this._modernButton3, false);
                    break;

                case MessageBoxButtons.RetryCancel:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&Retry";
                    this._modernButton1.Location = this._modernButton2.Location;
                    this._modernButton1.Tag = DialogResult.Retry;

                    this.EnableButton(this._modernButton2);

                    this._modernButton2.Text = "&Cancel";
                    this._modernButton2.Location = this._modernButton3.Location;
                    this._modernButton2.Tag = DialogResult.Cancel;

                    this.EnableButton(this._modernButton3, false);
                    break;

                case MessageBoxButtons.YesNo:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&Yes";
                    this._modernButton1.Location = this._modernButton2.Location;
                    this._modernButton1.Tag = DialogResult.Yes;

                    this.EnableButton(this._modernButton2);

                    this._modernButton2.Text = "&No";
                    this._modernButton2.Location = this._modernButton3.Location;
                    this._modernButton2.Tag = DialogResult.No;

                    this.EnableButton(this._modernButton3, false);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&Yes";
                    this._modernButton1.Tag = DialogResult.Yes;

                    this.EnableButton(this._modernButton2);

                    this._modernButton2.Text = "&No";
                    this._modernButton2.Tag = DialogResult.No;

                    this.EnableButton(this._modernButton3);

                    this._modernButton3.Text = "&Cancel";
                    this._modernButton3.Tag = DialogResult.Cancel;

                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    this.EnableButton(this._modernButton1);

                    this._modernButton1.Text = "&Abort";
                    this._modernButton1.Tag = DialogResult.Abort;

                    this.EnableButton(this._modernButton2);

                    this._modernButton2.Text = "&Retry";
                    this._modernButton2.Tag = DialogResult.Retry;

                    this.EnableButton(this._modernButton3);

                    this._modernButton3.Text = "&Ignore";
                    this._modernButton3.Tag = DialogResult.Ignore;

                    break;

                default: break;
            }

            switch (this._properties.Icon)
            {
                case MessageBoxIcon.Error:
                    this._panelBody.BackColor = ErrorColor;
                    break;

                case MessageBoxIcon.Warning:
                    this._panelBody.BackColor = WarningColor;
                    break;

                case MessageBoxIcon.Information:
                    this._panelBody.BackColor = DefaultColor;
                    break;

                case MessageBoxIcon.Question:
                    this._panelBody.BackColor = QuestionColor;
                    break;

                default:
                    this._panelBody.BackColor = Color.DarkGray;
                    break;
            }
        }

        /// <summary>
        /// Sets the default focused button.
        /// </summary>
        public void SetDefaultButton()
        {
            switch (this._properties.DefaultButton)
            {
                case MessageBoxDefaultButton.Button1:
                    if (this._modernButton1 != null)
                    {
                        if (this._modernButton1.Enabled)
                        {
                            this._modernButton1.Focus();
                        }
                    }

                    break;

                case MessageBoxDefaultButton.Button2:
                    if (this._modernButton2 != null)
                    {
                        if (this._modernButton2.Enabled)
                        {
                            this._modernButton2.Focus();
                        }
                    }

                    break;

                case MessageBoxDefaultButton.Button3:
                    if (this._modernButton3 != null)
                    {
                        if (this._modernButton3.Enabled)
                        {
                            this._modernButton3.Focus();
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this._components != null))
            {
                this._components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._panelBody = new Panel();
            this._tableLayoutPanelBody = new TableLayoutPanel();
            this._messageLabel = new Label();
            this._titleLabel = new Label();
            this._modernButton1 = new ModernButton();
            this._modernButton3 = new ModernButton();
            this._modernButton2 = new ModernButton();
            this._panelBottom = new Panel();
            this._panelBody.SuspendLayout();
            this._tableLayoutPanelBody.SuspendLayout();
            this._panelBottom.SuspendLayout();
            this.SuspendLayout();

            this._panelBody.BackColor = Color.DarkGray;
            this._panelBody.Controls.Add(this._tableLayoutPanelBody);
            this._panelBody.Dock = DockStyle.Fill;
            this._panelBody.Location = new Point(0, 0);
            this._panelBody.Margin = new Padding(0);
            this._panelBody.Name = "_panelBody";
            this._panelBody.Size = new Size(804, 211);
            this._panelBody.TabIndex = 2;

            this._tableLayoutPanelBody.ColumnCount = 3;
            this._tableLayoutPanelBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            this._tableLayoutPanelBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            this._tableLayoutPanelBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            this._tableLayoutPanelBody.Controls.Add(this._messageLabel, 1, 2);
            this._tableLayoutPanelBody.Controls.Add(this._titleLabel, 1, 1);
            this._tableLayoutPanelBody.Controls.Add(this._panelBottom, 1, 3);
            this._tableLayoutPanelBody.Dock = DockStyle.Fill;
            this._tableLayoutPanelBody.Location = new Point(0, 0);
            this._tableLayoutPanelBody.Name = "_tableLayoutPanelBody";
            this._tableLayoutPanelBody.RowCount = 4;
            this._tableLayoutPanelBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 5F));
            this._tableLayoutPanelBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            this._tableLayoutPanelBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._tableLayoutPanelBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._tableLayoutPanelBody.Size = new Size(804, 211);
            this._tableLayoutPanelBody.TabIndex = 6;

            this._messageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this._messageLabel.BackColor = Color.Transparent;
            this._messageLabel.ForeColor = Color.White;
            this._messageLabel.Location = new Point(83, 30);
            this._messageLabel.Margin = new Padding(3, 0, 0, 0);
            this._messageLabel.Name = "_messageLabel";
            this._messageLabel.Size = new Size(640, 141);
            this._messageLabel.TabIndex = 0;
            this._messageLabel.Text = "message here";

            this._titleLabel.AutoSize = true;
            this._titleLabel.BackColor = Color.Transparent;
            this._titleLabel.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this._titleLabel.ForeColor = Color.WhiteSmoke;
            this._titleLabel.Location = new Point(80, 5);
            this._titleLabel.Margin = new Padding(0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new Size(125, 25);
            this._titleLabel.TabIndex = 1;
            this._titleLabel.Text = "message title";

            this._modernButton1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this._modernButton1.BackColor = Color.ForestGreen;
            this._modernButton1.FontWeight = ModernFontWeight.Regular;
            this._modernButton1.Location = new Point(357, 1);
            this._modernButton1.Name = "_modernButton1";
            this._modernButton1.Size = new Size(90, 26);
            this._modernButton1.TabIndex = 3;
            this._modernButton1.Text = "button 1";
            this._modernButton1.UseSelectable = true;

            this._modernButton2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this._modernButton2.FontWeight = ModernFontWeight.Regular;
            this._modernButton2.Location = new Point(455, 1);
            this._modernButton2.Name = "_modernButton2";
            this._modernButton2.Size = new Size(90, 26);
            this._modernButton2.TabIndex = 4;
            this._modernButton2.Text = "button 2";
            this._modernButton2.UseSelectable = true;

            this._modernButton3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this._modernButton3.FontWeight = ModernFontWeight.Regular;
            this._modernButton3.Location = new Point(553, 1);
            this._modernButton3.Name = "_modernButton3";
            this._modernButton3.Size = new Size(90, 26);
            this._modernButton3.TabIndex = 5;
            this._modernButton3.Text = "button 3";
            this._modernButton3.UseSelectable = true;

            this._panelBottom.BackColor = Color.Transparent;
            this._panelBottom.Controls.Add(this._modernButton2);
            this._panelBottom.Controls.Add(this._modernButton1);
            this._panelBottom.Controls.Add(this._modernButton3);
            this._panelBottom.Dock = DockStyle.Fill;
            this._panelBottom.Location = new Point(80, 171);
            this._panelBottom.Margin = new Padding(0);
            this._panelBottom.Name = "_panelBottom";
            this._panelBottom.Size = new Size(643, 40);
            this._panelBottom.TabIndex = 2;

            this.AutoScaleDimensions = new SizeF(8F, 21F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(804, 211);
            this.ControlBox = false;
            this.Controls.Add(this._panelBody);
            this.Font = new Font("Segoe UI Light", 12F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Margin = new Padding(4, 5, 4, 5);
            this.Name = "ModernMessageBoxForm";
            this.StartPosition = FormStartPosition.Manual;

            this._panelBody.ResumeLayout(false);
            this._tableLayoutPanelBody.ResumeLayout(false);
            this._tableLayoutPanelBody.PerformLayout();
            this._panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Enables the button.
        /// </summary>
        /// <param name="button">The button.</param>
        private void EnableButton(ModernButton button)
        {
            this.EnableButton(button, true);
        }

        /// <summary>
        /// Enables the button.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="enabled">true to enable; otherwise, false.</param>
        private void EnableButton(ModernButton button, bool enabled)
        {
            button.Enabled = enabled;
            button.Visible = enabled;
        }

        /// <summary>
        /// The mouse enter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonMouseEnter(object sender, EventArgs e)
        {
            this.StylizeButton((ModernButton)sender, true);
        }

        /// <summary>
        /// The mouse leave.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonMouseLeave(object sender, EventArgs e)
        {
            this.StylizeButton((ModernButton)sender);
        }

        /// <summary>
        /// Stylizes the button.
        /// </summary>
        /// <param name="button">The button.</param>
        private void StylizeButton(ModernButton button)
        {
            this.StylizeButton(button, false);
        }

        /// <summary>
        /// Stylizes the button.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="hovered">if set to <c>true</c> [hovered].</param>
        private void StylizeButton(ModernButton button, bool hovered)
        {
            button.Cursor = Cursors.Hand;

            button.MouseEnter -= this.ButtonMouseEnter;
            button.MouseEnter += this.ButtonMouseEnter;

            button.MouseLeave -= this.ButtonMouseLeave;
            button.MouseLeave += this.ButtonMouseLeave;
        }

        /// <summary>
        /// The click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClick(object sender, EventArgs e)
        {
            ModernButton button = (ModernButton)sender;

            if (!button.Enabled)
            {
                return;
            }

            this._dialogResult = (DialogResult)button.Tag;
            this.Hide();
        }
    }
}
