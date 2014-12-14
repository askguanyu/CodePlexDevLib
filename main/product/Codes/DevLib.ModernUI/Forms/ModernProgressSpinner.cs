//-----------------------------------------------------------------------
// <copyright file="ModernProgressSpinner.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;

    /// <summary>
    /// ModernProgressSpinner user control.
    /// </summary>
    [ToolboxBitmap(typeof(ProgressBar))]
    public class ModernProgressSpinner : Control, IModernControl
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
        /// Field _timer.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Field _progress.
        /// </summary>
        private int _progress;

        /// <summary>
        /// Field _angle.
        /// </summary>
        private float _angle = 270;

        /// <summary>
        /// Field _minimum.
        /// </summary>
        private int _minimum = 0;

        /// <summary>
        /// Field _maximum.
        /// </summary>
        private int _maximum = 100;

        /// <summary>
        /// Field _ensureVisible.
        /// </summary>
        private bool _ensureVisible = true;

        /// <summary>
        /// Field _speed.
        /// </summary>
        private float _speed = 1;

        /// <summary>
        /// Field _backwards.
        /// </summary>
        private bool _backwards;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernProgressSpinner"/> class.
        /// </summary>
        public ModernProgressSpinner()
        {
            this._timer = new Timer();
            this._timer.Interval = 20;
            this._timer.Tick += this.TimerTick;
            this._timer.Enabled = true;

            this.Width = 16;
            this.Height = 16;
            this._speed = 1;
            this.DoubleBuffered = true;
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
        /// Gets or sets a value indicating whether this <see cref="ModernProgressSpinner"/> is spinning.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool Spinning
        {
            get
            {
                return this._timer.Enabled;
            }

            set
            {
                this._timer.Enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of progress.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(15)]
        [Category(ModernConstants.PropertyCategoryName)]
        public int Value
        {
            get
            {
                return this._progress;
            }

            set
            {
                if (value != -1 && (value < this._minimum || value > this._maximum))
                {
                    throw new ArgumentOutOfRangeException("Progress value must be -1 or between Minimum and Maximum.", (Exception)null);
                }

                this._progress = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of progress.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(0)]
        [Category(ModernConstants.PropertyCategoryName)]
        public int Minimum
        {
            get
            {
                return this._minimum;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Minimum value must bigger than or equal to 0.", (Exception)null);
                }

                if (value >= this._maximum)
                {
                    throw new ArgumentOutOfRangeException("Minimum value must be less than Maximum.", (Exception)null);
                }

                this._minimum = value;

                if (this._progress != -1 && this._progress < this._minimum)
                {
                    this._progress = this._minimum;
                }

                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of progress.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(60)]
        [Category(ModernConstants.PropertyCategoryName)]
        public int Maximum
        {
            get
            {
                return this._maximum;
            }

            set
            {
                if (value <= this._minimum)
                {
                    throw new ArgumentOutOfRangeException("Maximum value must be bigger than Minimum.", (Exception)null);
                }

                this._maximum = value;

                if (this._progress > this._maximum)
                {
                    this._progress = this._maximum;
                }

                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ensure visible.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool EnsureVisible
        {
            get
            {
                return this._ensureVisible;
            }

            set
            {
                this._ensureVisible = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(1f)]
        [Category(ModernConstants.PropertyCategoryName)]
        [Description("Speed value must be bigger than 0 and less than or equal to 10.")]
        public float Speed
        {
            get
            {
                return this._speed;
            }

            set
            {
                if (value <= 0)
                {
                    this._speed = 1;
                }

                if (value > 10)
                {
                    this._speed = 10;
                }

                this._speed = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ModernProgressSpinner"/> is backwards.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool Backwards
        {
            get
            {
                return this._backwards;
            }

            set
            {
                this._backwards = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use custom background.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseCustomBackground
        {
            get;
            set;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this._progress = this._minimum;
            this._angle = 270;
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
                    if (this.Parent is ModernTile)
                    {
                        backColor = ModernPaint.GetStyleColor(this.ColorStyle);
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
            Color foreColor;

            if (this.UseCustomBackground)
            {
                foreColor = ModernPaint.GetStyleColor(this.ColorStyle);
            }
            else
            {
                if (this.Parent is ModernTile)
                {
                    foreColor = ModernPaint.ForeColor.Tile.Normal(this.ThemeStyle);
                }
                else
                {
                    foreColor = ModernPaint.GetStyleColor(this.ColorStyle);
                }
            }

            using (Pen forePen = new Pen(foreColor, (float)this.Width / 5))
            {
                int padding = (int)Math.Ceiling((float)this.Width / 10);

                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                if (this._progress != -1)
                {
                    float sweepAngle;
                    float progFrac = (float)(this._progress - this._minimum) / (float)(this._maximum - this._minimum);

                    if (this._ensureVisible)
                    {
                        sweepAngle = 30 + (300f * progFrac);
                    }
                    else
                    {
                        sweepAngle = 360f * progFrac;
                    }

                    if (this._backwards)
                    {
                        sweepAngle = -sweepAngle;
                    }

                    e.Graphics.DrawArc(forePen, padding, padding, this.Width - (2 * padding) - 1, this.Height - (2 * padding) - 1, this._angle, sweepAngle);
                }
                else
                {
                    int maxOffset = 180;

                    for (int offset = 0; offset <= maxOffset; offset += 15)
                    {
                        int alpha = 290 - (offset * 290 / maxOffset);

                        if (alpha > 255)
                        {
                            alpha = 255;
                        }

                        if (alpha < 0)
                        {
                            alpha = 0;
                        }

                        Color color = Color.FromArgb(alpha, forePen.Color);

                        using (Pen gradPen = new Pen(color, forePen.Width))
                        {
                            float startAngle = this._angle + ((offset - (this._ensureVisible ? 30 : 0)) * (this._backwards ? 1 : -1));
                            float sweepAngle = 15 * (this._backwards ? 1 : -1);
                            e.Graphics.DrawArc(gradPen, padding, padding, this.Width - (2 * padding) - 1, this.Height - (2 * padding) - 1, startAngle, sweepAngle);
                        }
                    }
                }
            }

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, foreColor, e.Graphics));
        }

        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            this._angle += 6f * this._speed * (this._backwards ? -1 : 1);
            this.Refresh();
        }
    }
}
