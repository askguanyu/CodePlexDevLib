//-----------------------------------------------------------------------
// <copyright file="ModernScrollBar.cs" company="YuGuan Corporation">
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
    /// ModernScrollBar user control.
    /// </summary>
    [DefaultEvent("Scroll")]
    [DefaultProperty("Value")]
    [Designer(typeof(ScrollableControlDesigner), typeof(ParentControlDesigner))]
    public class ModernScrollBar : Control, IModernControl
    {
        /// <summary>
        /// Field _progressTimer.
        /// </summary>
        private readonly Timer _progressTimer = new Timer();

        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Default;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Default;

        /// <summary>
        /// Field _isFirstScrollEventVertical.
        /// </summary>
        private bool _isFirstScrollEventVertical = true;

        /// <summary>
        /// Field _isFirstScrollEventHorizontal.
        /// </summary>
        private bool _isFirstScrollEventHorizontal = true;

        /// <summary>
        /// Field _inUpdate.
        /// </summary>
        private bool _inUpdate;

        /// <summary>
        /// Field _clickedBarRectangle.
        /// </summary>
        private Rectangle _clickedBarRectangle;

        /// <summary>
        /// Field _thumbRectangle.
        /// </summary>
        private Rectangle _thumbRectangle;

        /// <summary>
        /// Field _topBarClicked.
        /// </summary>
        private bool _topBarClicked;

        /// <summary>
        /// Field _bottomBarClicked.
        /// </summary>
        private bool _bottomBarClicked;

        /// <summary>
        /// Field _thumbClicked.
        /// </summary>
        private bool _thumbClicked;

        /// <summary>
        /// Field _thumbWidth.
        /// </summary>
        private int _thumbWidth = 6;

        /// <summary>
        /// Field _thumbHeight.
        /// </summary>
        private int _thumbHeight;

        /// <summary>
        /// Field _thumbBottomLimitBottom.
        /// </summary>
        private int _thumbBottomLimitBottom;

        /// <summary>
        /// Field _thumbBottomLimitTop.
        /// </summary>
        private int _thumbBottomLimitTop;

        /// <summary>
        /// Field _thumbTopLimit.
        /// </summary>
        private int _thumbTopLimit;

        /// <summary>
        /// Field _thumbPosition.
        /// </summary>
        private int _thumbPosition;

        /// <summary>
        /// Field _trackPosition.
        /// </summary>
        private int _trackPosition;

        /// <summary>
        /// Field _mouseWheelBarPartitions.
        /// </summary>
        private int _mouseWheelBarPartitions = 10;

        /// <summary>
        /// Field _isHovered.
        /// </summary>
        private bool _isHovered;

        /// <summary>
        /// Field _isPressed.
        /// </summary>
        private bool _isPressed;

        /// <summary>
        /// Field _modernScrollBarOrientation.
        /// </summary>
        private ModernScrollBarOrientation _modernScrollBarOrientation = ModernScrollBarOrientation.Vertical;

        /// <summary>
        /// Field _scrollOrientation.
        /// </summary>
        private ScrollOrientation _scrollOrientation = ScrollOrientation.VerticalScroll;

        /// <summary>
        /// Field _minimum.
        /// </summary>
        private int _minimum;

        /// <summary>
        /// Field _maximum.
        /// </summary>
        private int _maximum = 100;

        /// <summary>
        /// Field _smallChange.
        /// </summary>
        private int _smallChange = 1;

        /// <summary>
        /// Field _largeChange.
        /// </summary>
        private int _largeChange = 10;

        /// <summary>
        /// Field _currentValue.
        /// </summary>
        private int _currentValue;

        /// <summary>
        /// Field _disableUpdateColor.
        /// </summary>
        private bool _disableUpdateColor = false;

        /// <summary>
        /// Field _autoHoverTimer.
        /// </summary>
        private Timer _autoHoverTimer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernScrollBar"/> class.
        /// </summary>
        public ModernScrollBar()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);

            this.Width = 10;
            this.Height = 200;

            this.SetupScrollBar();

            this._progressTimer.Interval = 20;
            this._progressTimer.Tick += this.ProgressTimerTick;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernScrollBar"/> class.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public ModernScrollBar(ModernScrollBarOrientation orientation)
            : this()
        {
            this.Orientation = orientation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernScrollBar"/> class.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <param name="width">The width.</param>
        public ModernScrollBar(ModernScrollBarOrientation orientation, int width)
            : this(orientation)
        {
            this.Width = width;
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
        /// Occurs when [scroll].
        /// </summary>
        public event ScrollEventHandler Scroll;

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
        [Browsable(false)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use StyleColors.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// Gets or sets the mouse wheel bar partitions.
        /// </summary>
        public int MouseWheelBarPartitions
        {
            get
            {
                return this._mouseWheelBarPartitions;
            }

            set
            {
                if (value > 0)
                {
                    this._mouseWheelBarPartitions = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("MouseWheelBarPartitions", "MouseWheelBarPartitions has to be greather than zero");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use bar color.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseBarColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the scrollbar.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public int ScrollbarSize
        {
            get
            {
                return this.Orientation == ModernScrollBarOrientation.Vertical ? this.Width : this.Height;
            }

            set
            {
                if (this.Orientation == ModernScrollBarOrientation.Vertical)
                {
                    this.Width = value;
                }
                else
                {
                    this.Height = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether highlight on wheel.
        /// </summary>
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool HighlightOnWheel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public ModernScrollBarOrientation Orientation
        {
            get
            {
                return this._modernScrollBarOrientation;
            }

            set
            {
                if (value == this._modernScrollBarOrientation)
                {
                    return;
                }

                this._modernScrollBarOrientation = value;

                if (value == ModernScrollBarOrientation.Vertical)
                {
                    this._scrollOrientation = ScrollOrientation.VerticalScroll;
                }
                else
                {
                    this._scrollOrientation = ScrollOrientation.HorizontalScroll;
                }

                this.Size = new Size(this.Height, this.Width);
                this.SetupScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public int Minimum
        {
            get
            {
                return this._minimum;
            }

            set
            {
                if (this._minimum == value || value < 0 || value >= this._maximum)
                {
                    return;
                }

                this._minimum = value;

                if (this._currentValue < value)
                {
                    this._currentValue = value;
                }

                if (this._largeChange > (this._maximum - this._minimum))
                {
                    this._largeChange = this._maximum - this._minimum;
                }

                this.SetupScrollBar();

                if (this._currentValue < value)
                {
                    this._disableUpdateColor = true;
                    this.Value = value;
                }
                else
                {
                    this.ChangeThumbPosition(this.GetThumbPosition());
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public int Maximum
        {
            get
            {
                return this._maximum;
            }

            set
            {
                if (value == this._maximum || value < 1 || value <= this._minimum)
                {
                    return;
                }

                this._maximum = value;

                if (this._largeChange > (this._maximum - this._minimum))
                {
                    this._largeChange = this._maximum - this._minimum;
                }

                this.SetupScrollBar();

                if (this._currentValue > value)
                {
                    this._disableUpdateColor = true;
                    this.Value = this._maximum;
                }
                else
                {
                    this.ChangeThumbPosition(this.GetThumbPosition());
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the small change.
        /// </summary>
        [DefaultValue(1)]
        public int SmallChange
        {
            get
            {
                return this._smallChange;
            }

            set
            {
                if (value == this._smallChange || value < 1 || value >= this._largeChange)
                {
                    return;
                }

                this._smallChange = value;
                this.SetupScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the large change.
        /// </summary>
        [DefaultValue(5)]
        public int LargeChange
        {
            get
            {
                return this._largeChange;
            }

            set
            {
                if (value == this._largeChange || value < this._smallChange || value < 2)
                {
                    return;
                }

                if (value > (this._maximum - this._minimum))
                {
                    this._largeChange = this._maximum - this._minimum;
                }
                else
                {
                    this._largeChange = value;
                }

                this.SetupScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the value of scroll position.
        /// </summary>
        [DefaultValue(0)]
        [Browsable(false)]
        public int Value
        {
            get
            {
                return this._currentValue;
            }

            set
            {
                if (this._currentValue == value || value < this._minimum || value > this._maximum)
                {
                    return;
                }

                this._currentValue = value;

                this.ChangeThumbPosition(this.GetThumbPosition());

                this.OnScroll(ScrollEventType.ThumbPosition, -1, value, this._scrollOrientation);

                if (!this._disableUpdateColor && this.HighlightOnWheel)
                {
                    if (!this._isHovered)
                    {
                        this._isHovered = true;
                    }

                    if (this._autoHoverTimer == null)
                    {
                        this._autoHoverTimer = new Timer();
                        this._autoHoverTimer.Interval = 1000;
                        this._autoHoverTimer.Tick += this.AutoHoverTimerTick;
                        this._autoHoverTimer.Start();
                    }
                    else
                    {
                        this._autoHoverTimer.Stop();
                        this._autoHoverTimer.Start();
                    }
                }
                else
                {
                    this._disableUpdateColor = false;
                }

                this.Refresh();
            }
        }

        /// <summary>
        /// Hits the test.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>This method returns true if the point represented by pt is contained within this System.Drawing.Rectangle structure; otherwise false.</returns>
        public bool HitTest(Point point)
        {
            return this._thumbRectangle.Contains(point);
        }

        /// <summary>
        /// Begins the update.
        /// </summary>
        [SecuritySafeCritical]
        public void BeginUpdate()
        {
            WinApi.SendMessage(this.Handle, (int)WinApi.Messages.WM_SETREDRAW, false, 0);
            this._inUpdate = true;
        }

        /// <summary>
        /// Ends the update.
        /// </summary>
        [SecuritySafeCritical]
        public void EndUpdate()
        {
            WinApi.SendMessage(this.Handle, (int)WinApi.Messages.WM_SETREDRAW, true, 0);
            this._inUpdate = false;
            this.SetupScrollBar();
            this.Refresh();
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
        /// Raises the <see cref="E:PaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                Color backColor = this.BackColor;

                if (!this.UseCustomBackColor)
                {
                    if (this.Parent != null)
                    {
                        if (this.Parent is IModernControl)
                        {
                            backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                        }
                        else
                        {
                            backColor = this.Parent.BackColor;
                        }
                    }
                    else
                    {
                        backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                    }
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
            Color backColor;
            Color thumbColor;
            Color barColor;

            if (this.UseCustomBackColor)
            {
                backColor = this.BackColor;
            }
            else
            {
                if (this.Parent != null)
                {
                    if (this.Parent is IModernControl)
                    {
                        backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                    }
                    else
                    {
                        backColor = this.Parent.BackColor;
                    }
                }
                else
                {
                    backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
                }
            }

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.ScrollBar.Thumb.Hover(this.ThemeStyle);
                barColor = ModernPaint.BackColor.ScrollBar.Bar.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.ScrollBar.Thumb.Press(this.ThemeStyle);
                barColor = ModernPaint.BackColor.ScrollBar.Bar.Press(this.ThemeStyle);
            }
            else if (!this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.ScrollBar.Thumb.Disabled(this.ThemeStyle);
                barColor = ModernPaint.BackColor.ScrollBar.Bar.Disabled(this.ThemeStyle);
            }
            else
            {
                thumbColor = ModernPaint.BackColor.ScrollBar.Thumb.Normal(this.ThemeStyle);
                barColor = ModernPaint.BackColor.ScrollBar.Bar.Normal(this.ThemeStyle);
            }

            this.DrawScrollBar(e.Graphics, backColor, thumbColor, barColor);

            this.OnCustomPaintForeground(new ModernPaintEventArgs(backColor, thumbColor, e.Graphics));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            this.Invalidate();

            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnLostFocus(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnter(EventArgs e)
        {
            this.Invalidate();

            base.OnEnter(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int v = e.Delta / 120 * (this._maximum - this._minimum) / this._mouseWheelBarPartitions;

            if (this.Orientation == ModernScrollBarOrientation.Vertical)
            {
                this.Value -= v;
            }
            else
            {
                this.Value += v;
            }
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

            this.Focus();

            if (e.Button == MouseButtons.Left)
            {
                var mouseLocation = e.Location;

                if (this._thumbRectangle.Contains(mouseLocation))
                {
                    this._thumbClicked = true;
                    this._thumbPosition = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? mouseLocation.Y - this._thumbRectangle.Y : mouseLocation.X - this._thumbRectangle.X;

                    this.Invalidate(this._thumbRectangle);
                }
                else
                {
                    this._trackPosition = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? mouseLocation.Y : mouseLocation.X;

                    if (this._trackPosition < (this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? this._thumbRectangle.Y : this._thumbRectangle.X))
                    {
                        this._topBarClicked = true;
                    }
                    else
                    {
                        this._bottomBarClicked = true;
                    }

                    this.ProgressThumb(true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this._trackPosition = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? e.Y : e.X;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            this._isPressed = false;

            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                if (this._thumbClicked)
                {
                    this._thumbClicked = false;
                    this.OnScroll(ScrollEventType.EndScroll, -1, this._currentValue, this._scrollOrientation);
                }
                else if (this._topBarClicked)
                {
                    this._topBarClicked = false;
                    this.StopTimer();
                }
                else if (this._bottomBarClicked)
                {
                    this._bottomBarClicked = false;
                    this.StopTimer();
                }

                this.Invalidate();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            this._isHovered = true;
            this.Invalidate();

            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            this._isHovered = false;
            this.Invalidate();

            base.OnMouseLeave(e);

            this.ResetScrollStatus();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                if (this._thumbClicked)
                {
                    int oldScrollValue = this._currentValue;

                    int pos = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;
                    int thumbSize = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? (pos / Height) / this._thumbHeight : (pos / Width) / this._thumbWidth;

                    if (pos <= (this._thumbTopLimit + this._thumbPosition))
                    {
                        this.ChangeThumbPosition(this._thumbTopLimit);
                        this._currentValue = this._minimum;
                        this.Invalidate();
                    }
                    else if (pos >= (this._thumbBottomLimitTop + this._thumbPosition))
                    {
                        this.ChangeThumbPosition(this._thumbBottomLimitTop);
                        this._currentValue = this._maximum;
                        this.Invalidate();
                    }
                    else
                    {
                        this.ChangeThumbPosition(pos - this._thumbPosition);

                        int pixelRange;
                        int thumbPos;

                        if (this.Orientation == ModernScrollBarOrientation.Vertical)
                        {
                            pixelRange = this.Height - thumbSize;
                            thumbPos = this._thumbRectangle.Y;
                        }
                        else
                        {
                            pixelRange = this.Width - thumbSize;
                            thumbPos = this._thumbRectangle.X;
                        }

                        float perc = 0f;

                        if (pixelRange != 0)
                        {
                            perc = thumbPos / (float)pixelRange;
                        }

                        this._currentValue = Convert.ToInt32((perc * (this._maximum - this._minimum)) + this._minimum);
                    }

                    if (oldScrollValue != this._currentValue)
                    {
                        this.OnScroll(ScrollEventType.ThumbTrack, oldScrollValue, this._currentValue, this._scrollOrientation);
                        this.Refresh();
                    }
                }
            }
            else if (!this.ClientRectangle.Contains(e.Location))
            {
                this.ResetScrollStatus();
            }
            else if (e.Button == MouseButtons.None)
            {
                if (this._thumbRectangle.Contains(e.Location))
                {
                    this.Invalidate(this._thumbRectangle);
                }
                else if (this.ClientRectangle.Contains(e.Location))
                {
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            this._isHovered = true;
            this._isPressed = true;
            this.Invalidate();

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left" /> property value of the control.</param>
        /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top" /> property value of the control.</param>
        /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width" /> property value of the control.</param>
        /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height" /> property value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified" /> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            if (this.DesignMode)
            {
                this.SetupScrollBar();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.SetupScrollBar();
        }

        /// <summary>
        /// Processes a dialog key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns>
        /// true if the key was processed by the control; otherwise, false.
        /// </returns>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            var keyUp = Keys.Up;
            var keyDown = Keys.Down;

            if (this.Orientation == ModernScrollBarOrientation.Horizontal)
            {
                keyUp = Keys.Left;
                keyDown = Keys.Right;
            }

            if (keyData == keyUp)
            {
                this.Value -= this._smallChange;

                return true;
            }

            if (keyData == keyDown)
            {
                this.Value += this._smallChange;

                return true;
            }

            if (keyData == Keys.PageUp)
            {
                this.Value = this.GetValue(false, true);

                return true;
            }

            if (keyData == Keys.PageDown)
            {
                if (this._currentValue + this._largeChange > this._maximum)
                {
                    this.Value = this._maximum;
                }
                else
                {
                    this.Value += this._largeChange;
                }

                return true;
            }

            if (keyData == Keys.Home)
            {
                this.Value = this._minimum;

                return true;
            }

            if (keyData == Keys.End)
            {
                this.Value = this._maximum;

                return true;
            }

            return base.ProcessDialogKey(keyData);
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
        /// Called when scroll.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="orientation">The orientation.</param>
        private void OnScroll(ScrollEventType type, int oldValue, int newValue, ScrollOrientation orientation)
        {
            if (this.Scroll == null)
            {
                return;
            }

            if (orientation == ScrollOrientation.HorizontalScroll)
            {
                if (type != ScrollEventType.EndScroll && this._isFirstScrollEventHorizontal)
                {
                    type = ScrollEventType.First;
                }
                else if (!this._isFirstScrollEventHorizontal && type == ScrollEventType.EndScroll)
                {
                    this._isFirstScrollEventHorizontal = true;
                }
            }
            else
            {
                if (type != ScrollEventType.EndScroll && this._isFirstScrollEventVertical)
                {
                    type = ScrollEventType.First;
                }
                else if (!this._isFirstScrollEventHorizontal && type == ScrollEventType.EndScroll)
                {
                    this._isFirstScrollEventVertical = true;
                }
            }

            this.Scroll(this, new ScrollEventArgs(type, oldValue, newValue, orientation));
        }

        /// <summary>
        /// Handles the Tick event of the autoHoverTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AutoHoverTimerTick(object sender, EventArgs e)
        {
            this._isHovered = false;
            this.Invalidate();
            this._autoHoverTimer.Stop();
        }

        /// <summary>
        /// Draws the scroll bar.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="backColor">Color of the back.</param>
        /// <param name="thumbColor">Color of the thumb.</param>
        /// <param name="barColor">Color of the bar.</param>
        private void DrawScrollBar(Graphics g, Color backColor, Color thumbColor, Color barColor)
        {
            if (this.UseBarColor)
            {
                using (var brush = new SolidBrush(barColor))
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }
            }

            using (var brush = new SolidBrush(backColor))
            {
                var thumbRect = new Rectangle(this._thumbRectangle.X - 1, this._thumbRectangle.Y - 1, this._thumbRectangle.Width + 2, this._thumbRectangle.Height + 2);
                g.FillRectangle(brush, thumbRect);
            }

            using (var brush = new SolidBrush(thumbColor))
            {
                g.FillRectangle(brush, this._thumbRectangle);
            }
        }

        /// <summary>
        /// Setups the scroll bar.
        /// </summary>
        private void SetupScrollBar()
        {
            if (this._inUpdate)
            {
                return;
            }

            if (this.Orientation == ModernScrollBarOrientation.Vertical)
            {
                this._thumbWidth = this.Width > 0 ? this.Width : 10;
                this._thumbHeight = this.GetThumbSize();

                this._clickedBarRectangle = this.ClientRectangle;
                this._clickedBarRectangle.Inflate(-1, -1);

                this._thumbRectangle = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this._thumbWidth, this._thumbHeight);

                this._thumbPosition = this._thumbRectangle.Height / 2;
                this._thumbBottomLimitBottom = this.ClientRectangle.Bottom;
                this._thumbBottomLimitTop = this._thumbBottomLimitBottom - this._thumbRectangle.Height;
                this._thumbTopLimit = this.ClientRectangle.Y;
            }
            else
            {
                this._thumbHeight = this.Height > 0 ? this.Height : 10;
                this._thumbWidth = this.GetThumbSize();

                this._clickedBarRectangle = this.ClientRectangle;
                this._clickedBarRectangle.Inflate(-1, -1);

                this._thumbRectangle = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this._thumbWidth, this._thumbHeight);

                this._thumbPosition = this._thumbRectangle.Width / 2;
                this._thumbBottomLimitBottom = this.ClientRectangle.Right;
                this._thumbBottomLimitTop = this._thumbBottomLimitBottom - this._thumbRectangle.Width;
                this._thumbTopLimit = this.ClientRectangle.X;
            }

            this.ChangeThumbPosition(this.GetThumbPosition());

            this.Refresh();
        }

        /// <summary>
        /// Resets the scroll status.
        /// </summary>
        private void ResetScrollStatus()
        {
            this._bottomBarClicked = this._topBarClicked = false;

            this.StopTimer();
            this.Refresh();
        }

        /// <summary>
        /// Progresses the timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ProgressTimerTick(object sender, EventArgs e)
        {
            this.ProgressThumb(true);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="isSmallChange">true to use small change; false to use large change.</param>
        /// <param name="isUp">true to up; false to down.</param>
        /// <returns>New value.</returns>
        private int GetValue(bool isSmallChange, bool isUp)
        {
            int newValue;

            if (isUp)
            {
                newValue = this._currentValue - (isSmallChange ? this._smallChange : this._largeChange);

                if (newValue < this._minimum)
                {
                    newValue = this._minimum;
                }
            }
            else
            {
                newValue = this._currentValue + (isSmallChange ? this._smallChange : this._largeChange);

                if (newValue > this._maximum)
                {
                    newValue = this._maximum;
                }
            }

            return newValue;
        }

        /// <summary>
        /// Gets the thumb position.
        /// </summary>
        /// <returns>The thumb position.</returns>
        private int GetThumbPosition()
        {
            int pixelRange;

            if (this._thumbHeight == 0 || this._thumbWidth == 0)
            {
                return 0;
            }

            int thumbSize = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? (this._thumbPosition / this.Height) / this._thumbHeight : (this._thumbPosition / this.Width) / this._thumbWidth;

            if (this.Orientation == ModernScrollBarOrientation.Vertical)
            {
                pixelRange = this.Height - thumbSize;
            }
            else
            {
                pixelRange = this.Width - thumbSize;
            }

            int realRange = this._maximum - this._minimum;
            float perc = 0f;

            if (realRange != 0)
            {
                perc = (this._currentValue - (float)this._minimum) / realRange;
            }

            return Math.Max(this._thumbTopLimit, Math.Min(this._thumbBottomLimitTop, Convert.ToInt32(perc * pixelRange)));
        }

        /// <summary>
        /// Gets the size of the thumb.
        /// </summary>
        /// <returns>The size of the thumb.</returns>
        private int GetThumbSize()
        {
            int trackSize = this._modernScrollBarOrientation == ModernScrollBarOrientation.Vertical ? this.Height : this.Width;

            if (this._maximum == 0 || this._largeChange == 0)
            {
                return trackSize;
            }

            float newThumbSize = (this._largeChange * (float)trackSize) / this._maximum;

            return Convert.ToInt32(Math.Min(trackSize, Math.Max(newThumbSize, 10f)));
        }

        /// <summary>
        /// Enables the timer.
        /// </summary>
        private void EnableTimer()
        {
            if (!this._progressTimer.Enabled)
            {
                this._progressTimer.Interval = 600;
                this._progressTimer.Start();
            }
            else
            {
                this._progressTimer.Interval = 10;
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void StopTimer()
        {
            this._progressTimer.Stop();
        }

        /// <summary>
        /// Changes the thumb position.
        /// </summary>
        /// <param name="position">The position.</param>
        private void ChangeThumbPosition(int position)
        {
            if (this.Orientation == ModernScrollBarOrientation.Vertical)
            {
                this._thumbRectangle.Y = position;
            }
            else
            {
                this._thumbRectangle.X = position;
            }
        }

        /// <summary>
        /// Progresses the thumb.
        /// </summary>
        /// <param name="enableTimer">true to enable timer; false to disable timer.</param>
        private void ProgressThumb(bool enableTimer)
        {
            int scrollOldValue = this._currentValue;
            var type = ScrollEventType.First;
            int thumbSize;
            int thumbPos;

            if (this.Orientation == ModernScrollBarOrientation.Vertical)
            {
                thumbPos = this._thumbRectangle.Y;
                thumbSize = this._thumbRectangle.Height;
            }
            else
            {
                thumbPos = this._thumbRectangle.X;
                thumbSize = this._thumbRectangle.Width;
            }

            if (this._bottomBarClicked && thumbPos + thumbSize < this._trackPosition)
            {
                type = ScrollEventType.LargeIncrement;

                this._currentValue = this.GetValue(false, false);

                if (this._currentValue == this._maximum)
                {
                    this.ChangeThumbPosition(this._thumbBottomLimitTop);

                    type = ScrollEventType.Last;
                }
                else
                {
                    this.ChangeThumbPosition(Math.Min(this._thumbBottomLimitTop, this.GetThumbPosition()));
                }
            }
            else if (this._topBarClicked && thumbPos > this._trackPosition)
            {
                type = ScrollEventType.LargeDecrement;

                this._currentValue = this.GetValue(false, true);

                if (this._currentValue == this._minimum)
                {
                    this.ChangeThumbPosition(this._thumbTopLimit);

                    type = ScrollEventType.First;
                }
                else
                {
                    this.ChangeThumbPosition(Math.Max(this._thumbTopLimit, this.GetThumbPosition()));
                }
            }

            if (scrollOldValue != this._currentValue)
            {
                this.OnScroll(type, scrollOldValue, this._currentValue, this._scrollOrientation);

                this.Invalidate();

                if (enableTimer)
                {
                    this.EnableTimer();
                }
            }
        }
    }
}