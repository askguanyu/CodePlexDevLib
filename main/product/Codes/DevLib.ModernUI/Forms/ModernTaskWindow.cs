//-----------------------------------------------------------------------
// <copyright file="ModernTaskWindow.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using DevLib.ModernUI.Animation;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernTaskWindow class.
    /// </summary>
    public sealed class ModernTaskWindow : ModernForm
    {
        /// <summary>
        /// Field _singletonWindow.
        /// </summary>
        private static ModernTaskWindow _singletonWindow;

        /// <summary>
        /// Field _closeTime.
        /// </summary>
        private readonly double _closeTime;

        /// <summary>
        /// Field _controlContainer.
        /// </summary>
        private readonly ModernPanel _controlContainer;

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
            this._controlContainer = new ModernPanel();
            this.Controls.Add(this._controlContainer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="duration">The duration in seconds.</param>
        /// <param name="userControl">The user control.</param>
        public ModernTaskWindow(int duration, Control userControl)
            : this()
        {
            if (userControl != null)
            {
                this._controlContainer.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }

            this._closeTime = duration * 1000d;

            if (this._closeTime > 0d)
            {
                this._delayedCallTimer = DelayedCall.Start(this.UpdateProgress, 4);
                this._lastUpdateTime = DateTime.Now;
                this._thresholdTime = this._closeTime / 1.5d;
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
        /// <param name="autoCloseTime">The seconds to close.</param>
        public static void Show(string text, int autoCloseTime)
        {
            Show(null, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The seconds to close.</param>
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
        /// <param name="autoCloseTime">The seconds to close.</param>
        public static void Show(IWin32Window owner, string text, int autoCloseTime)
        {
            Show(owner, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The seconds to close.</param>
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

            _singletonWindow.Opacity = 0;
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
                this._controlContainer.ThemeStyle = this.ThemeStyle;
                this._controlContainer.ColorStyle = this.ColorStyle;
                this._controlContainer.StyleManager = this.StyleManager;

                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Movable = true;

                this.Size = new Size(400, 200);

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

                this._controlContainer.Location = new Point(0, 60);
                this._controlContainer.Size = new Size(this.Width - 40, this.Height - 80);
                this._controlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
                this._controlContainer.AutoScroll = false;
                this._controlContainer.ShowHorizontalScrollBar = false;
                this._controlContainer.ShowVerticalScrollBar = false;
                this._controlContainer.Refresh();

                if (this.StyleManager != null)
                {
                    this.StyleManager.Update();
                }

                this._isInitialized = true;

                MoveAnimation moveAnimation = new MoveAnimation();
                moveAnimation.Start(this._controlContainer, new Point(20, 60), TransitionType.EaseInOutCubic, 4);
            }

            base.OnActivated(e);

            this.Opacity = 1;
            this.TopMost = true;
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (SolidBrush brush = new SolidBrush(ModernPaint.BackColor.Form(this.ThemeStyle)))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(this.Width - this._progressWidth, 0, this._progressWidth, 5));
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
            this.Invalidate(new Rectangle(0, 0, this.Width, 5));

            if (this._elapsedTime >= this._thresholdTime && this.Opacity > 0.1d)
            {
                double opacity = ((1d - percent) * 3d) + 0.01;

                if (opacity <= 0.1d)
                {
                    opacity = 0.1d;
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