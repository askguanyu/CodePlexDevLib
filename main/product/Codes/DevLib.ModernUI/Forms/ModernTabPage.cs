//-----------------------------------------------------------------------
// <copyright file="ModernTabPage.cs" company="YuGuan Corporation">
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
    /// ModernTabPage user control.
    /// </summary>
    [ToolboxItem(false)]
    [Designer(typeof(ScrollableControlDesigner))]
    public class ModernTabPage : TabPage, IModernControl
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
        /// Field _verticalScrollBar.
        /// </summary>
        private ModernScrollBar _verticalScrollBar = new ModernScrollBar(ModernScrollBarOrientation.Vertical);

        /// <summary>
        /// Field _horizontalScrollBar.
        /// </summary>
        private ModernScrollBar _horizontalScrollBar = new ModernScrollBar(ModernScrollBarOrientation.Horizontal);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTabPage"/> class.
        /// </summary>
        public ModernTabPage()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.Controls.Add(this._verticalScrollBar);
            this.Controls.Add(this._horizontalScrollBar);

            this._verticalScrollBar.UseBarColor = true;
            this._horizontalScrollBar.UseBarColor = true;

            this._verticalScrollBar.Scroll += this.VerticalScrollBarScroll;
            this._horizontalScrollBar.Scroll += this.HorizontalScrollBarScroll;
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
        [Browsable(true)]
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
                return this._horizontalScrollBar.ScrollbarSize;
            }

            set
            {
                this._horizontalScrollBar.ScrollbarSize = value;
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
                return this._horizontalScrollBar.UseBarColor;
            }

            set
            {
                this._horizontalScrollBar.UseBarColor = value;
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
                return this._horizontalScrollBar.HighlightOnWheel;
            }

            set
            {
                this._horizontalScrollBar.HighlightOnWheel = value;
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
                return this._verticalScrollBar.ScrollbarSize;
            }

            set
            {
                this._verticalScrollBar.ScrollbarSize = value;
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
                return this._verticalScrollBar.UseBarColor;
            }

            set
            {
                this._verticalScrollBar.UseBarColor = value;
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
                return this._verticalScrollBar.HighlightOnWheel;
            }

            set
            {
                this._verticalScrollBar.HighlightOnWheel = value;
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
                if (value)
                {
                    this.ShowHorizontalScrollBar = true;
                    this.ShowVerticalScrollBar = true;
                }

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
        /// Paints the background of the <see cref="T:System.Windows.Forms.TabPage" />.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains data useful for painting the background.</param>
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
            base.OnPaint(e);

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
                this._horizontalScrollBar.Visible = false;
                this._verticalScrollBar.Visible = false;
                return;
            }

            this.UpdateScrollBarPositions();

            if (this.ShowHorizontalScrollBar)
            {
                this._horizontalScrollBar.Visible = this.HorizontalScroll.Visible;
            }

            if (this.HorizontalScroll.Visible)
            {
                this._horizontalScrollBar.Minimum = this.HorizontalScroll.Minimum;
                this._horizontalScrollBar.Maximum = this.HorizontalScroll.Maximum;
                this._horizontalScrollBar.SmallChange = this.HorizontalScroll.SmallChange;
                this._horizontalScrollBar.LargeChange = this.HorizontalScroll.LargeChange;
            }

            if (this.ShowVerticalScrollBar)
            {
                this._verticalScrollBar.Visible = this.VerticalScroll.Visible;
            }

            if (this.VerticalScroll.Visible)
            {
                this._verticalScrollBar.Minimum = this.VerticalScroll.Minimum;
                this._verticalScrollBar.Maximum = this.VerticalScroll.Maximum;
                this._verticalScrollBar.SmallChange = this.VerticalScroll.SmallChange;
                this._verticalScrollBar.LargeChange = this.VerticalScroll.LargeChange;
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

            this._verticalScrollBar.Value = this.VerticalScroll.Value;
            this._horizontalScrollBar.Value = this.HorizontalScroll.Value;
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
        private void HorizontalScrollBarScroll(object sender, ScrollEventArgs e)
        {
            this.AutoScrollPosition = new Point(e.NewValue, this._verticalScrollBar.Value);
            this.UpdateScrollBarPositions();
        }

        /// <summary>
        /// Verticals the scrollbar scroll.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> instance containing the event data.</param>
        private void VerticalScrollBarScroll(object sender, ScrollEventArgs e)
        {
            this.AutoScrollPosition = new Point(this._horizontalScrollBar.Value, e.NewValue);
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
                this._verticalScrollBar.Visible = false;
                this._horizontalScrollBar.Visible = false;
                return;
            }

            this._verticalScrollBar.Location = new Point(this.ClientRectangle.Width - this._verticalScrollBar.Width, this.ClientRectangle.Y);
            this._verticalScrollBar.Height = this.ClientRectangle.Height;

            if (!this.ShowVerticalScrollBar)
            {
                this._verticalScrollBar.Visible = false;
            }

            this._horizontalScrollBar.Location = new Point(this.ClientRectangle.X, this.ClientRectangle.Height - this._horizontalScrollBar.Height);
            this._horizontalScrollBar.Width = this.ClientRectangle.Width;

            if (!this.ShowHorizontalScrollBar)
            {
                this._horizontalScrollBar.Visible = false;
            }
        }
    }
}