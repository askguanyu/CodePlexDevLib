//-----------------------------------------------------------------------
// <copyright file="ModernProgressBar.cs" company="YuGuan Corporation">
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
    /// ModernProgressBar user control.
    /// </summary>
    [ToolboxBitmap(typeof(ProgressBar))]
    public class ModernProgressBar : ProgressBar, IModernControl
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
        /// Field _marqueeX.
        /// </summary>
        private int _marqueeX = 0;

        /// <summary>
        /// Field _marqueeTimer.
        /// </summary>
        private Timer _marqueeTimer;

        /// <summary>
        /// Field _value.
        /// </summary>
        private long _value = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernProgressBar" /> class.
        /// </summary>
        public ModernProgressBar()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            this.ProgressBarStyle = ProgressBarStyle.Continuous;
            this.FontSize = ModernFontSize.Medium;
            this.FontWeight = ModernFontWeight.Regular;
            this.ShowPercentText = false;
            this.PercentTextAlign = ContentAlignment.MiddleCenter;
            this.ShowText = false;
            this.TextAlign = ContentAlignment.MiddleRight;
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
        [Browsable(false)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        /// Gets or sets text font size.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontSize.Medium)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontSize FontSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets text font weight.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Regular)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontWeight FontWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether show progress percent text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowPercentText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alignment of the percent text on the control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ContentAlignment PercentTextAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ProgressBar style.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ProgressBarStyle.Continuous)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ProgressBarStyle ProgressBarStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether show text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets progress text.
        /// </summary>
        [Browsable(true)]
        [DefaultValue("")]
        [Category(ModernConstants.PropertyCategoryName)]
        public new string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alignment of the text on the control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleRight)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ContentAlignment TextAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current position of the progress bar.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(0)]
        public new long Value
        {
            get
            {
                return this._value;
            }

            set
            {
                this._value = value > this.Maximum ? this.Maximum : value;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the range of the control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(100)]
        public new long Maximum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value of the range of the control.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(0)]
        public new long Minimum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets progress total percent.
        /// </summary>
        [Browsable(false)]
        public double ProgressTotalPercent
        {
            get
            {
                return (1d - ((double)(this.Maximum - this.Value) / this.Maximum)) * 100;
            }
        }

        /// <summary>
        /// Gets progress total value.
        /// </summary>
        [Browsable(false)]
        public double ProgressTotalValue
        {
            get
            {
                return 1d - ((double)(this.Maximum - this.Value) / this.Maximum);
            }
        }

        /// <summary>
        /// Gets progress percent text.
        /// </summary>
        [Browsable(false)]
        public string ProgressPercentText
        {
            get
            {
                return string.Format("{0}%", Math.Round(this.ProgressTotalPercent));
            }
        }

        /// <summary>
        /// Gets ProgressBar width.
        /// </summary>
        private double ProgressBarWidth
        {
            get
            {
                return ((double)this.Value / this.Maximum) * this.ClientRectangle.Width;
            }
        }

        /// <summary>
        /// Gets ProgressBar marquee width.
        /// </summary>
        private int ProgressBarMarqueeWidth
        {
            get
            {
                return this.ClientRectangle.Width / 3;
            }
        }

        /// <summary>
        /// Gets a value indicating whether marqueeTimer enabled.
        /// </summary>
        private bool MarqueeTimerEnabled
        {
            get
            {
                return this._marqueeTimer != null && this._marqueeTimer.Enabled;
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

            using (Graphics g = this.CreateGraphics())
            {
                proposedSize = new Size(int.MaxValue, int.MaxValue);
                preferredSize = TextRenderer.MeasureText(g, this.ProgressPercentText, ModernFonts.ProgressBar(this.FontSize, this.FontWeight), proposedSize, ModernPaint.GetTextFormatFlags(this.PercentTextAlign));
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
                    if (!this.Enabled)
                    {
                        backColor = ModernPaint.BackColor.ProgressBar.Bar.Disabled(this.ThemeStyle);
                    }
                    else
                    {
                        backColor = ModernPaint.BackColor.ProgressBar.Bar.Normal(this.ThemeStyle);
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
            if (this.ProgressBarStyle == ProgressBarStyle.Continuous)
            {
                if (!this.DesignMode)
                {
                    this.StopTimer();
                }

                this.DrawProgressContinuous(e.Graphics);
            }
            else if (this.ProgressBarStyle == ProgressBarStyle.Blocks)
            {
                if (!this.DesignMode)
                {
                    this.StopTimer();
                }

                this.DrawProgressContinuous(e.Graphics);
            }
            else if (this.ProgressBarStyle == ProgressBarStyle.Marquee)
            {
                if (!this.DesignMode && this.Enabled)
                {
                    this.StartTimer();
                }

                if (!this.Enabled)
                {
                    this.StopTimer();
                }

                if (this.Value == this.Maximum)
                {
                    this.StopTimer();
                    this.DrawProgressContinuous(e.Graphics);
                }
                else
                {
                    this.DrawProgressMarquee(e.Graphics);
                }
            }

            this.DrawProgressText(e.Graphics);

            using (Pen pen = new Pen(ModernPaint.BorderColor.ProgressBar.Normal(this.ThemeStyle)))
            {
                Rectangle borderRectangle = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRectangle);
            }

            this.OnCustomPaintForeground(new ModernPaintEventArgs(Color.Empty, Color.Empty, e.Graphics));
        }

        /// <summary>
        /// DrawProgressContinuous method.
        /// </summary>
        /// <param name="graphics">Graphics instance.</param>
        private void DrawProgressContinuous(Graphics graphics)
        {
            graphics.FillRectangle(ModernPaint.GetStyleBrush(this.ColorStyle), 0, 0, (int)this.ProgressBarWidth, this.ClientRectangle.Height);
        }

        /// <summary>
        /// DrawProgressMarquee method.
        /// </summary>
        /// <param name="graphics">Graphics instance.</param>
        private void DrawProgressMarquee(Graphics graphics)
        {
            graphics.FillRectangle(ModernPaint.GetStyleBrush(this.ColorStyle), this._marqueeX, 0, this.ProgressBarMarqueeWidth, this.ClientRectangle.Height);
        }

        /// <summary>
        /// DrawProgressText method.
        /// </summary>
        /// <param name="graphics">Graphics instance.</param>
        private void DrawProgressText(Graphics graphics)
        {
            if (!this.ShowText && !this.ShowPercentText)
            {
                return;
            }

            Color foreColor = this.Enabled ? ModernPaint.ForeColor.ProgressBar.Normal(this.ThemeStyle) : ModernPaint.ForeColor.ProgressBar.Disabled(this.ThemeStyle);

            if (this.ShowText)
            {
                TextRenderer.DrawText(graphics, this.Text, ModernFonts.ProgressBar(this.FontSize, this.FontWeight), this.ClientRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.TextAlign));
            }

            if (this.ShowPercentText)
            {
                TextRenderer.DrawText(graphics, this.ProgressPercentText, ModernFonts.ProgressBar(this.FontSize, this.FontWeight), this.ClientRectangle, foreColor, ModernPaint.GetTextFormatFlags(this.PercentTextAlign));
            }
        }

        /// <summary>
        /// Start timer.
        /// </summary>
        private void StartTimer()
        {
            if (this.MarqueeTimerEnabled)
            {
                return;
            }

            if (this._marqueeTimer == null)
            {
                this._marqueeTimer = new Timer();
                this._marqueeTimer.Interval = 10;
                this._marqueeTimer.Tick += this.OnMarqueeTimerTick;
            }

            this._marqueeX = -this.ProgressBarMarqueeWidth;

            this._marqueeTimer.Stop();
            this._marqueeTimer.Start();

            this._marqueeTimer.Enabled = true;

            this.Invalidate();
        }

        /// <summary>
        /// Stop timer.
        /// </summary>
        private void StopTimer()
        {
            if (this._marqueeTimer != null)
            {
                this._marqueeTimer.Stop();

                this.Invalidate();
            }
        }

        /// <summary>
        /// OnMarqueeTimerTick method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        private void OnMarqueeTimerTick(object sender, EventArgs e)
        {
            this._marqueeX++;

            if (this._marqueeX > this.ClientRectangle.Width)
            {
                this._marqueeX = -this.ProgressBarMarqueeWidth;
            }

            this.Invalidate();
        }
    }
}
