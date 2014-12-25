//-----------------------------------------------------------------------
// <copyright file="ModernTextBox.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernTextBox user control.
    /// </summary>
    [ToolboxBitmap(typeof(TextBox))]
    public class ModernTextBox : Control, IModernControl
    {
        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _modernFontSize.
        /// </summary>
        private ModernFontSize _modernFontSize = ModernFontSize.Small;

        /// <summary>
        /// Field _modernFontWeight.
        /// </summary>
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Regular;

        /// <summary>
        /// Field _baseTextBox.
        /// </summary>
        private PromptedTextBox _baseTextBox;

        /// <summary>
        /// Field _textBoxIcon.
        /// </summary>
        private Image _textBoxIcon = null;

        /// <summary>
        /// Field _textBoxIconRight.
        /// </summary>
        private bool _textBoxIconRight = false;

        /// <summary>
        /// Field _displayIcon.
        /// </summary>
        private bool _displayIcon = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTextBox" /> class.
        /// </summary>
        public ModernTextBox()
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

            base.TabStop = false;
            this.GotFocus += this.OnModernTextBoxGotFocus;

            this.CreateBaseTextBox();
            this.UpdateBaseTextBox();
            this.AddEventHandler();

            this.FontSize = ModernFontSize.Small;
            this.FontWeight = ModernFontWeight.Regular;
            this.DisplayIcon = true;
            this.MaxLength = int.MaxValue;
        }

        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

        /// <summary>
        /// Event AcceptsTabChanged.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler AcceptsTabChanged;

        /// <summary>
        /// Event Pasted.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler<ClipboardEventArgs> Pasted;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernColorStyle.Default)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernColorStyle ColorStyle
        {
            get
            {
                if (this.DesignMode || this._modernColorStyle != ModernColorStyle.Default)
                {
                    return this._modernColorStyle;
                }

                if (this.StyleManager != null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return this.StyleManager.ColorStyle;
                }

                if (this.StyleManager == null && this._modernColorStyle == ModernColorStyle.Default)
                {
                    return ModernConstants.DefaultColorStyle;
                }

                return this._modernColorStyle;
            }

            set
            {
                this._modernColorStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern theme style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernThemeStyle.Default)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernThemeStyle ThemeStyle
        {
            get
            {
                if (this.DesignMode || this._modernThemeStyle != ModernThemeStyle.Default)
                {
                    return this._modernThemeStyle;
                }

                if (this.StyleManager != null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return this.StyleManager.ThemeStyle;
                }

                if (this.StyleManager == null && this._modernThemeStyle == ModernThemeStyle.Default)
                {
                    return ModernConstants.DefaultThemeStyle;
                }

                return this._modernThemeStyle;
            }

            set
            {
                this._modernThemeStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets modern style manager.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom BackColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseSelectable
        {
            get
            {
                return this.GetStyle(ControlStyles.Selectable);
            }

            set
            {
                this.SetStyle(ControlStyles.Selectable, value);
            }
        }

        /// <summary>
        /// Gets or sets modern font size.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontSize.Small)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontSize FontSize
        {
            get
            {
                return this._modernFontSize;
            }

            set
            {
                this._modernFontSize = value;
                this.UpdateBaseTextBox();
            }
        }

        /// <summary>
        /// Gets or sets modern font weight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Regular)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontWeight FontWeight
        {
            get
            {
                return this._modernFontWeight;
            }

            set
            {
                this._modernFontWeight = value;
                this.UpdateBaseTextBox();
            }
        }

        /// <summary>
        /// Gets or sets prompt text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue("")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category(ModernConstants.PropertyCategoryName)]
        public string PromptText
        {
            get
            {
                return this._baseTextBox.PromptText;
            }

            set
            {
                this._baseTextBox.PromptText = value;
            }
        }

        /// <summary>
        /// Gets or sets icon.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category(ModernConstants.PropertyCategoryName)]
        public Image Icon
        {
            get
            {
                return this._textBoxIcon;
            }

            set
            {
                this._textBoxIcon = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether icon is on the right side or not.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool IconRight
        {
            get
            {
                return this._textBoxIconRight;
            }

            set
            {
                this._textBoxIconRight = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether display icon.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool DisplayIcon
        {
            get
            {
                return this._displayIcon;
            }

            set
            {
                this._displayIcon = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the shortcut menu associated with the control.
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get
            {
                return this._baseTextBox.ContextMenu;
            }

            set
            {
                base.ContextMenu = value;
                this._baseTextBox.ContextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets the ContextMenuStrip associated with this control.
        /// </summary>
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this._baseTextBox.ContextMenuStrip;
            }

            set
            {
                base.ContextMenuStrip = value;
                this._baseTextBox.ContextMenuStrip = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is a multiline control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool Multiline
        {
            get
            {
                return this._baseTextBox.Multiline;
            }

            set
            {
                this._baseTextBox.Multiline = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a multiline text box control automatically wraps words to the beginning of the next line when necessary.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool WordWrap
        {
            get
            {
                return this._baseTextBox.WordWrap;
            }

            set
            {
                this._baseTextBox.WordWrap = value;
            }
        }

        /// <summary>
        /// Gets or sets the current text in the TextBox.
        /// </summary>
        public override string Text
        {
            get
            {
                return this._baseTextBox.Text;
            }

            set
            {
                this._baseTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the lines of text in a text box control.
        /// </summary>
        public string[] Lines
        {
            get
            {
                return this._baseTextBox.Lines;
            }

            set
            {
                this._baseTextBox.Lines = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the currently selected text in the control.
        /// </summary>
        [Browsable(false)]
        public string SelectedText
        {
            get
            {
                return this._baseTextBox.SelectedText;
            }

            set
            {
                this._baseTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether text in the text box is read-only.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return this._baseTextBox.ReadOnly;
            }

            set
            {
                this._baseTextBox.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the character used to mask characters of a password in a single-line.
        /// </summary>
        public char PasswordChar
        {
            get
            {
                return this._baseTextBox.PasswordChar;
            }

            set
            {
                this._baseTextBox.PasswordChar = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text in the control should appear as the default password character.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        public bool UseSystemPasswordChar
        {
            get
            {
                return this._baseTextBox.UseSystemPasswordChar;
            }

            set
            {
                this._baseTextBox.UseSystemPasswordChar = value;
            }
        }

        /// <summary>
        /// Gets or sets how text is aligned in the control.
        /// </summary>
        public HorizontalAlignment TextAlign
        {
            get
            {
                return this._baseTextBox.TextAlign;
            }

            set
            {
                this._baseTextBox.TextAlign = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can give the focus to this control using the TAB key.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        public new bool TabStop
        {
            get
            {
                return this._baseTextBox.TabStop;
            }

            set
            {
                this._baseTextBox.TabStop = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters the user can type or paste into the text box control.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return this._baseTextBox.MaxLength;
            }

            set
            {
                this._baseTextBox.MaxLength = value;
            }
        }

        /// <summary>
        /// Gets or sets which scroll bars should appear in a multiline control.
        /// </summary>
        public ScrollBars ScrollBars
        {
            get
            {
                return this._baseTextBox.ScrollBars;
            }

            set
            {
                this._baseTextBox.ScrollBars = value;
            }
        }

        /// <summary>
        /// Gets icon size.
        /// </summary>
        protected Size IconSize
        {
            get
            {
                if (this.DisplayIcon && this.Icon != null)
                {
                    Size originalSize = this.Icon.Size;
                    double resizeFactor = (double)(this.ClientRectangle.Height - 2) / (double)originalSize.Height;
                    Point iconLocation = new Point(1, 1);
                    return new Size((int)(originalSize.Width * resizeFactor), (int)(originalSize.Height * resizeFactor));
                }

                return new Size(-1, -1);
            }
        }

        /// <summary>
        /// Selects a range of text in the text box.
        /// </summary>
        /// <param name="start">The position of the first character in the current text selection within the text box.</param>
        /// <param name="length">The number of characters to select.</param>
        public void Select(int start, int length)
        {
            this._baseTextBox.Select(start, length);
        }

        /// <summary>
        /// Selects all text in the text box.
        /// </summary>
        public void SelectAll()
        {
            this._baseTextBox.SelectAll();
        }

        /// <summary>
        /// Clears all text from the text box control.
        /// </summary>
        public void Clear()
        {
            this._baseTextBox.Clear();
        }

        /// <summary>
        /// Appends text to the current text of a text box.
        /// </summary>
        /// <param name="text">The text to append to the current contents of the text box.</param>
        public void AppendText(string text)
        {
            this._baseTextBox.AppendText(text);
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.UpdateBaseTextBox();
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintBackground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintBackground != null)
            {
                this.CustomPaintBackground(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaint" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaint(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaint != null)
            {
                this.CustomPaint(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintForeground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCustomPaintForeground(ModernPaintEventArgs e)
        {
            if (this.GetStyle(ControlStyles.UserPaint) && this.CustomPaintForeground != null)
            {
                this.CustomPaintForeground(this, e);
            }
        }

        /// <summary>
        /// OnPaintBackground method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                Color backColor = this.BackColor;

                if (!this.UseCustomBackColor)
                {
                    backColor = ModernPaint.BackColor.Button.Normal(this.ThemeStyle);
                }

                this._baseTextBox.BackColor = backColor;

                if (backColor.A == 255)
                {
                    e.Graphics.Clear(backColor);
                    return;
                }

                base.OnPaintBackground(e);

                this.OnCustomPaintBackground(new ModernPaintEventArgs(backColor, Color.Empty, e.Graphics));
            }
            catch
            {
                this.Invalidate();
            }
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (this.GetStyle(ControlStyles.AllPaintingInWmPaint))
                {
                    this.OnPaintBackground(e);
                }

                this.OnCustomPaint(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
                this.OnPaintForeground(e);
            }
            catch
            {
                this.Invalidate();
            }
        }

        /// <summary>
        /// OnPaintForeground method.
        /// </summary>
        /// <param name="e">PaintEventArgs instance.</param>
        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            if (!this.UseCustomForeColor)
            {
                this._baseTextBox.ForeColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);
            }
            else
            {
                this._baseTextBox.ForeColor = this.ForeColor;
            }

            Color borderColor = ModernPaint.BorderColor.Button.Normal(this.ThemeStyle);

            if (this.UseStyleColors)
            {
                borderColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }

            using (Pen p = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            this.DrawIcon(e.Graphics);
        }

        /// <summary>
        /// OnResize method.
        /// </summary>
        /// <param name="e">EventArgs instance.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.UpdateBaseTextBox();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.V) ||
                (e.Control && e.KeyCode == Keys.V) ||
                (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert) ||
                (e.Shift && e.KeyCode == Keys.Insert))
            {
                if (this.Pasted != null)
                {
                    this.Pasted(this, new ClipboardEventArgs(Clipboard.GetText()));
                }
            }
        }

        /// <summary>
        /// OnModernTextBoxGotFocus method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void OnModernTextBoxGotFocus(object sender, EventArgs e)
        {
            this._baseTextBox.Focus();
        }

        /// <summary>
        /// BaseTextBoxAcceptsTabChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxAcceptsTabChanged(object sender, EventArgs e)
        {
            if (this.AcceptsTabChanged != null)
            {
                this.AcceptsTabChanged(this, e);
            }
        }

        /// <summary>
        /// BaseTextBoxSizeChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxSizeChanged(object sender, EventArgs e)
        {
            this.OnSizeChanged(e);
        }

        /// <summary>
        /// BaseTextBoxCursorChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxCursorChanged(object sender, EventArgs e)
        {
            this.OnCursorChanged(e);
        }

        /// <summary>
        /// BaseTextBoxContextMenuStripChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxContextMenuStripChanged(object sender, EventArgs e)
        {
            this.OnContextMenuStripChanged(e);
        }

        /// <summary>
        /// BaseTextBoxContextMenuChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxContextMenuChanged(object sender, EventArgs e)
        {
            this.OnContextMenuChanged(e);
        }

        /// <summary>
        /// BaseTextBoxClientSizeChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxClientSizeChanged(object sender, EventArgs e)
        {
            this.OnClientSizeChanged(e);
        }

        /// <summary>
        /// BaseTextBoxClick method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxClick(object sender, EventArgs e)
        {
            this.OnClick(e);
        }

        /// <summary>
        /// BaseTextBoxChangeUICues method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">UICuesEventArgs instance.</param>
        private void BaseTextBoxChangeUICues(object sender, UICuesEventArgs e)
        {
            this.OnChangeUICues(e);
        }

        /// <summary>
        /// BaseTextBoxCausesValidationChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxCausesValidationChanged(object sender, EventArgs e)
        {
            this.OnCausesValidationChanged(e);
        }

        /// <summary>
        /// BaseTextBoxKeyUp method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyEventArgs instance.</param>
        private void BaseTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        /// <summary>
        /// BaseTextBoxKeyPress method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyPressEventArgs instance.</param>
        private void BaseTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        /// <summary>
        /// BaseTextBoxKeyDown method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">KeyEventArgs instance.</param>
        private void BaseTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        /// <summary>
        /// BaseTextBoxTextChanged method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">EventArgs instance.</param>
        private void BaseTextBoxTextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);
        }

        /// <summary>
        /// Draw icon.
        /// </summary>
        /// <param name="g">Graphics instance.</param>
        private void DrawIcon(Graphics g)
        {
            if (this.DisplayIcon && this.Icon != null)
            {
                Point iconLocation = new Point(1, 1);

                if (this.IconRight)
                {
                    iconLocation = new Point(this.ClientRectangle.Width - this.IconSize.Width - 1, 1);
                }

                g.DrawImage(this.Icon, new Rectangle(iconLocation, this.IconSize));

                this.UpdateBaseTextBox();
            }

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, this._baseTextBox.ForeColor, g));
        }

        /// <summary>
        /// Create base text box.
        /// </summary>
        private void CreateBaseTextBox()
        {
            if (this._baseTextBox != null)
            {
                return;
            }

            this._baseTextBox = new PromptedTextBox();

            this._baseTextBox.BorderStyle = BorderStyle.None;
            this._baseTextBox.Font = ModernFonts.TextBox(this._modernFontSize, this._modernFontWeight);
            this._baseTextBox.Location = new Point(3, 3);
            this._baseTextBox.Size = new Size(this.Width - 6, this.Height - 6);

            this.Size = new Size(this._baseTextBox.Width + 6, this._baseTextBox.Height + 6);

            this._baseTextBox.TabStop = true;

            Controls.Add(this._baseTextBox);
        }

        /// <summary>
        /// Add event handler.
        /// </summary>
        private void AddEventHandler()
        {
            this._baseTextBox.AcceptsTabChanged += this.BaseTextBoxAcceptsTabChanged;
            this._baseTextBox.CausesValidationChanged += this.BaseTextBoxCausesValidationChanged;
            this._baseTextBox.ChangeUICues += this.BaseTextBoxChangeUICues;
            this._baseTextBox.Click += this.BaseTextBoxClick;
            this._baseTextBox.ClientSizeChanged += this.BaseTextBoxClientSizeChanged;
            this._baseTextBox.ContextMenuChanged += this.BaseTextBoxContextMenuChanged;
            this._baseTextBox.ContextMenuStripChanged += this.BaseTextBoxContextMenuStripChanged;
            this._baseTextBox.CursorChanged += this.BaseTextBoxCursorChanged;
            this._baseTextBox.KeyDown += this.BaseTextBoxKeyDown;
            this._baseTextBox.KeyPress += this.BaseTextBoxKeyPress;
            this._baseTextBox.KeyUp += this.BaseTextBoxKeyUp;
            this._baseTextBox.SizeChanged += this.BaseTextBoxSizeChanged;
            this._baseTextBox.TextChanged += this.BaseTextBoxTextChanged;
        }

        /// <summary>
        /// Update base text box.
        /// </summary>
        private void UpdateBaseTextBox()
        {
            if (this._baseTextBox == null)
            {
                return;
            }

            this._baseTextBox.Font = ModernFonts.TextBox(this._modernFontSize, this._modernFontWeight);

            if (this.DisplayIcon)
            {
                Point textBoxLocation = new Point(this.IconSize.Width + 4, 3);

                if (this.IconRight)
                {
                    textBoxLocation = new Point(3, 3);
                }

                this._baseTextBox.Location = textBoxLocation;
                this._baseTextBox.Size = new Size(this.Width - 7 - this.IconSize.Width, this.Height - 6);
            }
            else
            {
                this._baseTextBox.Location = new Point(3, 3);
                this._baseTextBox.Size = new Size(this.Width - 6, this.Height - 6);
            }
        }

        /// <summary>
        /// PromptedTextBox user control.
        /// </summary>
        private class PromptedTextBox : TextBox
        {
            /// <summary>
            /// Field OCM_COMMAND.
            /// </summary>
            private const int OCM_COMMAND = 0x2111;

            /// <summary>
            /// Field WM_PAINT.
            /// </summary>
            private const int WM_PAINT = 15;

            /// <summary>
            /// Field _drawPrompt.
            /// </summary>
            private bool _drawPrompt;

            /// <summary>
            /// Field _promptText.
            /// </summary>
            private string _promptText = string.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="PromptedTextBox" /> class.
            /// </summary>
            public PromptedTextBox()
            {
                this._drawPrompt = this.Text == null || string.IsNullOrEmpty(this.Text.Trim());
            }

            /// <summary>
            /// Gets or sets prompt text.
            /// </summary>
            [Browsable(true)]
            [EditorBrowsable(EditorBrowsableState.Always)]
            [DefaultValue("")]
            public string PromptText
            {
                get
                {
                    return this._promptText;
                }

                set
                {
                    this._promptText = value.Trim();
                    this.Invalidate();
                }
            }

            /// <summary>
            /// OnPaint method.
            /// </summary>
            /// <param name="e">PaintEventArgs instance.</param>
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (this._drawPrompt)
                {
                    this.DrawTextPrompt(e.Graphics);
                }
            }

            /// <summary>
            /// OnTextAlignChanged method.
            /// </summary>
            /// <param name="e">EventArgs instance.</param>
            protected override void OnTextAlignChanged(EventArgs e)
            {
                base.OnTextAlignChanged(e);
                this.Invalidate();
            }

            /// <summary>
            /// OnTextChanged method.
            /// </summary>
            /// <param name="e">EventArgs instance.</param>
            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                this._drawPrompt = this.Text == null || string.IsNullOrEmpty(this.Text.Trim());
            }

            /// <summary>
            /// WndProc method.
            /// </summary>
            /// <param name="m">A Windows Message object.</param>
            protected override void WndProc(ref Message m)
            {
                if (((m.Msg == WM_PAINT) || (m.Msg == OCM_COMMAND)) && (this._drawPrompt && !this.GetStyle(ControlStyles.UserPaint)))
                {
                    this.DrawTextPrompt();
                }

                base.WndProc(ref m);
            }

            /// <summary>
            /// DrawTextPrompt method.
            /// </summary>
            private void DrawTextPrompt()
            {
                using (Graphics graphics = this.CreateGraphics())
                {
                    this.DrawTextPrompt(graphics);
                }
            }

            /// <summary>
            /// DrawTextPrompt method.
            /// </summary>
            /// <param name="g">Graphics instance.</param>
            private void DrawTextPrompt(Graphics g)
            {
                TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                Rectangle clientRectangle = ClientRectangle;

                switch (this.TextAlign)
                {
                    case HorizontalAlignment.Left:
                        clientRectangle.Offset(1, 1);
                        break;

                    case HorizontalAlignment.Right:
                        flags |= TextFormatFlags.Right;
                        clientRectangle.Offset(0, 1);
                        break;

                    case HorizontalAlignment.Center:
                        flags |= TextFormatFlags.HorizontalCenter;
                        clientRectangle.Offset(0, 1);
                        break;
                }

                TextRenderer.DrawText(g, this.PromptText, this.Font, clientRectangle, SystemColors.GrayText, this.BackColor, flags);
            }
        }
    }
}
