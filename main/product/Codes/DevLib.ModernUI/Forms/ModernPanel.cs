//-----------------------------------------------------------------------
// <copyright file="ModernPanel.cs" company="YuGuan Corporation">
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
    /// ModernPanel user control.
    /// </summary>
    [ToolboxBitmap(typeof(Panel))]
    [Designer(typeof(ParentControlDesigner))]
    public class ModernPanel : Panel, IModernControl
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
        /// Field _verticalScrollbar.
        /// </summary>
        private ModernScrollBar _verticalScrollbar = new ModernScrollBar(ModernScrollBarOrientation.Vertical);

        /// <summary>
        /// Field _horizontalScrollbar.
        /// </summary>
        private ModernScrollBar _horizontalScrollbar = new ModernScrollBar(ModernScrollBarOrientation.Horizontal);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernPanel"/> class.
        /// </summary>
        public ModernPanel()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.Controls.Add(this._verticalScrollbar);
            this.Controls.Add(this._horizontalScrollbar);

            this._verticalScrollbar.UseBarColor = true;
            this._horizontalScrollbar.UseBarColor = true;

            this._verticalScrollbar.Visible = false;
            this._horizontalScrollbar.Visible = false;

            this._verticalScrollbar.Scroll += this.VerticalScrollbarScroll;
            this._horizontalScrollbar.Scroll += this.HorizontalScrollbarScroll;
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
        /// Gets or sets a value indicating whether show horizontal scrollbar.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool ShowHorizontalScrollBar
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the horizontal scrollbar.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public int HorizontalScrollBarSize
        {
            get
            {
                return this._horizontalScrollbar.ScrollbarSize;
            }

            set
            {
                this._horizontalScrollbar.ScrollbarSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use horizontal scrollbar bar color.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseHorizontalBarColor
        {
            get
            {
                return this._horizontalScrollbar.UseBarColor;
            }

            set
            {
                this._horizontalScrollbar.UseBarColor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether highlight horizontal scrollbar on wheel.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool HighlightHorizontalScrollBarOnWheel
        {
            get
            {
                return this._horizontalScrollbar.HighlightOnWheel;
            }

            set
            {
                this._horizontalScrollbar.HighlightOnWheel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether show vertical scrollbar.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool ShowVerticalScrollBar
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the vertical scrollbar.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public int VerticalScrollBarSize
        {
            get
            {
                return this._verticalScrollbar.ScrollbarSize;
            }

            set
            {
                this._verticalScrollbar.ScrollbarSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use vertical scrollbar bar color.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseVerticalBarColor
        {
            get
            {
                return this._verticalScrollbar.UseBarColor;
            }

            set
            {
                this._verticalScrollbar.UseBarColor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether highlight vertical scrollbar highlight on wheel.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool HighlightVerticalScrollBarOnWheel
        {
            get
            {
                return this._verticalScrollbar.HighlightOnWheel;
            }

            set
            {
                this._verticalScrollbar.HighlightOnWheel = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public new bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }

            set
            {
                this.ShowHorizontalScrollBar = value;
                this.ShowVerticalScrollBar = value;

                base.AutoScroll = value;
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
                Color backColor = BackColor;

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
            if (this.DesignMode)
            {
                this._horizontalScrollbar.Visible = false;
                this._verticalScrollbar.Visible = false;
                return;
            }

            this.UpdateScrollBarPositions();

            if (this.ShowHorizontalScrollBar)
            {
                this._horizontalScrollbar.Visible = this.HorizontalScroll.Visible;
            }

            if (this.HorizontalScroll.Visible)
            {
                this._horizontalScrollbar.Minimum = this.HorizontalScroll.Minimum;
                this._horizontalScrollbar.Maximum = this.HorizontalScroll.Maximum;
                this._horizontalScrollbar.SmallChange = this.HorizontalScroll.SmallChange;
                this._horizontalScrollbar.LargeChange = this.HorizontalScroll.LargeChange;
            }

            if (this.ShowVerticalScrollBar)
            {
                this._verticalScrollbar.Visible = VerticalScroll.Visible;
            }

            if (this.VerticalScroll.Visible)
            {
                this._verticalScrollbar.Minimum = this.VerticalScroll.Minimum;
                this._verticalScrollbar.Maximum = this.VerticalScroll.Maximum;
                this._verticalScrollbar.SmallChange = this.VerticalScroll.SmallChange;
                this._verticalScrollbar.LargeChange = this.VerticalScroll.LargeChange;
            }

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            this._verticalScrollbar.Value = Math.Abs(this.VerticalScroll.Value);
            this._horizontalScrollbar.Value = Math.Abs(this.HorizontalScroll.Value);
        }

        /// <summary>
        /// WndProc method.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (!this.DesignMode)
            {
                WinApi.ShowScrollBar(this.Handle, (int)WinApi.ScrollBar.SB_BOTH, 0);
            }
        }

        /// <summary>
        /// Horizontals the scrollbar scroll.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void HorizontalScrollbarScroll(object sender, ScrollEventArgs e)
        {
            this.AutoScrollPosition = new Point(e.NewValue, this._verticalScrollbar.Value);
            this.UpdateScrollBarPositions();
        }

        /// <summary>
        /// Verticals the scrollbar scroll.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void VerticalScrollbarScroll(object sender, ScrollEventArgs e)
        {
            this.AutoScrollPosition = new Point(this._horizontalScrollbar.Value, e.NewValue);
            this.UpdateScrollBarPositions();
        }

        /// <summary>
        /// Updates the scroll bar positions.
        /// </summary>
        private void UpdateScrollBarPositions()
        {
            if (this.DesignMode)
            {
                return;
            }

            if (!this.AutoScroll)
            {
                this._verticalScrollbar.Visible = false;
                this._horizontalScrollbar.Visible = false;
                return;
            }

            this._verticalScrollbar.Location = new Point(this.ClientRectangle.Width - this._verticalScrollbar.Width, this.ClientRectangle.Y);
            this._verticalScrollbar.Height = this.ClientRectangle.Height - this._horizontalScrollbar.Height;

            if (!this.ShowVerticalScrollBar)
            {
                this._verticalScrollbar.Visible = false;
            }

            this._horizontalScrollbar.Location = new Point(this.ClientRectangle.X, this.ClientRectangle.Height - this._horizontalScrollbar.Height);
            this._horizontalScrollbar.Width = this.ClientRectangle.Width - this._verticalScrollbar.Width;

            if (!this.ShowHorizontalScrollBar)
            {
                this._horizontalScrollbar.Visible = false;
            }
        }
    }
}