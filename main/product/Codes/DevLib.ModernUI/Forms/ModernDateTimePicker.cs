//-----------------------------------------------------------------------
// <copyright file="ModernDateTimePicker.cs" company="YuGuan Corporation">
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
    /// ModernDateTime user control.
    /// </summary>
    [ToolboxBitmap(typeof(DateTimePicker))]
    public class ModernDateTimePicker : DateTimePicker, IModernControl
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
        /// Initializes a new instance of the <see cref="ModernDateTimePicker" /> class.
        /// </summary>
        public ModernDateTimePicker()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.FontSize = ModernFontSize.Medium;
            this.FontWeight = ModernFontWeight.Regular;
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
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom ForeColor.
        /// </summary>
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
        [DefaultValue(false)]
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
        [Category(ModernConstants.PropertyCategoryName)]
        [DefaultValue(true)]
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
        [Category(ModernConstants.PropertyCategoryName)]
        public bool DisplayFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font size.
        /// </summary>
        [DefaultValue(ModernFontSize.Medium)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontSize FontSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font weight.
        /// </summary>
        [DefaultValue(ModernFontWeight.Regular)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontWeight FontWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether show UpDown.
        /// </summary>
        [DefaultValue(false)]
        [Browsable(false)]
        public new bool ShowUpDown
        {
            get
            {
                return base.ShowUpDown;
            }

            set
            {
                base.ShowUpDown = false;
            }
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
        /// Get preferred size.
        /// </summary>
        /// <param name="proposedSize">Proposed size.</param>
        /// <returns>Preferred size.</returns>
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize;
            base.GetPreferredSize(proposedSize);

            using (var g = CreateGraphics())
            {
                string measureText = Text.Length > 0 ? Text : "MeasureText";
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, measureText, ModernFonts.DateTime(this.FontSize, this.FontWeight), proposedSize, TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter);
                preferredSize.Height += 10;
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
            this.MinimumSize = new Size(0, this.GetPreferredSize(Size.Empty).Height);

            Color borderColor;
            Color foreColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Hover(this.ThemeStyle);
                borderColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Press(this.ThemeStyle);
                borderColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }
            else if (!this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Disabled(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.ComboBox.Disabled(this.ThemeStyle);
            }
            else
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Normal(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.ComboBox.Normal(this.ThemeStyle);
            }

            using (Pen pen = new Pen(borderColor))
            {
                Rectangle boxRectangle = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, boxRectangle);
            }

            using (SolidBrush brush = new SolidBrush(foreColor))
            {
                e.Graphics.FillPolygon(brush, new Point[] { new Point(this.Width - 20, (this.Height / 2) - 2), new Point(this.Width - 9, (this.Height / 2) - 2), new Point(this.Width - 15, (this.Height / 2) + 4) });
            }

            int check = 0;

            if (this.ShowCheckBox)
            {
                check = 15;

                using (Pen pen = new Pen(borderColor))
                {
                    Rectangle boxRectangle = new Rectangle(3, (this.Height / 2) - 6, 12, 12);
                    e.Graphics.DrawRectangle(pen, boxRectangle);
                }

                if (this.Checked)
                {
                    Color fillColor = ModernPaint.GetStyleColor(this.ColorStyle);

                    using (SolidBrush brush = new SolidBrush(fillColor))
                    {
                        Rectangle boxRectangle = new Rectangle(5, (this.Height / 2) - 4, 9, 9);
                        e.Graphics.FillRectangle(brush, boxRectangle);
                    }
                }
                else
                {
                    foreColor = ModernPaint.ForeColor.ComboBox.Disabled(this.ThemeStyle);
                }
            }

            Rectangle textRectangle = new Rectangle(2 + check, 2, this.Width - 20, this.Height - 4);

            TextRenderer.DrawText(e.Graphics, this.Text ?? string.Empty, ModernFonts.DateTime(this.FontSize, this.FontWeight), textRectangle, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, foreColor, e.Graphics));

            if (this.DisplayFocus && this._isFocused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            this.Invalidate();
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
    }
}