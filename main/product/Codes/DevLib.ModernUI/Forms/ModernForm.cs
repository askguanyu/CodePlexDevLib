//-----------------------------------------------------------------------
// <copyright file="ModernForm.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using DevLib.ModernUI.ComponentModel;
    using DevLib.ModernUI.Drawing;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// Represents a modern style window or dialog box that makes up an application's user interface.
    /// </summary>
    public class ModernForm : Form, IModernForm, IDisposable
    {
        /// <summary>
        /// Field BorderWidth.
        /// </summary>
        private const int BorderWidth = 5;

        /// <summary>
        /// Field CS_DROPSHADOW.
        /// </summary>
        private const int CS_DROPSHADOW = 0x20000;

        /// <summary>
        /// Field WS_MINIMIZEBOX.
        /// </summary>
        private const int WS_MINIMIZEBOX = 0x20000;

        /// <summary>
        /// Field _modernColorStyle.
        /// </summary>
        private ModernColorStyle _modernColorStyle = ModernColorStyle.Blue;

        /// <summary>
        /// Field _modernThemeStyle.
        /// </summary>
        private ModernThemeStyle _modernThemeStyle = ModernThemeStyle.Light;

        /// <summary>
        /// Field _displayHeader.
        /// </summary>
        private bool _displayHeader = true;

        /// <summary>
        /// Field _shadowType.
        /// </summary>
        private ModernFormShadowType _shadowType = ModernFormShadowType.AeroShadow;

        /// <summary>
        /// Field _image.
        /// </summary>
        private Bitmap _image = null;

        /// <summary>
        /// Field _backImage.
        /// </summary>
        private Image _backImage;

        /// <summary>
        /// Field _backImagePadding.
        /// </summary>
        private Padding _backImagePadding;

        /// <summary>
        /// Field _backImageMaxSize.
        /// </summary>
        private int _backImageMaxSize;

        /// <summary>
        /// Field _backImageAlign.
        /// </summary>
        private ModernFormBackImageAlign _backImageAlign;

        /// <summary>
        /// Field _invertImage.
        /// </summary>
        private bool _invertImage;

        /// <summary>
        /// Field _buttonDictionary.
        /// </summary>
        private Dictionary<FormControlBox, ModernControlBox> _buttonDictionary;

        /// <summary>
        /// Field _shadowForm.
        /// </summary>
        private Form _shadowForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernForm"/> class.
        /// </summary>
        public ModernForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "ModernForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TransparencyKey = Color.Lavender;
            this.Movable = true;
            this.Resizable = true;
            this.TextAlign = ModernFormTextAlign.Left;
            this.BackImageAlign = ModernFormBackImageAlign.TopLeft;
            this.UseControlBox = true;
            this.UseMinimizeBox = true;
            this.UseMaximizeBox = true;
            this.UseCloseBox = true;
        }

        /// <summary>
        /// Gets or sets modern color style.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernColorStyle ColorStyle
        {
            get
            {
                if (this.StyleManager != null)
                {
                    return this.StyleManager.ColorStyle;
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
        public ModernThemeStyle ThemeStyle
        {
            get
            {
                if (this.StyleManager != null)
                {
                    return this.StyleManager.ThemeStyle;
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
        public ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFormTextAlign TextAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the back color.
        /// </summary>
        [Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return ModernPaint.BackColor.Form(this.ThemeStyle);
            }
        }

        /// <summary>
        /// Gets or sets the border style.
        /// </summary>
        [DefaultValue(ModernFormBorderStyle.None)]
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public ModernFormBorderStyle BorderStyle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ModernForm"/> is movable.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool Movable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a modern control box is displayed in the caption bar of the form.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseControlBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Minimize button is displayed in the caption bar of the form.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseMinimizeBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Maximize button is displayed in the caption bar of the form.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseMaximizeBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Close button is displayed in the caption bar of the form.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool UseCloseBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets padding within the control.
        /// </summary>
        public new Padding Padding
        {
            get
            {
                return base.Padding;
            }

            set
            {
                value.Top = Math.Max(value.Top, this.DisplayHeader ? 60 : 30);
                base.Padding = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether display header.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(true)]
        public bool DisplayHeader
        {
            get
            {
                return this._displayHeader;
            }

            set
            {
                if (value != this._displayHeader)
                {
                    Padding padding = base.Padding;
                    padding.Top += value ? 30 : -30;
                    base.Padding = padding;
                }

                this._displayHeader = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ModernForm"/> is resizable.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public bool Resizable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the shadow.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(ModernFormShadowType.AeroShadow)]
        public ModernFormShadowType ShadowType
        {
            get
            {
                return this.IsMdiChild ? ModernFormShadowType.None : this._shadowType;
            }

            set
            {
                this._shadowType = value;
            }
        }

        /// <summary>
        /// Gets or sets the border style of the form.
        /// </summary>
        [Browsable(false)]
        public new FormBorderStyle FormBorderStyle
        {
            get
            {
                return base.FormBorderStyle;
            }

            set
            {
                base.FormBorderStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the current multiple-document interface (MDI) parent form of this form.
        /// </summary>
        public new Form MdiParent
        {
            get
            {
                return base.MdiParent;
            }

            set
            {
                if (value != null)
                {
                    this.RemoveShadow();
                    this._shadowType = ModernFormShadowType.None;
                }

                base.MdiParent = value;
            }
        }

        /// <summary>
        /// Gets or sets the back image.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(null)]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public Image BackImage
        {
            get
            {
                return this._backImage;
            }

            set
            {
                this._backImage = value;

                if (value != null)
                {
                    this._image = this.ApplyInvertImage(new Bitmap(value));
                }

                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the back image padding.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public Padding BackImagePadding
        {
            get
            {
                return this._backImagePadding;
            }

            set
            {
                this._backImagePadding = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the back image.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        public int BackImageMaxSize
        {
            get
            {
                return this._backImageMaxSize;
            }

            set
            {
                this._backImageMaxSize = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the back image align.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(ModernFormBackImageAlign.TopLeft)]
        public ModernFormBackImageAlign BackImageAlign
        {
            get
            {
                return this._backImageAlign;
            }

            set
            {
                this._backImageAlign = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether invert image.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryAppearance)]
        [DefaultValue(true)]
        public bool InvertImage
        {
            get
            {
                return this._invertImage;
            }

            set
            {
                this._invertImage = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets the internal spacing, in pixels, of the contents of a control.
        /// </summary>
        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(20, this.DisplayHeader ? 60 : 20, 20, 20);
            }
        }

        /// <summary>
        /// Gets the create parameters.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;

                if (this.ShadowType == ModernFormShadowType.SystemShadow)
                {
                    cp.ClassStyle |= CS_DROPSHADOW;
                }

                return cp;
            }
        }

        /// <summary>
        /// Applies the invert image.
        /// </summary>
        /// <param name="sourceImage">The source bitmap image.</param>
        /// <returns>The inverted image.</returns>
        public Bitmap ApplyInvertImage(Bitmap sourceImage)
        {
            byte a;
            byte r;
            byte g;
            byte b;

            Color pixelColor;

            for (int y = 0; y < sourceImage.Height; y++)
            {
                for (int x = 0; x < sourceImage.Width; x++)
                {
                    pixelColor = sourceImage.GetPixel(x, y);
                    a = pixelColor.A;
                    r = (byte)(255 - pixelColor.R);
                    g = (byte)(255 - pixelColor.G);
                    b = (byte)(255 - pixelColor.B);

                    if (r <= 0)
                    {
                        r = 17;
                    }

                    if (g <= 0)
                    {
                        g = 17;
                    }

                    if (b <= 0)
                    {
                        b = 17;
                    }

                    sourceImage.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }

            return sourceImage;
        }

        /// <summary>
        /// Focuses current form.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        [SecuritySafeCritical]
        public bool FocusCurrentForm()
        {
            return WinApi.SetForegroundWindow(this.Handle);
        }

        /// <summary>
        /// Removes the close box.
        /// </summary>
        [SecuritySafeCritical]
        public void RemoveCloseBox()
        {
            IntPtr hMenu = WinApi.GetSystemMenu(this.Handle, false);

            if (hMenu == IntPtr.Zero)
            {
                return;
            }

            int n = WinApi.GetMenuItemCount(hMenu);

            if (n <= 0)
            {
                return;
            }

            WinApi.RemoveMenu(hMenu, (uint)(n - 1), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.RemoveMenu(hMenu, (uint)(n - 2), WinApi.MfByposition | WinApi.MfRemove);
            WinApi.DrawMenuBar(this.Handle);
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.Windows.Forms.Form" />.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.RemoveShadow();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Color backColor = ModernPaint.BackColor.Form(this.ThemeStyle);
            Color foreColor = ModernPaint.ForeColor.Title(this.ThemeStyle);

            e.Graphics.Clear(backColor);

            using (SolidBrush brush = ModernPaint.GetStyleBrush(this.ColorStyle))
            {
                Rectangle topRectangle = new Rectangle(0, 0, this.Width, BorderWidth);
                e.Graphics.FillRectangle(brush, topRectangle);
            }

            if (this.BorderStyle != ModernFormBorderStyle.None)
            {
                Color borderColor = ModernPaint.BorderColor.Form(this.ThemeStyle);

                using (Pen pen = new Pen(borderColor))
                {
                    e.Graphics.DrawLines(
                        pen,
                        new[]
                        {
                            new Point(0, BorderWidth),
                            new Point(0, this.Height - 1),
                            new Point(this.Width - 1, this.Height - 1),
                            new Point(this.Width - 1, BorderWidth)
                        });
                }
            }

            if (this.BackImage != null && this.BackImageMaxSize != 0)
            {
                Image image = ModernImage.ResizeImage(this.BackImage, new Rectangle(0, 0, this.BackImageMaxSize, this.BackImageMaxSize));

                if (this.InvertImage)
                {
                    image = ModernImage.ResizeImage(this.ThemeStyle == ModernThemeStyle.Dark ? this._image : this.BackImage, new Rectangle(0, 0, this.BackImageMaxSize, this.BackImageMaxSize));
                }

                switch (this.BackImageAlign)
                {
                    case ModernFormBackImageAlign.TopLeft:
                        e.Graphics.DrawImage(image, 0 + this.BackImagePadding.Left, 0 + this.BackImagePadding.Top);
                        break;

                    case ModernFormBackImageAlign.TopRight:
                        e.Graphics.DrawImage(image, ClientRectangle.Right - (this.BackImagePadding.Right + image.Width), 0 + this.BackImagePadding.Top);
                        break;

                    case ModernFormBackImageAlign.BottomLeft:
                        e.Graphics.DrawImage(image, 0 + this.BackImagePadding.Left, ClientRectangle.Bottom - (image.Height + this.BackImagePadding.Bottom));
                        break;

                    case ModernFormBackImageAlign.BottomRight:
                        e.Graphics.DrawImage(image, ClientRectangle.Right - (this.BackImagePadding.Right + image.Width), ClientRectangle.Bottom - (image.Height + this.BackImagePadding.Bottom));
                        break;
                }
            }

            if (this.DisplayHeader)
            {
                Rectangle bounds = new Rectangle(20, 20, this.ClientRectangle.Width - (2 * 20), 40);
                TextFormatFlags flags = TextFormatFlags.EndEllipsis | this.GetTextFormatFlags();
                TextRenderer.DrawText(e.Graphics, this.Text ?? string.Empty, ModernFonts.Title, bounds, foreColor, flags);
            }

            if (this.Resizable && (this.SizeGripStyle == SizeGripStyle.Auto || this.SizeGripStyle == SizeGripStyle.Show))
            {
                using (SolidBrush brush = new SolidBrush(ModernPaint.ForeColor.Button.Disabled(this.ThemeStyle)))
                {
                    Size resizeHandleSize = new Size(2, 2);
                    e.Graphics.FillRectangles(
                        brush,
                        new Rectangle[]
                        {
                            new Rectangle(new Point(this.ClientRectangle.Width - 6, this.ClientRectangle.Height - 6), resizeHandleSize),
                            new Rectangle(new Point(this.ClientRectangle.Width - 10, this.ClientRectangle.Height - 10), resizeHandleSize),
                            new Rectangle(new Point(this.ClientRectangle.Width - 10, this.ClientRectangle.Height - 6), resizeHandleSize),
                            new Rectangle(new Point(this.ClientRectangle.Width - 6, this.ClientRectangle.Height - 10), resizeHandleSize),
                            new Rectangle(new Point(this.ClientRectangle.Width - 14, this.ClientRectangle.Height - 6), resizeHandleSize),
                            new Rectangle(new Point(this.ClientRectangle.Width - 6, this.ClientRectangle.Height - 14), resizeHandleSize)
                        });
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!(this is ModernTaskWindow))
            {
                ModernTaskWindow.ForceClose();
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            this.RemoveShadow();

            base.OnClosed(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
            {
                return;
            }

            switch (this.StartPosition)
            {
                case FormStartPosition.CenterParent:
                    this.CenterToParent();
                    break;

                case FormStartPosition.CenterScreen:
                    if (this.IsMdiChild)
                    {
                        this.CenterToParent();
                    }
                    else
                    {
                        this.CenterToScreen();
                    }

                    break;
            }

            this.RemoveCloseBox();

            if (this.UseControlBox)
            {
                if (this.UseCloseBox)
                {
                    this.AddControlBox(FormControlBox.Close);
                }

                if (this.UseMaximizeBox)
                {
                    this.AddControlBox(FormControlBox.Maximize);
                }

                if (this.UseMinimizeBox)
                {
                    this.AddControlBox(FormControlBox.Minimize);
                }

                this.UpdateControlBoxPosition();
            }

            this.CreateShadow();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Activated" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this.ShadowType == ModernFormShadowType.AeroShadow && IsAeroThemeEnabled() && IsDropShadowSupported())
            {
                int val = 2;
                DwmApi.DwmSetWindowAttribute(this.Handle, 2, ref val, 4);

                var m = new DwmApi.MARGINS
                {
                    cyBottomHeight = 1,
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyTopHeight = 0
                };

                DwmApi.DwmExtendFrameIntoClientArea(this.Handle, ref m);
            }
        }

        /// <summary>
        /// Raises the EnabledChanged event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            this.Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            this.UpdateControlBoxPosition();
        }

        /// <summary>
        /// WndProc method.
        /// </summary>
        /// <param name="m">A Windows Message object.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (this.DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case (int)WinApi.Messages.WM_SYSCOMMAND:

                    int sc = m.WParam.ToInt32() & 0xFFF0;

                    switch (sc)
                    {
                        case (int)WinApi.Messages.SC_MOVE:

                            if (!this.Movable)
                            {
                                return;
                            }

                            break;

                        case (int)WinApi.Messages.SC_MAXIMIZE:
                            break;

                        case (int)WinApi.Messages.SC_RESTORE:
                            break;
                    }

                    break;

                case (int)WinApi.Messages.WM_NCLBUTTONDBLCLK:
                case (int)WinApi.Messages.WM_LBUTTONDBLCLK:

                    if (!this.MaximizeBox)
                    {
                        return;
                    }

                    break;

                case (int)WinApi.Messages.WM_NCHITTEST:

                    WinApi.HitTest ht = this.HitTestNCA(m.HWnd, m.WParam, m.LParam);

                    if (ht != WinApi.HitTest.HTCLIENT)
                    {
                        m.Result = (IntPtr)ht;
                        return;
                    }

                    break;

                case (int)WinApi.Messages.WM_DWMCOMPOSITIONCHANGED:
                    break;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && this.Movable)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    return;
                }

                if (this.Width - BorderWidth > e.Location.X && e.Location.X > BorderWidth && e.Location.Y > BorderWidth)
                {
                    this.MoveControl();
                }
            }
        }

        /// <summary>
        /// Determines whether is aero theme enabled.
        /// </summary>
        /// <returns>true if enabled; otherwise, false.</returns>
        [SecuritySafeCritical]
        private static bool IsAeroThemeEnabled()
        {
            if (Environment.OSVersion.Version.Major <= 5)
            {
                return false;
            }

            bool aeroEnabled;
            DwmApi.DwmIsCompositionEnabled(out aeroEnabled);

            return aeroEnabled;
        }

        /// <summary>
        /// Determines whether is drop shadow supported.
        /// </summary>
        /// <returns>true if supported; otherwise, false.</returns>
        private static bool IsDropShadowSupported()
        {
            return Environment.OSVersion.Version.Major > 5 && SystemInformation.IsDropShadowEnabled;
        }

        /// <summary>
        /// Gets the text format flags.
        /// </summary>
        /// <returns>TextFormatFlags instance.</returns>
        private TextFormatFlags GetTextFormatFlags()
        {
            switch (this.TextAlign)
            {
                case ModernFormTextAlign.Left:
                    return TextFormatFlags.Left;
                case ModernFormTextAlign.Center:
                    return TextFormatFlags.HorizontalCenter;
                case ModernFormTextAlign.Right:
                    return TextFormatFlags.Right;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// HitTestNCA method.
        /// </summary>
        /// <param name="hwnd">The hwnd IntPtr.</param>
        /// <param name="wparam">The wparam IntPtr.</param>
        /// <param name="lparam">The lparam IntPtr.</param>
        /// <returns>HitTest result.</returns>
        private WinApi.HitTest HitTestNCA(IntPtr hwnd, IntPtr wparam, IntPtr lparam)
        {
            Point vPoint = new Point((short)lparam, (short)((int)lparam >> 16));
            int vPadding = Math.Max(this.Padding.Right, this.Padding.Bottom);

            if (this.Resizable)
            {
                if (this.RectangleToScreen(new Rectangle(this.ClientRectangle.Width - vPadding, this.ClientRectangle.Height - vPadding, vPadding, vPadding)).Contains(vPoint))
                {
                    return WinApi.HitTest.HTBOTTOMRIGHT;
                }
            }

            if (this.RectangleToScreen(new Rectangle(BorderWidth, BorderWidth, this.ClientRectangle.Width - (2 * BorderWidth), 50)).Contains(vPoint))
            {
                return WinApi.HitTest.HTCAPTION;
            }

            return WinApi.HitTest.HTCLIENT;
        }

        /// <summary>
        /// Moves the control.
        /// </summary>
        [SecuritySafeCritical]
        private void MoveControl()
        {
            WinApi.ReleaseCapture();
            WinApi.SendMessage(this.Handle, (int)WinApi.Messages.WM_NCLBUTTONDOWN, (int)WinApi.HitTest.HTCAPTION, 0);
        }

        /// <summary>
        /// Adds the control box.
        /// </summary>
        /// <param name="controlBox">The control box.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private void AddControlBox(FormControlBox controlBox)
        {
            if (this._buttonDictionary == null)
            {
                this._buttonDictionary = new Dictionary<FormControlBox, ModernControlBox>();
            }

            if (this._buttonDictionary.ContainsKey(controlBox))
            {
                return;
            }

            ModernControlBox newButton = new ModernControlBox();

            if (controlBox == FormControlBox.Close)
            {
                newButton.Text = "r";
            }
            else if (controlBox == FormControlBox.Minimize)
            {
                newButton.Text = "0";
            }
            else if (controlBox == FormControlBox.Maximize)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    newButton.Text = "1";
                }
                else
                {
                    newButton.Text = "2";
                }
            }

            newButton.ColorStyle = this.ColorStyle;
            newButton.ThemeStyle = this.ThemeStyle;
            newButton.Tag = controlBox;
            newButton.Size = new Size(25, 20);
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.TabStop = false;
            newButton.Click += this.OnControlBoxClick;
            this.Controls.Add(newButton);

            this._buttonDictionary.Add(controlBox, newButton);
        }

        /// <summary>
        /// Handles the Click event of the control box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnControlBoxClick(object sender, EventArgs e)
        {
            var modernFormButton = sender as ModernControlBox;

            if (modernFormButton != null)
            {
                var controlBoxFlag = (FormControlBox)modernFormButton.Tag;

                if (controlBoxFlag == FormControlBox.Close)
                {
                    this.Close();
                }
                else if (controlBoxFlag == FormControlBox.Minimize)
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                else if (controlBoxFlag == FormControlBox.Maximize)
                {
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        this.WindowState = FormWindowState.Maximized;
                        modernFormButton.Text = "2";
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                        modernFormButton.Text = "1";
                    }
                }
            }
        }

        /// <summary>
        /// Updates the control box position.
        /// </summary>
        private void UpdateControlBoxPosition()
        {
            if (!this.UseControlBox)
            {
                return;
            }

            Dictionary<int, FormControlBox> priorityOrder = new Dictionary<int, FormControlBox>();

            if (this.UseCloseBox)
            {
                priorityOrder.Add(0, FormControlBox.Close);
            }

            if (this.UseMaximizeBox)
            {
                priorityOrder.Add(1, FormControlBox.Maximize);
            }

            if (this.UseMinimizeBox)
            {
                priorityOrder.Add(2, FormControlBox.Minimize);
            }

            Point firstControlBoxLocation = new Point(this.ClientRectangle.Width - BorderWidth - 25, BorderWidth);
            int lastDrawedControlBoxPosition = firstControlBoxLocation.X - 25;

            ModernControlBox firstButton = null;

            if (this._buttonDictionary.Count == 1)
            {
                foreach (KeyValuePair<FormControlBox, ModernControlBox> button in this._buttonDictionary)
                {
                    button.Value.Location = firstControlBoxLocation;
                }
            }
            else
            {
                foreach (KeyValuePair<int, FormControlBox> button in priorityOrder)
                {
                    bool buttonExists = this._buttonDictionary.ContainsKey(button.Value);

                    if (firstButton == null && buttonExists)
                    {
                        firstButton = this._buttonDictionary[button.Value];
                        firstButton.Location = firstControlBoxLocation;
                        continue;
                    }

                    if (firstButton == null || !buttonExists)
                    {
                        continue;
                    }

                    this._buttonDictionary[button.Value].Location = new Point(lastDrawedControlBoxPosition, BorderWidth);
                    lastDrawedControlBoxPosition = lastDrawedControlBoxPosition - 25;
                }
            }

            this.Refresh();
        }

        /// <summary>
        /// Creates the shadow.
        /// </summary>
        private void CreateShadow()
        {
            switch (this.ShadowType)
            {
                case ModernFormShadowType.Flat:
                    this._shadowForm = new ModernFlatDropShadow(this);
                    return;

                case ModernFormShadowType.DropShadow:
                    this._shadowForm = new ModernRealisticDropShadow(this);
                    return;
            }
        }

        /// <summary>
        /// Removes the shadow.
        /// </summary>
        private void RemoveShadow()
        {
            if (this._shadowForm == null || this._shadowForm.IsDisposed)
            {
                return;
            }

            this._shadowForm.Visible = false;
            this.Owner = this._shadowForm.Owner;
            this._shadowForm.Owner = null;
            this._shadowForm.Dispose();
            this._shadowForm = null;
        }

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="g">Graphics instance.</param>
        /// <param name="clientRectangle">The client rectangle.</param>
        /// <param name="font">The font.</param>
        /// <param name="text">The text.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>Rectangle instance.</returns>
        private Rectangle MeasureText(Graphics g, Rectangle clientRectangle, Font font, string text, TextFormatFlags flags)
        {
            var proposedSize = new Size(int.MaxValue, int.MinValue);
            var actualSize = TextRenderer.MeasureText(g, text, font, proposedSize, flags);

            return new Rectangle(clientRectangle.X, clientRectangle.Y, actualSize.Width, actualSize.Height);
        }
    }
}