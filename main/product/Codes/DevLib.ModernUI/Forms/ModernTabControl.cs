//-----------------------------------------------------------------------
// <copyright file="ModernTabControl.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.ComponentModel.Design;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernTabControl user control.
    /// </summary>
    [ToolboxBitmap(typeof(TabControl))]
    [Designer(typeof(ModernTabControlDesigner))]
    public class ModernTabControl : TabControl, IModernControl
    {
        /// <summary>
        /// Field TabBottomBorderHeight.
        /// </summary>
        private const int TabBottomBorderHeight = 3;

        /// <summary>
        /// Field WM_SETFONT.
        /// </summary>
        private const int WM_SETFONT = 0x30;

        /// <summary>
        /// Field WM_FONTCHANGE.
        /// </summary>
        private const int WM_FONTCHANGE = 0x1d;

        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _subClassUpDown.
        /// </summary>
        private SubClass _subClassUpDown = null;

        /// <summary>
        /// Field _bUpDown.
        /// </summary>
        private bool _bUpDown = false;

        /// <summary>
        /// Field _isMirrored.
        /// </summary>
        private bool _isMirrored;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTabControl" /> class.
        /// </summary>
        public ModernTabControl()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            this.Padding = new Point(6, 8);
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.FontSize = ModernFontSize.Medium;
            this.FontWeight = ModernFontWeight.Light;
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
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>
        /// The size of the font.
        /// </value>
        [DefaultValue(ModernFontSize.Medium)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFontSize FontSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        /// <value>
        /// The font weight.
        /// </value>
        [DefaultValue(ModernFontWeight.Light)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFontWeight FontWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        /// <value>
        /// The text align.
        /// </value>
        [DefaultValue(ContentAlignment.MiddleLeft)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ContentAlignment TextAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the collection of tab pages in this tab control.
        /// </summary>
        public new TabPageCollection TabPages
        {
            get
            {
                return base.TabPages;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is mirrored.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public new bool IsMirrored
        {
            get
            {
                return this._isMirrored;
            }

            set
            {
                if (this._isMirrored == value)
                {
                    return;
                }

                this._isMirrored = value;
                this.UpdateStyles();
            }
        }

        /// <summary>
        /// This member overrides <see cref="P:System.Windows.Forms.Control.CreateParams" />.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                const int WS_EX_LAYOUTRTL = 0x400000;
                const int WS_EX_NOINHERITLAYOUT = 0x100000;
                var cp = base.CreateParams;

                if (this._isMirrored)
                {
                    cp.ExStyle = cp.ExStyle | WS_EX_LAYOUTRTL | WS_EX_NOINHERITLAYOUT;
                }

                return cp;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CustomPaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ModernPaintEventArgs" /> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="ModernPaintEventArgs" /> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="ModernPaintEventArgs" /> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
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
        /// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPaintForeground(PaintEventArgs e)
        {
            for (int index = 0; index < this.TabPages.Count; index++)
            {
                if (index != this.SelectedIndex)
                {
                    this.DrawTab(index, e.Graphics);
                }
            }

            if (this.SelectedIndex <= -1)
            {
                return;
            }

            this.DrawTabBottomBorder(this.SelectedIndex, e.Graphics);
            this.DrawTab(this.SelectedIndex, e.Graphics);
            this.DrawTabSelected(this.SelectedIndex, e.Graphics);

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.EnabledChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged" /> event when the <see cref="P:System.Windows.Forms.Control.BackColor" /> property value of the control's container changes.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            this.Invalidate();
        }

        /// <summary>
        /// This member overrides <see cref="M:System.Windows.Forms.Control.OnResize(System.EventArgs)" />.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        /// <summary>
        /// This member overrides <see cref="M:System.Windows.Forms.Control.WndProc(System.Windows.Forms.Message@)" />.
        /// </summary>
        /// <param name="m">A Windows Message Object.</param>
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.SelectedIndex != -1)
            {
                if (!this.TabPages[this.SelectedIndex].Focused)
                {
                    bool subControlFocused = false;

                    foreach (Control control in this.TabPages[this.SelectedIndex].Controls)
                    {
                        if (control.Focused)
                        {
                            subControlFocused = true;
                            return;
                        }
                    }

                    if (!subControlFocused)
                    {
                        this.TabPages[this.SelectedIndex].Select();
                        this.TabPages[this.SelectedIndex].Focus();
                    }
                }
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl" /> method.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.OnFontChanged(EventArgs.Empty);
            this.FindUpDown();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ControlAdded" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.ControlEventArgs" /> that contains the event data.</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            this.FindUpDown();
            this.UpdateUpDown();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ControlRemoved" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.ControlEventArgs" /> that contains the event data.</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            this.FindUpDown();
            this.UpdateUpDown();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.TabControl.SelectedIndexChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.UpdateUpDown();
            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        [SecuritySafeCritical]
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            IntPtr hFont = ModernFonts.TabControl(this.FontSize, this.FontWeight).ToHfont();
            WinApi.SendMessage(this.Handle, WM_SETFONT, hFont, (IntPtr)(-1));
            WinApi.SendMessage(this.Handle, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
            this.UpdateStyles();
        }

        /// <summary>
        /// Draws the tab bottom border.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="graphics">The graphics.</param>
        private void DrawTabBottomBorder(int index, Graphics graphics)
        {
            using (Brush bgBrush = new SolidBrush(ModernPaint.BorderColor.TabControl.Normal(this.ThemeStyle)))
            {
                Rectangle borderRectangle = new Rectangle(this.DisplayRectangle.X, this.GetTabRect(index).Bottom + 2 - TabBottomBorderHeight, this.DisplayRectangle.Width, TabBottomBorderHeight);
                graphics.FillRectangle(bgBrush, borderRectangle);
            }
        }

        /// <summary>
        /// Draws the tab selected.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="graphics">The graphics.</param>
        private void DrawTabSelected(int index, Graphics graphics)
        {
            using (Brush selectionBrush = new SolidBrush(ModernPaint.GetStyleColor(this.ColorStyle)))
            {
                Rectangle selectedTabRect = this.GetTabRect(index);
                Rectangle borderRectangle = new Rectangle(selectedTabRect.X + ((index == 0) ? 2 : 0), this.GetTabRect(index).Bottom + 2 - TabBottomBorderHeight, selectedTabRect.Width + ((index == 0) ? 0 : 2), TabBottomBorderHeight);
                graphics.FillRectangle(selectionBrush, borderRectangle);
            }
        }

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Preferred size.</returns>
        private Size MeasureText(string text)
        {
            Size preferredSize;

            using (Graphics g = CreateGraphics())
            {
                Size proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, text ?? string.Empty, ModernFonts.TabControl(this.FontSize, this.FontWeight), proposedSize, ModernPaint.GetTextFormatFlags(this.TextAlign) | TextFormatFlags.NoPadding);
            }

            return preferredSize;
        }

        /// <summary>
        /// Draws the tab.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="graphics">The graphics.</param>
        private void DrawTab(int index, Graphics graphics)
        {
            Color foreColor;
            Color backColor = this.BackColor;

            if (!this.UseCustomBackColor)
            {
                backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            }

            TabPage tabPage = this.TabPages[index];
            Rectangle tabRectangle = this.GetTabRect(index);

            if (!this.Enabled)
            {
                foreColor = ModernPaint.ForeColor.Label.Disabled(this.ThemeStyle);
            }
            else
            {
                if (this.UseCustomForeColor)
                {
                    foreColor = Control.DefaultForeColor;
                }
                else
                {
                    foreColor = !this.UseStyleColors ? ModernPaint.ForeColor.TabControl.Normal(this.ThemeStyle) : ModernPaint.GetStyleColor(this.ColorStyle);
                }
            }

            if (index == 0)
            {
                tabRectangle.X = this.DisplayRectangle.X;
            }

            Rectangle bgRectangle = tabRectangle;

            tabRectangle.Width += 20;

            using (Brush bgBrush = new SolidBrush(backColor))
            {
                graphics.FillRectangle(bgBrush, bgRectangle);
            }

            TextRenderer.DrawText(graphics, tabPage.Text ?? string.Empty, ModernFonts.TabControl(this.FontSize, this.FontWeight), tabRectangle, foreColor, backColor, ModernPaint.GetTextFormatFlags(this.TextAlign));
        }

        /// <summary>
        /// Draws up down.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        [SecuritySafeCritical]
        private void DrawUpDown(Graphics graphics)
        {
            Color backColor = this.Parent != null ? this.Parent.BackColor : ModernPaint.BackColor.Form(this.ThemeStyle);

            Rectangle borderRectangle = new Rectangle();
            WinApi.GetClientRect(this._subClassUpDown.Handle, ref borderRectangle);

            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            graphics.Clear(backColor);

            using (Brush brush = new SolidBrush(ModernPaint.BorderColor.TabControl.Normal(this.ThemeStyle)))
            {
                using (GraphicsPath gp = new GraphicsPath(FillMode.Winding))
                {
                    PointF[] pts = { new PointF(6, 6), new PointF(16, 0), new PointF(16, 12) };
                    gp.AddLines(pts);
                    graphics.FillPath(brush, gp);

                    gp.Reset();

                    PointF[] pts2 = { new PointF(borderRectangle.Width - 15, 0), new PointF(borderRectangle.Width - 5, 6), new PointF(borderRectangle.Width - 15, 12) };
                    gp.AddLines(pts2);
                    graphics.FillPath(brush, gp);
                }
            }
        }

        /// <summary>
        /// Returns the bounding rectangle for a specified tab in this tab control.
        /// </summary>
        /// <param name="index">The zero-based index of the tab you want.</param>
        /// <returns>A <see cref="T:System.Drawing.Rectangle" /> that represents the bounds of the specified tab.</returns>
        private new Rectangle GetTabRect(int index)
        {
            if (index < 0)
            {
                return new Rectangle();
            }

            Rectangle baseRectangle = base.GetTabRect(index);

            return baseRectangle;
        }

        /// <summary>
        /// Finds up down.
        /// </summary>
        [SecuritySafeCritical]
        private void FindUpDown()
        {
            if (!this.DesignMode)
            {
                bool bFound = false;

                IntPtr pWnd = WinApi.GetWindow(this.Handle, WinApi.GW_CHILD);

                while (pWnd != IntPtr.Zero)
                {
                    char[] buffer = new char[33];

                    int length = WinApi.GetClassName(pWnd, buffer, 32);

                    string className = new string(buffer, 0, length);

                    if (className == "msctls_updown32")
                    {
                        bFound = true;

                        if (!this._bUpDown)
                        {
                            this._subClassUpDown = new SubClass(pWnd, true);
                            this._subClassUpDown.SubClassedWndProc += this.OnSubClassUpDownSubClassedWndProc;

                            this._bUpDown = true;
                        }

                        break;
                    }

                    pWnd = WinApi.GetWindow(pWnd, WinApi.GW_HWNDNEXT);
                }

                if (!bFound && this._bUpDown)
                {
                    this._bUpDown = false;
                }
            }
        }

        /// <summary>
        /// Updates up down.
        /// </summary>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void UpdateUpDown()
        {
            if (this._bUpDown && !this.DesignMode)
            {
                if (WinApi.IsWindowVisible(this._subClassUpDown.Handle))
                {
                    Rectangle rectangle = new Rectangle();
                    WinApi.GetClientRect(this._subClassUpDown.Handle, ref rectangle);
                    WinApi.InvalidateRect(this._subClassUpDown.Handle, ref rectangle, true);
                }
            }
        }

        /// <summary>
        /// OnSubClassUpDownSubClassedWndProc method.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        /// <returns>Result code.</returns>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private int OnSubClassUpDownSubClassedWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WinApi.Messages.WM_PAINT:

                    IntPtr hDC = WinApi.GetWindowDC(this._subClassUpDown.Handle);

                    Graphics g = Graphics.FromHdc(hDC);

                    this.DrawUpDown(g);

                    g.Dispose();

                    WinApi.ReleaseDC(this._subClassUpDown.Handle, hDC);

                    m.Result = IntPtr.Zero;

                    Rectangle rectangle = new Rectangle();

                    WinApi.GetClientRect(this._subClassUpDown.Handle, ref rectangle);
                    WinApi.ValidateRect(this._subClassUpDown.Handle, ref rectangle);

                    return 1;
            }

            return 0;
        }

        /// <summary>
        /// ModernTabPage Collection.
        /// </summary>
        [ToolboxItem(false)]
        [Editor(typeof(ModernTabPageCollectionEditor), typeof(CollectionEditor))]
        public class ModernTabPageCollection : TabControl.TabPageCollection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModernTabPageCollection" /> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public ModernTabPageCollection(ModernTabControl owner)
                : base(owner)
            {
            }
        }
    }
}