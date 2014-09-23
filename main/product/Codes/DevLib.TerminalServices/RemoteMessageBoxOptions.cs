//-----------------------------------------------------------------------
// <copyright file="RemoteMessageBoxOptions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;

    /// <summary>
    /// Specifies additional options for a remote message box.
    /// </summary>
    [Flags]
    public enum RemoteMessageBoxOptions
    {
        /// <summary>
        /// No additional options. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the text in the message box should be right-aligned. The default is left-aligned.
        /// </summary>
        RightAligned = 0x00080000,

        /// <summary>
        /// Specifies that the message box should use a right-to-left reading order.
        /// </summary>
        RtlReading = 0x00100000,

        /// <summary>
        /// Specifies that the message box should be set to the foreground window when displayed.
        /// </summary>
        SetForeground = 0x00010000,

        /// <summary>
        /// Specifies that the message box should appear above all other windows on the screen.
        /// </summary>
        TopMost = 0x00080000,
    }
}
