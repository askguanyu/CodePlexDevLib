//-----------------------------------------------------------------------
// <copyright file="ModernRealisticDropShadow.cs" company="YuGuan Corporation">
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
    /// ModernRealisticDropShadow class.
    /// </summary>
    internal class ModernRealisticDropShadow : ModernShadowBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernRealisticDropShadow"/> class.
        /// </summary>
        /// <param name="targetForm">The target form.</param>
        public ModernRealisticDropShadow(Form targetForm)
            : base(targetForm, 15, ModernShadowBase.WS_EX_LAYERED | ModernShadowBase.WS_EX_TRANSPARENT | ModernShadowBase.WS_EX_NOACTIVATE)
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
                WinApi.BLENDFUNCTION blend = new WinApi.BLENDFUNCTION
                {
                    BlendOp = WinApi.AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = opacity,
                    AlphaFormat = WinApi.AC_SRC_ALPHA
                };

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
            return (Bitmap)this.DrawOutsetShadow(0, 0, 40, 1, Color.Black, new Rectangle(1, 1, this.ClientRectangle.Width, this.ClientRectangle.Height));
        }

        /// <summary>
        /// Draws the outset shadow.
        /// </summary>
        /// <param name="hShadow">The h shadow.</param>
        /// <param name="vShadow">The v shadow.</param>
        /// <param name="blur">The blur.</param>
        /// <param name="spread">The spread.</param>
        /// <param name="color">The color.</param>
        /// <param name="shadowCanvasArea">The shadow canvas area.</param>
        /// <returns>Image instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private Image DrawOutsetShadow(int hShadow, int vShadow, int blur, int spread, Color color, Rectangle shadowCanvasArea)
        {
            Rectangle rOuter = shadowCanvasArea;
            Rectangle rInner = shadowCanvasArea;
            rInner.Offset(hShadow, vShadow);
            rInner.Inflate(-blur, -blur);
            rOuter.Inflate(spread, spread);
            rOuter.Offset(hShadow, vShadow);

            Rectangle originalOuter = rOuter;

            Bitmap result = new Bitmap(originalOuter.Width, originalOuter.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int currentBlur = 0;

            do
            {
                double transparency = (rOuter.Height - rInner.Height) / (double)((blur * 2) + (spread * 2));
                Color shadowColor = Color.FromArgb((int)(200 * transparency * transparency), color);
                Rectangle rOutput = rInner;
                rOutput.Offset(-originalOuter.Left, -originalOuter.Top);

                this.DrawRoundedRectangle(g, rOutput, currentBlur, Pens.Transparent, shadowColor);
                rInner.Inflate(1, 1);
                currentBlur = (int)((double)blur * (1 - (transparency * transparency)));
            }
            while (rOuter.Contains(rInner));

            g.Flush();
            g.Dispose();

            return result;
        }

        /// <summary>
        /// Draws the rounded rectangle.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="cornerRadius">The corner radius.</param>
        /// <param name="drawPen">The draw pen.</param>
        /// <param name="fillColor">Color of the fill.</param>
        private void DrawRoundedRectangle(Graphics g, Rectangle bounds, int cornerRadius, Pen drawPen, Color fillColor)
        {
            int strokeOffset = Convert.ToInt32(Math.Ceiling(drawPen.Width));
            bounds = Rectangle.Inflate(bounds, -strokeOffset, -strokeOffset);

            using (GraphicsPath gfxPath = new GraphicsPath())
            {
                if (cornerRadius > 0)
                {
                    gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
                    gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
                    gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                    gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                }
                else
                {
                    gfxPath.AddRectangle(bounds);
                }

                gfxPath.CloseAllFigures();

                if (cornerRadius > 5)
                {
                    using (SolidBrush brush = new SolidBrush(fillColor))
                    {
                        g.FillPath(brush, gfxPath);
                    }
                }

                if (drawPen != Pens.Transparent)
                {
                    using (Pen pen = new Pen(drawPen.Color))
                    {
                        pen.EndCap = pen.StartCap = LineCap.Round;
                        g.DrawPath(pen, gfxPath);
                    }
                }
            }
        }
    }
}
