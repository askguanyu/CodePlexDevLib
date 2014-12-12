//-----------------------------------------------------------------------
// <copyright file="ModernTile.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernTile user control.
    /// </summary>
    [ToolboxBitmap(typeof(Button))]
    [Designer(typeof(ParentControlDesigner))]
    public class ModernTile : Button, IContainerControl, IModernControl
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
        /// Initializes a new instance of the <see cref="ModernTile" /> class.
        /// </summary>
        public ModernTile()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.TextAlign = ContentAlignment.BottomLeft;
            this.TileImageAlign = ContentAlignment.TopLeft;
            this.ShowTileCount = true;
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
        /// Gets or sets active control.
        /// </summary>
        [Browsable(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public Control ActiveControl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether show tile count.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowTileCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets tile count.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(0)]
        [Category(ModernConstants.PropertyCategoryName)]
        public int TileCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alignment of the text on the control.
        /// </summary>
        [DefaultValue(ContentAlignment.BottomLeft)]
        public new ContentAlignment TextAlign
        {
            get
            {
                return base.TextAlign;
            }

            set
            {
                base.TextAlign = value;
            }
        }

        /// <summary>
        /// Gets or sets tile image.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [Category(ModernConstants.PropertyCategoryName)]
        public Image TileImage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use tile image.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseTileImage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alignment of the image on the control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ContentAlignment.TopLeft)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ContentAlignment TileImageAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets tile text font size.
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
        /// Gets or sets tile text font weight.
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
        /// Gets a value indicating whether the control is activate.
        /// </summary>
        /// <param name="control">Control to check.</param>
        /// <returns>true if the control is activate; otherwise, false.</returns>
        public bool ActivateControl(Control control)
        {
            if (this.Controls.Contains(control))
            {
                control.Select();
                this.ActiveControl = control;
                return true;
            }

            return false;
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
                    backColor = ModernPaint.GetStyleColor(this.ColorStyle);
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
            Color foreColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Tile.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Tile.Press(this.ThemeStyle);
            }
            else if (!this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Tile.Disabled(this.ThemeStyle);
            }
            else
            {
                foreColor = ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
            }

            if (this.UseCustomForeColor)
            {
                foreColor = this.ForeColor;
            }

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (this.UseTileImage)
            {
                if (this.TileImage != null)
                {
                    Rectangle imageRectangle;

                    switch (this.TileImageAlign)
                    {
                        case ContentAlignment.BottomLeft:
                            imageRectangle = new Rectangle(new Point(0, this.Height - this.TileImage.Height), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.BottomCenter:
                            imageRectangle = new Rectangle(new Point((this.Width / 2) - (this.TileImage.Width / 2), this.Height - this.TileImage.Height), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.BottomRight:
                            imageRectangle = new Rectangle(new Point(this.Width - this.TileImage.Width, this.Height - this.TileImage.Height), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.MiddleLeft:
                            imageRectangle = new Rectangle(new Point(0, (this.Height / 2) - (this.TileImage.Height / 2)), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.MiddleCenter:
                            imageRectangle = new Rectangle(new Point((this.Width / 2) - (this.TileImage.Width / 2), (this.Height / 2) - (this.TileImage.Height / 2)), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.MiddleRight:
                            imageRectangle = new Rectangle(new Point(this.Width - this.TileImage.Width, (this.Height / 2) - (this.TileImage.Height / 2)), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.TopLeft:
                            imageRectangle = new Rectangle(new Point(0, 0), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.TopCenter:
                            imageRectangle = new Rectangle(new Point((this.Width / 2) - (this.TileImage.Width / 2), 0), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        case ContentAlignment.TopRight:
                            imageRectangle = new Rectangle(new Point(this.Width - this.TileImage.Width, 0), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;

                        default:
                            imageRectangle = new Rectangle(new Point(0, 0), new Size(this.TileImage.Width, this.TileImage.Height));
                            break;
                    }

                    e.Graphics.DrawImage(this.TileImage, imageRectangle);
                }
            }

            if (this.TileCount > 0 && this.ShowTileCount)
            {
                Size countSize = TextRenderer.MeasureText(this.TileCount.ToString(), ModernFonts.TileCount);

                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                TextRenderer.DrawText(e.Graphics, this.TileCount.ToString(), ModernFonts.TileCount, new Point(this.Width - countSize.Width, 0), foreColor);

                e.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
            }

            Size textSize = TextRenderer.MeasureText(this.Text ?? string.Empty, ModernFonts.Tile(this.FontSize, this.FontWeight));

            TextFormatFlags flags = ModernPaint.GetTextFormatFlags(this.TextAlign) | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.EndEllipsis;

            Rectangle textRectangle = this.ClientRectangle;

            if (this._isPressed)
            {
                textRectangle.Inflate(-2, -2);
            }

            TextRenderer.DrawText(e.Graphics, this.Text ?? string.Empty, ModernFonts.Tile(this.FontSize, this.FontWeight), textRectangle, foreColor, flags);

            if (this._isFocused)
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