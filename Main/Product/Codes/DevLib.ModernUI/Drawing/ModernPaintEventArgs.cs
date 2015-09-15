//-----------------------------------------------------------------------
// <copyright file="ModernPaintEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Drawing
{
    using System;
    using System.Drawing;

    /// <summary>
    /// ModernPaint EventArgs class.
    /// </summary>
    public class ModernPaintEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModernPaintEventArgs" /> class.
        /// </summary>
        /// <param name="backColor">Back color.</param>
        /// <param name="foreColor">Fore color.</param>
        /// <param name="g">Graphics instance.</param>
        public ModernPaintEventArgs(Color backColor, Color foreColor, Graphics g)
        {
            this.BackColor = backColor;
            this.ForeColor = foreColor;
            this.Graphics = g;
        }

        /// <summary>
        /// Gets back color.
        /// </summary>
        public Color BackColor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets fore color.
        /// </summary>
        public Color ForeColor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets graphics.
        /// </summary>
        public Graphics Graphics
        {
            get;
            private set;
        }
    }
}
