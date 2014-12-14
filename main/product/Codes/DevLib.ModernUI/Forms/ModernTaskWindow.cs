//-----------------------------------------------------------------------
// <copyright file="ModernTaskWindow.cs" company="YuGuan Corporation">
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
    using DevLib.ModernUI.Animation;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernTaskWindow class.
    /// </summary>
    public class ModernTaskWindow : ModernForm
    {
        /// <summary>
        /// Field _controlContainer.
        /// </summary>
        protected readonly ModernPanel ControlContainer;

        /// <summary>
        /// Field _singletonWindow.
        /// </summary>
        private static ModernTaskWindow _singletonWindow;

        /// <summary>
        /// Field _closeTime.
        /// </summary>
        private readonly double _closeTime;

        /// <summary>
        /// Field _elapsedTime.
        /// </summary>
        private double _elapsedTime = 0;

        /// <summary>
        /// Field _thresholdTime.
        /// </summary>
        private double _thresholdTime;

        /// <summary>
        /// Field _progressWidth.
        /// </summary>
        private int _progressWidth = 0;

        /// <summary>
        /// Field _delayedCallTimer.
        /// </summary>
        private DelayedCall _delayedCallTimer;

        /// <summary>
        /// Field _isInitialized.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Field _lastUpdateTime.
        /// </summary>
        private DateTime _lastUpdateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        public ModernTaskWindow()
        {
            this.ControlContainer = new ModernPanel();
            this.Controls.Add(this.ControlContainer);
            this.Size = new Size(400, 200);
            this.ShowStatusStrip = false;
            this.ShowBorder = false;
            this.TopBarHeight = 2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        public ModernTaskWindow(int autoCloseTime)
            : this(autoCloseTime, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="userControl">The user control.</param>
        public ModernTaskWindow(Control userControl)
            : this()
        {
            if (userControl != null)
            {
                this.ControlContainer.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        /// <param name="userControl">The user control.</param>
        public ModernTaskWindow(int autoCloseTime, Control userControl)
            : this()
        {
            if (userControl != null)
            {
                this.ControlContainer.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }

            this._closeTime = autoCloseTime * 1000d;

            if (this._closeTime > 0d)
            {
                this._delayedCallTimer = DelayedCall.Start(this.UpdateProgress, 4);
                this._lastUpdateTime = DateTime.Now;
                this._thresholdTime = this._closeTime / 1.5d;
            }
        }

        /// <summary>
        /// Gets or sets the height of the top bar.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(2)]
        [Category(ModernConstants.PropertyCategoryName)]
        public new int TopBarHeight
        {
            get
            {
                return base.TopBarHeight;
            }

            set
            {
                base.TopBarHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether cancel timer.
        /// </summary>
        public bool CancelTimer
        {
            get;
            set;
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Show(string text)
        {
            Show(null, text, 0, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(string text, Control userControl)
        {
            Show(null, text, 0, userControl);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        public static void Show(IWin32Window owner, string text)
        {
            Show(owner, text, 0, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(IWin32Window owner, string text, Control userControl)
        {
            Show(owner, text, 0, userControl);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        public static void Show(string text, int autoCloseTime)
        {
            Show(null, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(string text, int autoCloseTime, Control userControl)
        {
            Show(null, text, autoCloseTime, userControl);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        public static void Show(IWin32Window owner, string text, int autoCloseTime)
        {
            Show(owner, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in seconds.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(IWin32Window owner, string text, int autoCloseTime, Control userControl)
        {
            if (_singletonWindow != null)
            {
                _singletonWindow.Close();
                _singletonWindow.Dispose();
                _singletonWindow = null;
            }

            _singletonWindow = new ModernTaskWindow(autoCloseTime, userControl);
            _singletonWindow.Text = text;
            _singletonWindow.Resizable = false;
            _singletonWindow.Movable = true;
            _singletonWindow.StartPosition = FormStartPosition.Manual;
            _singletonWindow.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 400 - 5, Screen.PrimaryScreen.Bounds.Height - 200 - 5);

            IModernForm ownerForm = null;

            if (owner != null)
            {
                ownerForm = owner as IModernForm;
            }
            else
            {
                ownerForm = Form.ActiveForm as IModernForm;
            }

            if (ownerForm != null)
            {
                _singletonWindow.ThemeStyle = ownerForm.ThemeStyle;
                _singletonWindow.ColorStyle = ownerForm.ColorStyle;
                _singletonWindow.StyleManager = ownerForm.StyleManager.Clone(_singletonWindow) as ModernStyleManager;
            }

            _singletonWindow.Show();
        }

        /// <summary>
        /// Determines whether this instance is visible.
        /// </summary>
        /// <returns>true if visible; otherwise, false.</returns>
        public static bool IsVisible()
        {
            return _singletonWindow != null && _singletonWindow.Visible;
        }

        /// <summary>
        /// Cancels the automatic close.
        /// </summary>
        public static void CancelAutoClose()
        {
            if (_singletonWindow != null)
            {
                _singletonWindow.CancelTimer = true;
            }
        }

        /// <summary>
        /// Forces the close.
        /// </summary>
        public static void ForceClose()
        {
            if (_singletonWindow != null)
            {
                CancelAutoClose();
                _singletonWindow.Close();
                _singletonWindow.Dispose();
                _singletonWindow = null;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Activated" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            if (!this._isInitialized)
            {
                this.ControlContainer.ThemeStyle = this.ThemeStyle;
                this.ControlContainer.ColorStyle = this.ColorStyle;
                this.ControlContainer.StyleManager = this.StyleManager;

                Taskbar taskbar = new Taskbar();

                switch (taskbar.Position)
                {
                    case TaskbarPosition.Left:
                        this.Location = new Point(taskbar.Bounds.Width + 5, taskbar.Bounds.Height - this.Height - 5);
                        break;

                    case TaskbarPosition.Top:
                        this.Location = new Point(taskbar.Bounds.Width - this.Width - 5, taskbar.Bounds.Height + 5);
                        break;

                    case TaskbarPosition.Right:
                        this.Location = new Point(taskbar.Bounds.X - this.Width - 5, taskbar.Bounds.Height - this.Height - 5);
                        break;

                    case TaskbarPosition.Bottom:
                        this.Location = new Point(taskbar.Bounds.Width - this.Width - 5, taskbar.Bounds.Y - this.Height - 5);
                        break;

                    case TaskbarPosition.Unknown:
                    default:
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 5, Screen.PrimaryScreen.Bounds.Height - this.Height - 5);
                        break;
                }

                this.ControlContainer.Location = new Point(0, 60);
                this.ControlContainer.Size = new Size(this.Width - 40, this.Height - 80);
                this.ControlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
                this.ControlContainer.AutoScroll = false;
                this.ControlContainer.ShowHorizontalScrollBar = false;
                this.ControlContainer.ShowVerticalScrollBar = false;
                this.ControlContainer.Refresh();

                if (this.StyleManager != null)
                {
                    this.StyleManager.Update();
                }

                this._isInitialized = true;

                MoveAnimation moveAnimation = new MoveAnimation();
                moveAnimation.Start(this.ControlContainer, new Point(20, 60), TransitionType.EaseInOutCubic, 4);
            }

            base.OnActivated(e);

            this.TopMost = true;
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (SolidBrush brush = new SolidBrush(ModernPaint.GetStyleColor(this.ColorStyle)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this._progressWidth, this.TopBarHeight));
            }
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        private void UpdateProgress()
        {
            if (this._elapsedTime >= this._closeTime || this.IsDisposed)
            {
                this._delayedCallTimer.Dispose();
                this._delayedCallTimer = null;
                this.Close();
                return;
            }

            this._elapsedTime += (DateTime.Now - this._lastUpdateTime).TotalMilliseconds;
            this._lastUpdateTime = DateTime.Now;

            if (this.CancelTimer)
            {
                this._elapsedTime = 0;
                this.Opacity = 1;
            }

            double percent = this._elapsedTime / this._closeTime;
            this._progressWidth = (int)((double)this.Width * percent);
            this.Invalidate(new Rectangle(0, 0, this.Width, this.TopBarHeight));

            if (this._elapsedTime >= this._thresholdTime && this.Opacity > 0.05d)
            {
                double opacity = ((1d - percent) * 3d) + 0.01;

                if (opacity <= 0.05d)
                {
                    opacity = 0.05d;
                }

                this.Opacity = opacity;
            }

            if (!this.CancelTimer)
            {
                this._delayedCallTimer.Reset();
            }
        }
    }
}
