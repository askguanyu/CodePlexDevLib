//-----------------------------------------------------------------------
// <copyright file="ModernControlBox.cs" company="YuGuan Corporation">
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
    /// Form control box.
    /// </summary>
    internal enum FormControlBox
    {
        /// <summary>
        /// The minimize box.
        /// </summary>
        Minimize,

        /// <summary>
        /// The maximize box.
        /// </summary>
        Maximize,

        /// <summary>
        /// The close box.
        /// </summary>
        Close
    }

    /// <summary>
    /// ModernFormButton class.
    /// </summary>
    internal class ModernControlBox : Button, IModernControl
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
        /// Initializes a new instance of the <see cref="ModernControlBox"/> class.
        /// </summary>
        public ModernControlBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Color backColor;
            Color foreColor;

            ModernThemeStyle modernThemeStyle = this.ThemeStyle;

            if (this.Parent != null)
            {
                if (this.Parent is IModernForm)
                {
                    modernThemeStyle = ((IModernForm)Parent).ThemeStyle;
                    backColor = ModernPaint.BackColor.Form(modernThemeStyle);
                }
                else if (this.Parent is IModernControl)
                {
                    backColor = ModernPaint.GetStyleColor(this.ColorStyle);
                }
                else
                {
                    backColor = this.Parent.BackColor;
                }
            }
            else
            {
                backColor = ModernPaint.BackColor.Form(modernThemeStyle);
            }

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Button.Normal(modernThemeStyle);
                backColor = ModernPaint.BackColor.Button.Normal(modernThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Button.Press(modernThemeStyle);
                backColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }
            else if (!this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Button.Disabled(modernThemeStyle);
                backColor = ModernPaint.BackColor.Button.Disabled(modernThemeStyle);
            }
            else
            {
                foreColor = ModernPaint.ForeColor.Button.Normal(modernThemeStyle);
            }

            e.Graphics.Clear(backColor);

            using (Font buttonFont = new Font("Webdings", 9.25f))
            {
                TextRenderer.DrawText(e.Graphics, this.Text ?? string.Empty, buttonFont, this.ClientRectangle, foreColor, backColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            this._isHovered = true;
            this.Invalidate();

            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            this._isPressed = false;
            this.Invalidate();

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            this._isHovered = false;
            this.Invalidate();

            base.OnMouseLeave(e);
        }
    }
}
