//-----------------------------------------------------------------------
// <copyright file="ModernButton.cs" company="YuGuan Corporation">
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
    /// ModernButton user control.
    /// </summary>
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public class ModernButton : Button, IModernControl
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
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Bold;

        /// <summary>
        /// Field _isHovered.
        /// </summary>
        private bool _isHovered = false;

        /// <summary>
        /// Field _isPressed.
        /// </summary>
        private bool _isPressed = false;

        /// <summary>
        /// Field _isFocused.
        /// </summary>
        private bool _isFocused = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernButton" /> class.
        /// </summary>
        public ModernButton()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.FontSize = ModernFontSize.Small;
            this.FontWeight = ModernFontWeight.Bold;
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
        /// Gets or sets a value indicating whether display focus rectangle.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool DisplayFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether highlight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool Highlight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font size.
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
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets text font weight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Bold)]
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

                if (this._isHovered && !this._isPressed && this.Enabled)
                {
                    backColor = ModernPaint.BackColor.Button.Hover(this.ThemeStyle);
                }
                else if (this._isHovered && this._isPressed && this.Enabled)
                {
                    backColor = ModernPaint.BackColor.Button.Press(this.ThemeStyle);
                }
                else if (!this.Enabled)
                {
                    backColor = ModernPaint.BackColor.Button.Disabled(this.ThemeStyle);
                }
                else
                {
                    if (!this.UseCustomBackColor)
                    {
                        backColor = ModernPaint.BackColor.Button.Normal(this.ThemeStyle);
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
            Color borderColor;
            Color foreColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                borderColor = ModernPaint.BorderColor.Button.Hover(this.ThemeStyle);
                foreColor = ModernPaint.ForeColor.Button.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                borderColor = ModernPaint.BorderColor.Button.Press(this.ThemeStyle);
                foreColor = ModernPaint.ForeColor.Button.Press(this.ThemeStyle);
            }
            else if (!this.Enabled)
            {
                borderColor = ModernPaint.BorderColor.Button.Disabled(this.ThemeStyle);
                foreColor = ModernPaint.ForeColor.Button.Disabled(this.ThemeStyle);
            }
            else
            {
                borderColor = ModernPaint.BorderColor.Button.Normal(this.ThemeStyle);

                if (this.UseCustomForeColor)
                {
                    foreColor = this.ForeColor;
                }
                else if (this.UseStyleColors)
                {
                    foreColor = ModernPaint.GetStyleColor(this.ColorStyle);
                }
                else
                {
                    foreColor = ModernPaint.ForeColor.Button.Normal(this.ThemeStyle);
                }
            }

            using (Pen pen = new Pen(borderColor))
            {
                Rectangle borderRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRect);
            }

            if (this.Highlight && !this._isHovered && !this._isPressed && this.Enabled)
            {
                using (Pen pen = ModernPaint.GetStylePen(this.ColorStyle))
                {
                    Rectangle borderRectangle = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                    e.Graphics.DrawRectangle(pen, borderRectangle);
                    borderRectangle = new Rectangle(1, 1, this.Width - 3, this.Height - 3);
                    e.Graphics.DrawRectangle(pen, borderRectangle);
                }
            }

            TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.Button(this.FontSize, this.FontWeight), this.ClientRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign));

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, foreColor, e.Graphics));

            if (this.DisplayFocus && this._isFocused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            this._isFocused = true;
            this._isHovered = true;
            this.Invalidate();

            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the LostFocus event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            this._isFocused = false;
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnLostFocus(e);
        }

        /// <summary>
        /// Raises the Enter event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            this._isFocused = true;
            this._isHovered = true;
            this.Invalidate();

            base.OnEnter(e);
        }

        /// <summary>
        /// Raises the Leave event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            this._isFocused = false;
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnLeave(e);
        }

        /// <summary>
        /// Raises the KeyDown event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this._isHovered = true;
                this._isPressed = true;
                this.Invalidate();
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Raises the KeyUp event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            this.Invalidate();

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            this._isHovered = true;
            this.Invalidate();

            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._isPressed = true;
                this.Invalidate();
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            this._isPressed = false;
            this.Invalidate();

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!this._isFocused)
            {
                this._isHovered = false;
            }

            this.Invalidate();

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the EnabledChanged event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            this.Invalidate();

            base.OnEnabledChanged(e);
        }
    }
}