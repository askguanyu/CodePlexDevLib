//-----------------------------------------------------------------------
// <copyright file="ModernToggle.cs" company="YuGuan Corporation">
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
    /// ModernToggle user control.
    /// </summary>
    [ToolboxBitmap(typeof(CheckBox))]
    public class ModernToggle : CheckBox, IModernControl
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
        /// Initializes a new instance of the <see cref="ModernToggle" /> class.
        /// </summary>
        public ModernToggle()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);

            this.TextAlign = ContentAlignment.MiddleLeft;
            this.Name = "ModernToggle";
            this.TextStatusOn = "On";
            this.TextStatusOff = "Off";
            this.DisplayStatus = true;
            this.FontSize = ModernFontSize.Small;
            this.FontWeight = ModernFontWeight.Regular;
        }

        /// <summary>
        /// Event CustomPaintBackground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintBackground;

        /// <summary>
        /// Event CustomPaint.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public event EventHandler<ModernPaintEventArgs> CustomPaint;

        /// <summary>
        /// Event CustomPaintForeground.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public event EventHandler<ModernPaintEventArgs> CustomPaintForeground;

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(ModernColorStyle.Default)]
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
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(ModernThemeStyle.Default)]
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
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseStyleColors
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can receive focus.
        /// </summary>
        [Browsable(false)]
        [Category(ModernConstants.PropertyCategoryBehavior)]
        [DefaultValue(false)]
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
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool DisplayFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font size.
        /// </summary>
        [DefaultValue(ModernFontSize.Small)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFontSize FontSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font weight.
        /// </summary>
        [DefaultValue(ModernFontWeight.Regular)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFontWeight FontWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether display status.
        /// </summary>
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool DisplayStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets font.
        /// </summary>
        [Browsable(false)]
        public override Font Font
        {
            get
            {
                return base.Font;
            }

            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the control.
        /// </summary>
        [Browsable(false)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets text.
        /// </summary>
        [Browsable(false)]
        public override string Text
        {
            get
            {
                if (this.Checked)
                {
                    return this.TextStatusOn ?? "On";
                }

                return this.TextStatusOff ?? "Off";
            }
        }

        /// <summary>
        /// Gets or sets text of status on.
        /// </summary>
        [Browsable(true)]
        [DefaultValue("On")]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public string TextStatusOn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text of status off.
        /// </summary>
        [Browsable(true)]
        [DefaultValue("Off")]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public string TextStatusOff
        {
            get;
            set;
        }

        /// <summary>
        /// Get preferred size.
        /// </summary>
        /// <param name="proposedSize">Proposed size.</param>
        /// <returns>Preferred size.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize = base.GetPreferredSize(proposedSize);
            preferredSize.Width = this.DisplayStatus ? 80 : 50;
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
                    backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                }

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
            Color borderColor;
            Color foreColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.CheckBox.Hover(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.CheckBox.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.CheckBox.Press(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.CheckBox.Press(this.ThemeStyle);
            }
            else if (!this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.CheckBox.Disabled(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.CheckBox.Disabled(this.ThemeStyle);
            }
            else
            {
                foreColor = !this.UseStyleColors ? ModernPaint.ForeColor.CheckBox.Normal(this.ThemeStyle) : ModernPaint.GetStyleColor(this.ColorStyle);
                borderColor = ModernPaint.BorderColor.CheckBox.Normal(this.ThemeStyle);
            }

            using (Pen pen = new Pen(borderColor))
            {
                Rectangle boxRectangle = new Rectangle(this.DisplayStatus ? 30 : 0, 0, this.ClientRectangle.Width - (this.DisplayStatus ? 31 : 1), this.ClientRectangle.Height - 1);
                e.Graphics.DrawRectangle(pen, boxRectangle);
            }

            Color fillColor = this.Checked ? ModernPaint.GetStyleColor(this.ColorStyle) : ModernPaint.BorderColor.CheckBox.Normal(this.ThemeStyle);

            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                Rectangle boxRectangle = new Rectangle(this.DisplayStatus ? 32 : 2, 2, this.ClientRectangle.Width - (this.DisplayStatus ? 34 : 4), this.ClientRectangle.Height - 4);
                e.Graphics.FillRectangle(brush, boxRectangle);
            }

            Color backColor = this.BackColor;

            if (!this.UseCustomBackColor)
            {
                backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            }

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                int left = this.Checked ? this.Width - 11 : (this.DisplayStatus ? 30 : 0);

                Rectangle boxRectangle = new Rectangle(left, 0, 11, this.ClientRectangle.Height);
                e.Graphics.FillRectangle(brush, boxRectangle);
            }

            using (SolidBrush brush = new SolidBrush(ModernPaint.BorderColor.CheckBox.Hover(this.ThemeStyle)))
            {
                int left = this.Checked ? this.Width - 10 : (this.DisplayStatus ? 30 : 0);

                Rectangle boxRectangle = new Rectangle(left, 0, 10, this.ClientRectangle.Height);
                e.Graphics.FillRectangle(brush, boxRectangle);
            }

            if (this.DisplayStatus)
            {
                Rectangle textRectangle = new Rectangle(0, 0, 30, this.ClientRectangle.Height);
                TextRenderer.DrawText(e.Graphics, this.Text ?? string.Empty, ModernFonts.Link(this.FontSize, this.FontWeight), textRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign));
            }

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

        /// <summary>
        /// Raises the CheckedChanged event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnCheckedChanged(EventArgs e)
        {
            this.Invalidate();

            base.OnCheckedChanged(e);
        }
    }
}