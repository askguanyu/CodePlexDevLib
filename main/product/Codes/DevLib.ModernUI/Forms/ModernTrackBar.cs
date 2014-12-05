//-----------------------------------------------------------------------
// <copyright file="ModernTrackBar.cs" company="YuGuan Corporation">
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
    /// ModernTrackBar user control.
    /// </summary>
    [ToolboxBitmap(typeof(TrackBar))]
    [DefaultEvent("Scroll")]
    public class ModernTrackBar : Control, IModernControl
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
        /// Field _trackerValue.
        /// </summary>
        private int _trackerValue = 50;

        /// <summary>
        /// Field _minimum.
        /// </summary>
        private int _minimum = 0;

        /// <summary>
        /// Field _maximum.
        /// </summary>
        private int _maximum = 100;

        /// <summary>
        /// Field _mouseWheelBarPartitions.
        /// </summary>
        private int _mouseWheelBarPartitions = 10;

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
        /// Initializes a new instance of the <see cref="ModernTrackBar"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="value">The value of position.</param>
        public ModernTrackBar(int min, int max, int value)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserMouse | ControlStyles.UserPaint, true);

            this.BackColor = Color.Transparent;

            this.Minimum = min;
            this.Maximum = max;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTrackBar"/> class.
        /// </summary>
        public ModernTrackBar()
            : this(0, 100, 50)
        {
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
        /// Event ValueChanged.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Event Scroll.
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
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool DisplayFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of position.
        /// </summary>
        [DefaultValue(50)]
        public int Value
        {
            get
            {
                return this._trackerValue;
            }

            set
            {
                if (value >= this._minimum & value <= this._maximum)
                {
                    this._trackerValue = value;
                    this.OnValueChanged();
                    this.Invalidate();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Value", "Value is outside appropriate range (min, max)");
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum of value.
        /// </summary>
        [DefaultValue(0)]
        public int Minimum
        {
            get
            {
                return this._minimum;
            }

            set
            {
                if (value < this._maximum)
                {
                    this._minimum = value;

                    if (this._trackerValue < this._minimum)
                    {
                        this._trackerValue = this._minimum;

                        if (this.ValueChanged != null)
                        {
                            this.ValueChanged(this, EventArgs.Empty);
                        }
                    }

                    this.Invalidate();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Minimum", "Minimal value is greather than maximal one");
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        [DefaultValue(100)]
        public int Maximum
        {
            get
            {
                return this._maximum;
            }

            set
            {
                if (value > this._minimum)
                {
                    this._maximum = value;

                    if (this._trackerValue > this._maximum)
                    {
                        this._trackerValue = this._maximum;

                        if (this.ValueChanged != null)
                        {
                            this.ValueChanged(this, EventArgs.Empty);
                        }
                    }

                    this.Invalidate();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Maximum", "Maximal value is lower than minimal one");
                }
            }
        }

        /// <summary>
        /// Gets or sets the small change.
        /// </summary>
        [DefaultValue(1)]
        public int SmallChange
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the large change.
        /// </summary>
        [DefaultValue(5)]
        public int LargeChange
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mouse wheel bar partitions.
        /// </summary>
        [DefaultValue(10)]
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
            Color thumbColor;
            Color barColor;

            if (this._isHovered && !this._isPressed && this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.TrackBar.Thumb.Hover(this.ThemeStyle);
                barColor = ModernPaint.BackColor.TrackBar.Bar.Hover(this.ThemeStyle);
            }
            else if (this._isHovered && this._isPressed && this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.TrackBar.Thumb.Press(this.ThemeStyle);
                barColor = ModernPaint.BackColor.TrackBar.Bar.Press(this.ThemeStyle);
            }
            else if (!this.Enabled)
            {
                thumbColor = ModernPaint.BackColor.TrackBar.Thumb.Disabled(this.ThemeStyle);
                barColor = ModernPaint.BackColor.TrackBar.Bar.Disabled(this.ThemeStyle);
            }
            else
            {
                thumbColor = ModernPaint.BackColor.TrackBar.Thumb.Normal(this.ThemeStyle);
                barColor = ModernPaint.BackColor.TrackBar.Bar.Normal(this.ThemeStyle);
            }

            this.DrawTrackBar(e.Graphics, thumbColor, barColor);

            if (this.DisplayFocus && this._isFocused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, this.ClientRectangle);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            this._isFocused = true;
            this.Invalidate();

            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            this._isFocused = false;
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
            this._isFocused = true;
            this.Invalidate();

            base.OnEnter(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            this._isFocused = false;
            this._isHovered = false;
            this._isPressed = false;
            this.Invalidate();

            base.OnLeave(e);
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

            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Left:
                    this.SetProperValue(this.Value - this.SmallChange);
                    this.OnScroll(ScrollEventType.SmallDecrement, this.Value);
                    break;

                case Keys.Up:
                case Keys.Right:
                    this.SetProperValue(this.Value + this.SmallChange);
                    this.OnScroll(ScrollEventType.SmallIncrement, this.Value);
                    break;

                case Keys.Home:
                    this.Value = this._minimum;
                    break;

                case Keys.End:
                    this.Value = this._maximum;
                    break;

                case Keys.PageDown:
                    this.SetProperValue(this.Value - this.LargeChange);
                    this.OnScroll(ScrollEventType.LargeDecrement, this.Value);
                    break;

                case Keys.PageUp:
                    this.SetProperValue(this.Value + this.LargeChange);
                    this.OnScroll(ScrollEventType.LargeIncrement, this.Value);
                    break;
            }

            if (this.Value == this._minimum)
            {
                this.OnScroll(ScrollEventType.First, this.Value);
            }

            if (this.Value == this._maximum)
            {
                this.OnScroll(ScrollEventType.Last, this.Value);
            }

            Point pt = this.PointToClient(Cursor.Position);
            this.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0));
        }

        /// <summary>
        /// Processes a dialog key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns>true if the key was processed by the control; otherwise, false.</returns>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab | Control.ModifierKeys == Keys.Shift)
            {
                return base.ProcessDialogKey(keyData);
            }
            else
            {
                this.OnKeyDown(new KeyEventArgs(keyData));
                return true;
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

            if (e.Button == MouseButtons.Left)
            {
                this.Capture = true;
                this.OnScroll(ScrollEventType.ThumbTrack, this._trackerValue);
                this.OnValueChanged();
                this.OnMouseMove(e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.Capture & e.Button == MouseButtons.Left)
            {
                ScrollEventType set = ScrollEventType.ThumbPosition;
                Point pt = e.Location;
                int p = pt.X;

                float coef = (float)(this._maximum - this._minimum) / (float)(this.ClientSize.Width - 3);
                this._trackerValue = (int)((p * coef) + this._minimum);

                if (this._trackerValue <= this._minimum)
                {
                    this._trackerValue = this._minimum;
                    set = ScrollEventType.First;
                }
                else if (this._trackerValue >= this._maximum)
                {
                    this._trackerValue = this._maximum;
                    set = ScrollEventType.Last;
                }

                this.OnScroll(set, this._trackerValue);
                this.OnValueChanged();

                this.Invalidate();
            }
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            this._isHovered = false;
            this.Invalidate();

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int v = e.Delta / 120 * (this._maximum - this._minimum) / this._mouseWheelBarPartitions;
            this.SetProperValue(this.Value + v);
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
        /// Called when [value changed].
        /// </summary>
        private void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [scroll].
        /// </summary>
        /// <param name="scrollType">Type of the scroll.</param>
        /// <param name="newValue">The new value.</param>
        private void OnScroll(ScrollEventType scrollType, int newValue)
        {
            if (this.Scroll != null)
            {
                this.Scroll(this, new ScrollEventArgs(scrollType, newValue));
            }
        }

        /// <summary>
        /// Draws the track bar.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="thumbColor">Color of the thumb.</param>
        /// <param name="barColor">Color of the bar.</param>
        private void DrawTrackBar(Graphics g, Color thumbColor, Color barColor)
        {
            int trackX = ((this._trackerValue - this._minimum) * (this.Width - 6)) / (this._maximum - this._minimum);

            using (SolidBrush brush = new SolidBrush(thumbColor))
            {
                Rectangle barRectangle = new Rectangle(0, (this.Height / 2) - 2, trackX, 4);
                g.FillRectangle(brush, barRectangle);

                Rectangle thumbRect = new Rectangle(trackX, (this.Height / 2) - 8, 6, 16);
                g.FillRectangle(brush, thumbRect);
            }

            using (SolidBrush brush = new SolidBrush(barColor))
            {
                Rectangle barRectangle = new Rectangle(trackX + 7, (this.Height / 2) - 2, this.Width - trackX + 7, 4);
                g.FillRectangle(brush, barRectangle);
            }
        }

        /// <summary>
        /// Sets the proper value.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetProperValue(int value)
        {
            if (value < this._minimum)
            {
                this.Value = this._minimum;
            }
            else if (value > this._maximum)
            {
                this.Value = this._maximum;
            }
            else
            {
                this.Value = value;
            }
        }
    }
}