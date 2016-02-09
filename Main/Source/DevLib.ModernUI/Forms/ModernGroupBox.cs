//-----------------------------------------------------------------------
// <copyright file="ModernGroupBox.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernGroupBox user control.
    /// </summary>
    [ToolboxBitmap(typeof(GroupBox))]
    [Designer(typeof(ParentControlDesigner))]
    public class ModernGroupBox : GroupBox, IModernControl
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
        /// Field _textAlign.
        /// </summary>
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;

        /// <summary>
        /// Field _headerHeight.
        /// </summary>
        private uint _headerHeight = 22;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernGroupBox" /> class.
        /// </summary>
        public ModernGroupBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
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
        ///  Gets or sets control text
        /// </summary>
        [Browsable(true)]
        [DefaultValue("")]
        [Category(ModernConstants.PropertyCategoryName)]
        public new string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                base.Text = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ContentAlignment TextAlign
        {
            get
            {
                return this._textAlign;
            }

            set
            {
                this._textAlign = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the height of the header.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(22)]
        [Category(ModernConstants.PropertyCategoryName)]
        public uint HeaderHeight
        {
            get
            {
                return this._headerHeight;
            }

            set
            {
                this._headerHeight = value;
                this.Invalidate();
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
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
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

                Color foreColor = this.UseCustomForeColor ? this.ForeColor : ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
                Color borderColor = ModernPaint.GetStyleColor(this.ColorStyle);

                using (Pen pen = new Pen(borderColor))
                {
                    e.Graphics.DrawLines(
                        pen,
                        new[]
                        {
                            new Point(0, 0),
                            new Point(0, this.Height - 1),
                            new Point(this.Width - 1, this.Height - 1),
                            new Point(this.Width - 1, 0),
                            new Point(0, 0)
                        });
                }

                using (SolidBrush brush = ModernPaint.GetStyleBrush(this.ColorStyle))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, this.Width, this.HeaderHeight);
                }

                Rectangle textRectangle = new Rectangle(0, 0, this.Width, (int)this.HeaderHeight);
                TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.CheckBox(this.FontSize, this.FontWeight), textRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign));

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
            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
        }
    }
}
