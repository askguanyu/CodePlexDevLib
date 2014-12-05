//-----------------------------------------------------------------------
// <copyright file="ModernFlatDropShadow.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Security;
    using System.Windows.Forms;
    using DevLib.ModernUI.NativeAPI;

    /// <summary>
    /// ModernFlatDropShadow class.
    /// </summary>
    internal class ModernFlatDropShadow : ModernShadowBase
    {
        /// <summary>
        /// The offset
        /// </summary>
        private Point _offset = new Point(-6, -6);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernFlatDropShadow"/> class.
        /// </summary>
        /// <param name="targetForm">The target form.</param>
        public ModernFlatDropShadow(Form targetForm)
            : base(targetForm, 6, ModernShadowBase.WS_EX_LAYERED | ModernShadowBase.WS_EX_TRANSPARENT | ModernShadowBase.WS_EX_NOACTIVATE)
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.PaintShadow();
        }

        /// <summary>
        /// OnPaint method.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            this.Visible = true;
            this.PaintShadow();
        }

        /// <summary>
        /// Paints the shadow.
        /// </summary>
        protected override void PaintShadow()
        {
            using (Bitmap bitmap = this.DrawBlurBorder())
            {
                this.SetBitmap(bitmap, 255);
            }
        }

        /// <summary>
        /// Clears the shadow.
        /// </summary>
        protected override void ClearShadow()
        {
            using (Bitmap image = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb))
            {
                Graphics g = Graphics.FromImage(image);
                g.Clear(Color.Transparent);
                g.Flush();
                g.Dispose();
                this.SetBitmap(image, 255);
            }
        }

        /// <summary>
        /// Sets the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="opacity">The opacity.</param>
        /// <exception cref="System.ApplicationException">The bitmap must be 32ppp with alpha-channel.</exception>
        [SecuritySafeCritical]
        private void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");
            }

            IntPtr screenDc = WinApi.GetDC(IntPtr.Zero);
            IntPtr memDc = WinApi.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmap = WinApi.SelectObject(memDc, hBitmap);

                WinApi.SIZE size = new WinApi.SIZE(bitmap.Width, bitmap.Height);
                WinApi.POINT pointSource = new WinApi.POINT(0, 0);
                WinApi.POINT topPos = new WinApi.POINT(Left, Top);
                WinApi.BLENDFUNCTION blend = new WinApi.BLENDFUNCTION();
                blend.BlendOp = WinApi.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = opacity;
                blend.AlphaFormat = WinApi.AC_SRC_ALPHA;

                WinApi.UpdateLayeredWindow(this.Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, WinApi.ULW_ALPHA);
            }
            finally
            {
                WinApi.ReleaseDC(IntPtr.Zero, screenDc);

                if (hBitmap != IntPtr.Zero)
                {
                    WinApi.SelectObject(memDc, oldBitmap);
                    WinApi.DeleteObject(hBitmap);
                }

                WinApi.DeleteDC(memDc);
            }
        }

        /// <summary>
        /// Draws the blur border.
        /// </summary>
        /// <returns>Bitmap instance.</returns>
        private Bitmap DrawBlurBorder()
        {
            return (Bitmap)this.DrawOutsetShadow(Color.Black, new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height));
        }

        /// <summary>
        /// Draws the outset shadow.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="shadowCanvasArea">The shadow canvas area.</param>
        /// <returns>Image instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private Image DrawOutsetShadow(Color color, Rectangle shadowCanvasArea)
        {
            Rectangle rOuter = shadowCanvasArea;
            Rectangle rInner = new Rectangle(shadowCanvasArea.X + (-this._offset.X - 1), shadowCanvasArea.Y + (-this._offset.Y - 1), shadowCanvasArea.Width - ((-this._offset.X * 2) - 1), shadowCanvasArea.Height - ((-this._offset.Y * 2) - 1));

            Bitmap result = new Bitmap(rOuter.Width, rOuter.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (Brush brush = new SolidBrush(Color.FromArgb(30, Color.Black)))
                {
                    g.FillRectangle(brush, rOuter);
                }

                using (Brush brush = new SolidBrush(Color.FromArgb(60, Color.Black)))
                {
                    g.FillRectangle(brush, rInner);
                }
            }

            return result;
        }
    }
}
