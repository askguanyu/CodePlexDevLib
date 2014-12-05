//-----------------------------------------------------------------------
// <copyright file="ModernImage.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Image of modern style.
    /// </summary>
    internal static class ModernImage
    {
        /// <summary>
        /// Resize image.
        /// </summary>
        /// <param name="source">Source image to resize.</param>
        /// <param name="maxOffset">Maximum offset.</param>
        /// <returns>Resized image.</returns>
        public static Image ResizeImage(Image source, Rectangle maxOffset)
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;

            float percent = 0;
            float percentWidth = 0;
            float percentHeight = 0;

            percentWidth = (float)maxOffset.Width / sourceWidth;
            percentHeight = (float)maxOffset.Height / sourceHeight;

            percent = percentHeight < percentWidth ? percentHeight : percentWidth;

            int destWidth = (int)(sourceWidth * percent);
            int destHeight = (int)(sourceHeight * percent);

            return source.GetThumbnailImage(destWidth, destHeight, null, IntPtr.Zero);
        }
    }
}