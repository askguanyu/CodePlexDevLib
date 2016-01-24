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
    [ToolboxBitmap(typeof(Form))]
    public class ModernForm : Form, IModernForm, IDisposable
    {
        /// <summary>
        /// Field StatusStrip.
        /// </summary>
        protected readonly Panel StatusStrip = new Panel() { Height = 20, BackColor = Color.Transparent, ForeColor = Color.WhiteSmoke, Visible = false };

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
        /// Field _modernFontWeight.
        /// </summary>
        private ModernFontWeight _modernFontWeight = ModernFontWeight.Light;

        /// <summary>
        /// Field _showHeader.
        /// </summary>
        private bool _showHeader = true;

        /// <summary>
        /// Field _shadowType.
        /// </summary>
        private ModernFormShadowType _shadowType = ModernFormShadowType.AeroShadow;

        /// <summary>
        /// Field _invertBackImage.
        /// </summary>
        private Image _invertBackImage = null;

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
        /// Field _backImageInvert.
        /// </summary>
        private bool _backImageInvert;

        /// <summary>
        /// Field _controlBoxDictionary.
        /// </summary>
        private Dictionary<FormControlBox, ModernControlBox> _controlBoxDictionary;

        /// <summary>
        /// Field _shadowForm.
        /// </summary>
        private ModernShadowBase _shadowForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernForm"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public ModernForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "ModernForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TransparencyKey = Color.Lavender;
            this.Movable = true;
            this.Resizable = true;
            this.ShowBorder = true;
            this.TextAlign = ModernFormTextAlign.Left;
            this.BackImageAlign = ModernFormBackImageAlign.TopLeft;
            this.UseControlBox = true;
            this.UseMinimizeBox = true;
            this.UseMaximizeBox = true;
            this.UseCloseBox = true;
            this.FontSize = 24f;
            this.Controls.Add(this.StatusStrip);
            this.ShowStatusStrip = false;
            this.ShowHeader = true;
            this.TopBarHeight = 4;
            this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
        }

        /// <summary>
        /// Event CloseClick.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler CloseClick;

        /// <summary>
        /// Event MinimizeClick.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler MinimizeClick;

        /// <summary>
        /// Event MaximizeClick.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler MaximizeClick;

        /// <summary>
        /// Event NormalClick.
        /// </summary>
        [Category(ModernConstants.PropertyCategoryName)]
        public event EventHandler NormalClick;

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
        [Browsable(true)]
        [DefaultValue(ModernThemeStyle.Default)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernStyleManager StyleManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>
        /// The size of the font.
        /// </value>
        [Browsable(true)]
        [DefaultValue(24f)]
        [Category(ModernConstants.PropertyCategoryName)]
        public float FontSize
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
        [Browsable(true)]
        [DefaultValue(ModernFontWeight.Light)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFontWeight FontWeight
        {
            get
            {
                return this._modernFontWeight;
            }

            set
            {
                this._modernFontWeight = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the text align.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFormTextAlign.Left)]
        [Category(ModernConstants.PropertyCategoryName)]
        public ModernFormTextAlign TextAlign
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ModernForm"/> is movable.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool Movable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a modern control box is displayed in the caption bar of the form.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseControlBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Minimize button is displayed in the caption bar of the form.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseMinimizeBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Maximize button is displayed in the caption bar of the form.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool UseMaximizeBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the modern Close button is displayed in the caption bar of the form.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
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
                value.Top = Math.Max(value.Top, this.ShowHeader ? 60 : 30);
                base.Padding = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether show header.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowHeader
        {
            get
            {
                return this._showHeader;
            }

            set
            {
                if (value != this._showHeader)
                {
                    Padding padding = base.Padding;
                    padding.Top += value ? 30 : -30;
                    base.Padding = padding;
                }

                this._showHeader = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ModernForm"/> is resizable.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool Resizable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the shadow.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ModernFormShadowType.AeroShadow)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Browsable(true)]
        [DefaultValue(null)]
        [Category(ModernConstants.PropertyCategoryName)]
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
                    this._invertBackImage = this.GetInvertImage(value);
                }

                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the back image padding.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Browsable(true)]
        [DefaultValue(0)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        [Browsable(true)]
        [DefaultValue(ModernFormBackImageAlign.TopRight)]
        [Category(ModernConstants.PropertyCategoryName)]
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
        /// Gets or sets a value indicating whether invert back image.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool BackImageInvert
        {
            get
            {
                return this._backImageInvert;
            }

            set
            {
                this._backImageInvert = value;
                this.Refresh();
            }
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
        /// Gets or sets a value indicating whether control box use custom BackColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ControlBoxUseCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether control box use custom ForeColor.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ControlBoxUseCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the custom back color of the control box.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public Color ControlBoxCustomBackColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the custom fore color of the control box.
        /// </summary>
        [Browsable(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public Color ControlBoxCustomForeColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether show status strip.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowStatusStrip
        {
            get
            {
                return this.StatusStrip.Visible;
            }

            set
            {
                this.StatusStrip.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether show border.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(true)]
        [Category(ModernConstants.PropertyCategoryName)]
        public bool ShowBorder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the top bar.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(0)]
        [Category(ModernConstants.PropertyCategoryName)]
        public uint TopBarHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the internal spacing, in pixels, of the contents of a control.
        /// </summary>
        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(20, this.ShowHeader ? 60 : 20, 20, 20);
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
        /// Gets the invert image.
        /// </summary>
        /// <param name="sourceImage">The source bitmap image.</param>
        /// <returns>The inverted image.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public virtual Image GetInvertImage(Image sourceImage)
        {
            Bitmap result = new Bitmap(sourceImage);

            byte a;
            byte r;
            byte g;
            byte b;

            Color pixelColor;

            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    pixelColor = result.GetPixel(x, y);

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

                    result.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }

            return result;
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

                this.StatusStrip.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Color backColor = this.UseCustomBackColor ? this.BackColor : ModernPaint.BackColor.Form(this.ThemeStyle);
            Color foreColor = this.UseCustomForeColor ? this.ForeColor : ModernPaint.ForeColor.Title(this.ThemeStyle);

            e.Graphics.Clear(backColor);

            if (this.ShowHeader)
            {
                Rectangle bounds = new Rectangle(20, 20, this.ClientRectangle.Width - (2 * 20), 40);
                TextFormatFlags flags = TextFormatFlags.EndEllipsis | this.GetTextFormatFlags();
                TextRenderer.DrawText(e.Graphics, this.Text, ModernFonts.GetDefaultFont(this.FontSize, this.FontWeight), bounds, foreColor, flags);
            }

            if (this.TopBarHeight > 0)
            {
                using (SolidBrush brush = ModernPaint.GetStyleBrush(this.ColorStyle))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this.Width, (int)this.TopBarHeight + (this.ShowBorder ? 1 : 0)));
                }
            }

            if (this.BackImage != null && this.BackImageMaxSize != 0)
            {
                Image image = ModernImage.ResizeImage(this.BackImage, new Rectangle(0, 0, this.BackImageMaxSize, this.BackImageMaxSize));

                if (this.BackImageInvert)
                {
                    image = ModernImage.ResizeImage(this.ThemeStyle == ModernThemeStyle.Dark ? this._invertBackImage : this.BackImage, new Rectangle(0, 0, this.BackImageMaxSize, this.BackImageMaxSize));
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

            if (this.ShowStatusStrip)
            {
                using (SolidBrush brush = ModernPaint.GetStyleBrush(this.ColorStyle))
                {
                    this.StatusStrip.Height = 20;
                    this.StatusStrip.Width = this.Width - 20;
                    this.StatusStrip.Location = new Point(0, this.Height - 20);

                    Rectangle bottomRectangle = new Rectangle(0, this.Height - 20, this.Width, 20);
                    e.Graphics.FillRectangle(brush, bottomRectangle);
                }
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

            if (this.ShowBorder && this.WindowState != FormWindowState.Maximized)
            {
                Color borderColor = ModernPaint.GetStyleColor(this.ColorStyle);

                using (Pen pen = new Pen(borderColor))
                {
                    e.Graphics.DrawLines(
                        pen,
                        new[]
                        {
                            new Point(0, 0),
                            new Point(0, this.Height),
                            new Point(this.Width - 1, this.Height),
                            new Point(this.Width - 1, 0),
                            new Point(0, 0)
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

            this.StatusStrip.Dispose();

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

                DwmApi.MARGINS m = new DwmApi.MARGINS
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

                if (this.ShowBorder)
                {
                    if (this.Width - 1 > e.Location.X && e.Location.X > 1 && e.Location.Y > 1 + this.TopBarHeight)
                    {
                        this.MoveControl();
                    }
                }
                else
                {
                    if (this.Width > e.Location.X && e.Location.X > 0 && e.Location.Y > 0 + this.TopBarHeight)
                    {
                        this.MoveControl();
                    }
                }
            }
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            switch (this.WindowState)
            {
                case FormWindowState.Maximized:

                    if (this._controlBoxDictionary != null && this._controlBoxDictionary.ContainsKey(FormControlBox.Maximize))
                    {
                        this._controlBoxDictionary[FormControlBox.Maximize].Text = "2";
                    }

                    if (this.MaximizeClick != null)
                    {
                        this.MaximizeClick(this, EventArgs.Empty);
                    }

                    break;

                case FormWindowState.Minimized:

                    if (this.MinimizeClick != null)
                    {
                        this.MinimizeClick(this, EventArgs.Empty);
                    }

                    break;

                case FormWindowState.Normal:

                    if (this._controlBoxDictionary != null && this._controlBoxDictionary.ContainsKey(FormControlBox.Maximize))
                    {
                        this._controlBoxDictionary[FormControlBox.Maximize].Text = "1";
                    }

                    if (this.NormalClick != null)
                    {
                        this.NormalClick(this, EventArgs.Empty);
                    }

                    break;

                default:
                    break;
            }
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

            base.WndProc(ref m);

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

            if (this.RectangleToScreen(new Rectangle(0, 0, this.ClientRectangle.Width, 50)).Contains(vPoint))
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
            if (this._controlBoxDictionary == null)
            {
                this._controlBoxDictionary = new Dictionary<FormControlBox, ModernControlBox>();
            }

            if (this._controlBoxDictionary.ContainsKey(controlBox))
            {
                return;
            }

            ModernControlBox modernControlBox = new ModernControlBox();

            if (controlBox == FormControlBox.Close)
            {
                modernControlBox.Text = "r";
            }
            else if (controlBox == FormControlBox.Minimize)
            {
                modernControlBox.Text = "0";
            }
            else if (controlBox == FormControlBox.Maximize)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    modernControlBox.Text = "1";
                }
                else
                {
                    modernControlBox.Text = "2";
                }
            }

            modernControlBox.ColorStyle = this.ColorStyle;
            modernControlBox.ThemeStyle = this.ThemeStyle;
            modernControlBox.Tag = controlBox;
            modernControlBox.Size = new Size(25, 20);
            modernControlBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            modernControlBox.TabStop = false;
            modernControlBox.Click += this.OnControlBoxClick;
            modernControlBox.UseCustomBackColor = this.ControlBoxUseCustomBackColor;
            modernControlBox.UseCustomForeColor = this.ControlBoxUseCustomForeColor;

            if (this.ControlBoxUseCustomBackColor)
            {
                modernControlBox.BackColor = this.ControlBoxCustomBackColor;
            }

            if (this.ControlBoxUseCustomForeColor)
            {
                modernControlBox.ForeColor = this.ControlBoxCustomForeColor;
            }

            this.Controls.Add(modernControlBox);

            this._controlBoxDictionary.Add(controlBox, modernControlBox);
        }

        /// <summary>
        /// Handles the Click event of the control box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnControlBoxClick(object sender, EventArgs e)
        {
            ModernControlBox modernControlBox = sender as ModernControlBox;

            if (modernControlBox != null)
            {
                FormControlBox controlBoxFlag = (FormControlBox)modernControlBox.Tag;

                if (controlBoxFlag == FormControlBox.Close)
                {
                    this.DialogResult = DialogResult.Abort;

                    if (this.CloseClick != null)
                    {
                        this.CloseClick(this, EventArgs.Empty);
                    }

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
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
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

            Point firstControlBoxLocation;

            if (this.ShowBorder)
            {
                firstControlBoxLocation = new Point(this.ClientRectangle.Width - 25 - 1, (int)this.TopBarHeight + 1);
            }
            else
            {
                firstControlBoxLocation = new Point(this.ClientRectangle.Width - 25, (int)this.TopBarHeight);
            }

            int lastDrawedControlBoxPosition = firstControlBoxLocation.X - 25;

            ModernControlBox firstControlBox = null;

            if (this._controlBoxDictionary.Count == 1)
            {
                foreach (KeyValuePair<FormControlBox, ModernControlBox> item in this._controlBoxDictionary)
                {
                    item.Value.Location = firstControlBoxLocation;
                }
            }
            else
            {
                foreach (KeyValuePair<int, FormControlBox> item in priorityOrder)
                {
                    bool exists = this._controlBoxDictionary.ContainsKey(item.Value);

                    if (firstControlBox == null && exists)
                    {
                        firstControlBox = this._controlBoxDictionary[item.Value];
                        firstControlBox.Location = firstControlBoxLocation;
                        continue;
                    }

                    if (firstControlBox == null || !exists)
                    {
                        continue;
                    }

                    this._controlBoxDictionary[item.Value].Location = new Point(lastDrawedControlBoxPosition, (int)this.TopBarHeight + (this.ShowBorder ? 1 : 0));
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
            this.Owner = this._shadowForm.TargetFormOwner;
            this._shadowForm.Owner = null;
            this._shadowForm.Dispose();
            this._shadowForm = null;
        }
    }
}
