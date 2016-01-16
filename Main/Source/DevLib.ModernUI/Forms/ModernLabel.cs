//-----------------------------------------------------------------------
// <copyright file="ModernLabel.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernLabel user control.
    /// </summary>
    [ToolboxBitmap(typeof(Label))]
    public class ModernLabel : Label, IModernControl
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
        private ModernFontSize _modernFontSize = ModernFontSize.Medium;

        /// <summary>
        /// Field _modernFontWeight.
        /// </summary>
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Light;

        /// <summary>
        /// Field _baseTextBox.
        /// </summary>
        private DoubleBufferedTextBox _baseTextBox;

        /// <summary>
        /// Field _wordWrap.
        /// </summary>
        private bool _wordWrap;

        /// <summary>
        /// Field _firstInitialization.
        /// </summary>
        private bool _firstInitialization = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernLabel"/> class.
        /// </summary>
        public ModernLabel()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this._baseTextBox = new DoubleBufferedTextBox();
            this._baseTextBox.Visible = false;
            this.Controls.Add(this._baseTextBox);

            this.FontSize = ModernFontSize.Medium;
            this.FontWeight = ModernFontWeight.Light;
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
        /// Gets or sets the size of the font.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontSize.Medium)]
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
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Light)]
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
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether wrap to line.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool WordWrap
        {
            get
            {
                return this._wordWrap;
            }

            set
            {
                this._wordWrap = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether user can select label text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool CanSelectText
        {
            get;
            set;
        }

        /// <summary>
        /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
        /// </summary>
        public override void Refresh()
        {
            if (this.CanSelectText)
            {
                this.UpdateBaseTextBox();
            }

            base.Refresh();
        }

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="proposedSize">The custom-sized area for a control.</param>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (Graphics g = this.CreateGraphics())
            {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, this.Text ?? string.Empty, ModernFonts.Label(this.FontSize, this.FontWeight), proposedSize, ModernPaint.GetTextFormatFlags(this.TextAlign));
            }

            return preferredSize;
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
        /// Raises the <see cref="E:PaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                Color backColor = this.BackColor;

                if (!this.UseCustomBackColor)
                {
                    backColor = ModernPaint.BackColor.Form(this.ThemeStyle);

                    if (this.Parent is ModernTile)
                    {
                        backColor = ModernPaint.GetStyleColor(this.ColorStyle);
                    }
                }

                if (backColor.A == 255 && this.BackgroundImage == null)
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
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
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
        /// Raises the <see cref="E:PaintForeground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            Color foreColor;

            if (this.UseCustomForeColor)
            {
                foreColor = this.ForeColor;
            }
            else
            {
                if (!this.Enabled)
                {
                    if (this.Parent != null)
                    {
                        if (this.Parent is ModernTile)
                        {
                            foreColor = ModernPaint.ForeColor.Tile.Disabled(this.ThemeStyle);
                        }
                        else
                        {
                            foreColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);
                        }
                    }
                    else
                    {
                        foreColor = ModernPaint.ForeColor.Label.Disabled(this.ThemeStyle);
                    }
                }
                else
                {
                    if (this.Parent != null)
                    {
                        if (this.Parent is ModernTile)
                        {
                            foreColor = ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
                        }
                        else
                        {
                            if (this.UseStyleColors)
                            {
                                foreColor = ModernPaint.GetStyleColor(this.ColorStyle);
                            }
                            else
                            {
                                foreColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);
                            }
                        }
                    }
                    else
                    {
                        if (this.UseStyleColors)
                        {
                            foreColor = ModernPaint.GetStyleColor(this.ColorStyle);
                        }
                        else
                        {
                            foreColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);
                        }
                    }
                }
            }

            if (this.CanSelectText)
            {
                this.CreateBaseTextBox();
                this.UpdateBaseTextBox();

                if (!this._baseTextBox.Visible)
                {
                    TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.Label(this.FontSize, this.FontWeight), this.ClientRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign));
                }
            }
            else
            {
                this.DestroyBaseTextbox();
                TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.Label(this.FontSize, this.FontWeight), this.ClientRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign, this.WordWrap));
                this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, foreColor, e.Graphics));
            }
        }

        /// <summary>
        /// Raises the EnabledChanged event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            if (this.CanSelectText)
            {
                this.HideBaseTextBox();
            }

            base.OnResize(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this.CanSelectText)
            {
                this.ShowBaseTextBox();
            }
        }

        /// <summary>
        /// Creates the base text box.
        /// </summary>
        private void CreateBaseTextBox()
        {
            if (this._baseTextBox.Visible && !this._firstInitialization)
            {
                return;
            }

            if (!this._firstInitialization)
            {
                return;
            }

            this._firstInitialization = false;

            if (!this.DesignMode)
            {
                Form parentForm = this.FindForm();

                if (parentForm != null)
                {
                    parentForm.ResizeBegin += this.OnParentFormResizeBegin;
                    parentForm.ResizeEnd += this.OnParentFormResizeEnd;
                }
            }

            this._baseTextBox.BackColor = Color.Transparent;
            this._baseTextBox.Visible = true;
            this._baseTextBox.BorderStyle = BorderStyle.None;
            this._baseTextBox.Font = ModernFonts.Label(this.FontSize, this.FontWeight);
            this._baseTextBox.Location = new Point(1, 0);
            this._baseTextBox.Text = this.Text;
            this._baseTextBox.ReadOnly = true;

            this._baseTextBox.Size = this.GetPreferredSize(Size.Empty);
            this._baseTextBox.Multiline = true;

            this._baseTextBox.DoubleClick += this.BaseTextBoxOnDoubleClick;
            this._baseTextBox.Click += this.BaseTextBoxOnClick;

            this.Controls.Add(this._baseTextBox);
        }

        /// <summary>
        /// Handles the ResizeEnd event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnParentFormResizeEnd(object sender, EventArgs e)
        {
            if (this.CanSelectText)
            {
                this.ShowBaseTextBox();
            }
        }

        /// <summary>
        /// Handles the ResizeBegin event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnParentFormResizeBegin(object sender, EventArgs e)
        {
            if (this.CanSelectText)
            {
                this.HideBaseTextBox();
            }
        }

        /// <summary>
        /// Destroys the base textbox.
        /// </summary>
        private void DestroyBaseTextbox()
        {
            if (!this._baseTextBox.Visible)
            {
                return;
            }

            this._baseTextBox.DoubleClick -= this.BaseTextBoxOnDoubleClick;
            this._baseTextBox.Click -= this.BaseTextBoxOnClick;
            this._baseTextBox.Visible = false;
        }

        /// <summary>
        /// Updates the base text box.
        /// </summary>
        private void UpdateBaseTextBox()
        {
            if (!this._baseTextBox.Visible)
            {
                return;
            }

            this.SuspendLayout();
            this._baseTextBox.SuspendLayout();

            if (this.UseCustomBackColor)
            {
                this._baseTextBox.BackColor = this.BackColor;
            }
            else
            {
                this._baseTextBox.BackColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            }

            if (!this.Enabled)
            {
                if (this.Parent != null)
                {
                    if (this.Parent is ModernTile)
                    {
                        this._baseTextBox.ForeColor = ModernPaint.ForeColor.Tile.Disabled(this.ThemeStyle);
                    }
                    else
                    {
                        if (this.UseStyleColors)
                        {
                            this._baseTextBox.ForeColor = ModernPaint.GetStyleColor(this.ColorStyle);
                        }
                        else
                        {
                            this._baseTextBox.ForeColor = ModernPaint.ForeColor.Label.Disabled(this.ThemeStyle);
                        }
                    }
                }
                else
                {
                    if (this.UseStyleColors)
                    {
                        this._baseTextBox.ForeColor = ModernPaint.GetStyleColor(this.ColorStyle);
                    }
                    else
                    {
                        this._baseTextBox.ForeColor = ModernPaint.ForeColor.Label.Disabled(this.ThemeStyle);
                    }
                }
            }
            else
            {
                if (this.Parent != null)
                {
                    if (this.Parent is ModernTile)
                    {
                        this._baseTextBox.ForeColor = ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
                    }
                    else
                    {
                        if (this.UseStyleColors)
                        {
                            this._baseTextBox.ForeColor = ModernPaint.GetStyleColor(this.ColorStyle);
                        }
                        else
                        {
                            this._baseTextBox.ForeColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);
                        }
                    }
                }
                else
                {
                    if (this.UseStyleColors)
                    {
                        this._baseTextBox.ForeColor = ModernPaint.GetStyleColor(this.ColorStyle);
                    }
                    else
                    {
                        this._baseTextBox.ForeColor = ModernPaint.ForeColor.Label.Normal(this.ThemeStyle);
                    }
                }
            }

            this._baseTextBox.Font = ModernFonts.Label(this.FontSize, this.FontWeight);
            this._baseTextBox.Text = this.Text;
            this._baseTextBox.BorderStyle = BorderStyle.None;

            this.Size = this.GetPreferredSize(Size.Empty);

            this._baseTextBox.ResumeLayout();
            this.ResumeLayout();
        }

        /// <summary>
        /// Hides the base text box.
        /// </summary>
        private void HideBaseTextBox()
        {
            this._baseTextBox.Visible = false;
        }

        /// <summary>
        /// Shows the base text box.
        /// </summary>
        private void ShowBaseTextBox()
        {
            this._baseTextBox.Visible = true;
        }

        /// <summary>
        /// Bases the text box on click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SecuritySafeCritical]
        private void BaseTextBoxOnClick(object sender, EventArgs eventArgs)
        {
            WinCaret.HideCaret(this._baseTextBox.Handle);
        }

        /// <summary>
        /// Bases the text box on double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SecuritySafeCritical]
        private void BaseTextBoxOnDoubleClick(object sender, EventArgs eventArgs)
        {
            this._baseTextBox.SelectAll();
            WinCaret.HideCaret(this._baseTextBox.Handle);
        }

        /// <summary>
        /// DoubleBufferedTextBox class.
        /// </summary>
        private class DoubleBufferedTextBox : TextBox
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DoubleBufferedTextBox"/> class.
            /// </summary>
            public DoubleBufferedTextBox()
            {
                this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
            }
        }
    }
}
