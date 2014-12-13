//-----------------------------------------------------------------------
// <copyright file="ModernComboBox.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernComboBox user control.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBox))]
    public class ModernComboBox : ComboBox, IModernControl
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
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Regular;

        /// <summary>
        /// Field _promptText.
        /// </summary>
        private string _promptText = string.Empty;

        /// <summary>
        /// Field _drawPrompt.
        /// </summary>
        private bool _drawPrompt = false;

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
        /// Initializes a new instance of the <see cref="ModernComboBox" /> class.
        /// </summary>
        public ModernComboBox()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.DropDownStyle = ComboBoxStyle.DropDownList;

            this._drawPrompt = this.SelectedIndex == -1;

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
        [Browsable(true)]
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
        /// Gets or sets draw mode.
        /// </summary>
        [DefaultValue(DrawMode.OwnerDrawFixed)]
        [Browsable(false)]
        public new DrawMode DrawMode
        {
            get
            {
                return DrawMode.OwnerDrawFixed;
            }

            set
            {
                base.DrawMode = DrawMode.OwnerDrawFixed;
            }
        }

        /// <summary>
        /// Gets or sets drop down style.
        /// </summary>
        [DefaultValue(ComboBoxStyle.DropDownList)]
        [Browsable(false)]
        public new ComboBoxStyle DropDownStyle
        {
            get
            {
                return ComboBoxStyle.DropDownList;
            }

            set
            {
                base.DropDownStyle = ComboBoxStyle.DropDownList;
            }
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
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets prompt text.
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue("")]
        [Category(ModernConstants.PropertyCategoryName)]
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
        /// Gets or sets font.
        /// </summary>
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

            using (var g = this.CreateGraphics())
            {
                string measureText = this.Text.Length > 0 ? this.Text : "MeasureText";
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, measureText, ModernFonts.ComboBox(this.FontSize, this.FontWeight), proposedSize, TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.VerticalCenter);
                preferredSize.Height += 4;
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
            this.ItemHeight = this.GetPreferredSize(Size.Empty).Height;

            Color borderColor;
            Color foreColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Hover(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.ComboBox.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.ComboBox.Press(this.ThemeStyle);
                borderColor = ModernPaint.BorderColor.ComboBox.Press(this.ThemeStyle);
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

            Rectangle textRect = new Rectangle(2, 2, this.Width - 20, this.Height - 4);

            TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.ComboBox(this.FontSize, this.FontWeight), textRect, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, foreColor, e.Graphics));

            if (this.DisplayFocus && this._isFocused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, this.ClientRectangle);
            }

            if (this._drawPrompt)
            {
                this.DrawTextPrompt(e.Graphics);
            }
        }

        /// <summary>
        /// OnDrawItem method.
        /// </summary>
        /// <param name="e">DrawItemEventArgs instance.</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                Color foreColor;
                Color backColor = BackColor;

                if (!this.UseCustomBackColor)
                {
                    backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                }

                if (e.State == (DrawItemState.NoAccelerator | DrawItemState.NoFocusRect) || e.State == DrawItemState.None)
                {
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height));
                    }

                    foreColor = ModernPaint.ForeColor.Link.Normal(this.ThemeStyle);
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(ModernPaint.GetStyleColor(this.ColorStyle)))
                    {
                        e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height));
                    }

                    foreColor = ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
                }

                Rectangle textRectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, this.GetItemText(this.Items[e.Index]), ModernFonts.ComboBox(this.FontSize, this.FontWeight), textRectangle, foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            else
            {
                base.OnDrawItem(e);
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
        /// Raises the SelectedIndexChanged event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this._drawPrompt = this.SelectedIndex == -1;
            this.Invalidate();
        }

        /// <summary>
        /// WndProc method.
        /// </summary>
        /// <param name="m">A Windows Message object.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (((m.Msg == WM_PAINT) || (m.Msg == OCM_COMMAND)) && this._drawPrompt)
            {
                this.DrawTextPrompt();
            }
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
            Color backColor = this.BackColor;

            if (!this.UseCustomBackColor)
            {
                backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            }

            Rectangle textRectangle = new Rectangle(2, 2, this.Width - 20, this.Height - 4);
            TextRenderer.DrawText(g, this._promptText, ModernFonts.ComboBox(this.FontSize, this.FontWeight), textRectangle, SystemColors.GrayText, backColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}