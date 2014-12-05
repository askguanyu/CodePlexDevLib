//-----------------------------------------------------------------------
// <copyright file="ModernBackImageBuffer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;

    /// <summary>
    /// ModernBackImageBuffer class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Reviewed.")]
    internal sealed class ModernBackImageBuffer
    {
        /// <summary>
        /// Filed _buffer.
        /// </summary>
        private readonly Bitmap _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernBackImageBuffer" /> class.
        /// </summary>
        /// <param name="bufferSize">Size of buffer.</param>
        public ModernBackImageBuffer(Size bufferSize)
        {
            this._buffer = new Bitmap(bufferSize.Width, bufferSize.Height, PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// Create graphics.
        /// </summary>
        /// <returns>Graphics instance.</returns>
        public Graphics CreateGraphics()
        {
            Graphics result = Graphics.FromImage(this._buffer);

            result.CompositingMode = CompositingMode.SourceOver;
            result.CompositingQuality = CompositingQuality.HighQuality;
            result.InterpolationMode = InterpolationMode.High;
            result.PixelOffsetMode = PixelOffsetMode.HighQuality;
            result.SmoothingMode = SmoothingMode.AntiAlias;
            result.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            return result;
        }

        /// <summary>
        /// Draw graphics.
        /// </summary>
        /// <param name="g">Graphics to draw.</param>
        public void Draw(Graphics g)
        {
            g.DrawImageUnscaled(this._buffer, Point.Empty);
        }
    }
}