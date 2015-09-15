//-----------------------------------------------------------------------
// <copyright file="ClipboardEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Forms
{
    using System;

    /// <summary>
    /// Clipboard EventArgs.
    /// </summary>
    [Serializable]
    public class ClipboardEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClipboardEventArgs"/> class.
        /// </summary>
        /// <param name="clipboardText">The clipboard text.</param>
        public ClipboardEventArgs(string clipboardText)
        {
            this.ClipboardText = clipboardText;
        }

        /// <summary>
        /// Gets or sets the clipboard text.
        /// </summary>
        public string ClipboardText
        {
            get;
            set;
        }
    }
}
