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
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernTaskWindow class.
    /// </summary>
    [ToolboxBitmap(typeof(Form))]
    public class ModernTaskWindow : ModernForm
    {
        /// <summary>
        /// Field _controlContainer.
        /// </summary>
        protected readonly ModernPanel _controlContainer;

        /// <summary>
        /// Field SingletonWindow.
        /// </summary>
        private static ModernTaskWindow SingletonWindow;

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
        /// Field _timer.
        /// </summary>
        private Timer _timer;

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
            this.Size = new Size(400, 200);
            this.ShowStatusStrip = false;
            this.ShowBorder = false;
            base.TopBarHeight = 0;
            this.TopBarHeight = 2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        public ModernTaskWindow(long autoCloseTime)
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
                this._controlContainer.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernTaskWindow"/> class.
        /// </summary>
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        /// <param name="userControl">The user control.</param>
        public ModernTaskWindow(long autoCloseTime, Control userControl)
            : this()
        {
            if (userControl != null)
            {
                this._controlContainer.Controls.Add(userControl);
                userControl.Dock = DockStyle.Fill;
            }

            this._closeTime = autoCloseTime;

            if (this._closeTime > 0d)
            {
                if (this._timer != null)
                {
                    this._timer.Stop();
                    this._timer.Dispose();
                    this._timer = null;
                }

                this._timer = new Timer();
                this._timer.Interval = 4;
                this._timer.Tick += (s, e) => this.UpdateProgress();
                this._timer.Start();

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
        public new uint TopBarHeight
        {
            get;
            set;
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
        /// Gets or sets a value indicating whether finish timer countdown immediately.
        /// </summary>
        public bool EndTimer
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
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        public static void Show(string text, long autoCloseTime)
        {
            Show(null, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(string text, long autoCloseTime, Control userControl)
        {
            Show(null, text, autoCloseTime, userControl);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        public static void Show(IWin32Window owner, string text, long autoCloseTime)
        {
            Show(owner, text, autoCloseTime, null);
        }

        /// <summary>
        /// Shows the task window.
        /// </summary>
        /// <param name="owner">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="autoCloseTime">The auto close window duration in milliseconds.</param>
        /// <param name="userControl">The user control.</param>
        public static void Show(IWin32Window owner, string text, long autoCloseTime, Control userControl)
        {
            if (SingletonWindow != null)
            {
                SingletonWindow.Close();
                SingletonWindow.Dispose();
                SingletonWindow = null;
            }

            SingletonWindow = new ModernTaskWindow(autoCloseTime, userControl);
            SingletonWindow.Text = text;
            SingletonWindow.Resizable = false;
            SingletonWindow.Movable = true;
            SingletonWindow.StartPosition = FormStartPosition.Manual;
            SingletonWindow.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 400 - 5, Screen.PrimaryScreen.Bounds.Height - 200 - 5);

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
                SingletonWindow.ThemeStyle = ownerForm.ThemeStyle;
                SingletonWindow.ColorStyle = ownerForm.ColorStyle;
                SingletonWindow.StyleManager = ownerForm.StyleManager.Clone(SingletonWindow) as ModernStyleManager;
            }

            SingletonWindow.Show();
        }

        /// <summary>
        /// Determines whether this instance is visible.
        /// </summary>
        /// <returns>true if visible; otherwise, false.</returns>
        public static bool IsVisible()
        {
            return SingletonWindow != null && SingletonWindow.Visible;
        }

        /// <summary>
        /// Cancels the automatic close.
        /// </summary>
        public static void CancelAutoClose()
        {
            if (SingletonWindow != null)
            {
                SingletonWindow.CancelTimer = true;
            }
        }

        /// <summary>
        /// Forces the close.
        /// </summary>
        public static void ForceClose()
        {
            if (SingletonWindow != null)
            {
                CancelAutoClose();
                SingletonWindow.Close();
                SingletonWindow.Dispose();
                SingletonWindow = null;
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

            using (SolidBrush brush = ModernPaint.GetStyleBrush(this.ColorStyle))
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this._progressWidth, (int)this.TopBarHeight));
            }
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        private void UpdateProgress()
        {
            if (this.EndTimer)
            {
                this.CloseNow();
                return;
            }

            if (this._elapsedTime >= this._closeTime || this.IsDisposed)
            {
                this.CloseNow();
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
            this.Invalidate(new Rectangle(0, 0, this.Width, (int)this.TopBarHeight));

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
                this._timer.Stop();
                this._timer.Start();
            }
        }

        /// <summary>
        /// Close current instance immediately.
        /// </summary>
        private void CloseNow()
        {
            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Dispose();
                this._timer = null;
            }

            this.Close();
        }
    }
}
