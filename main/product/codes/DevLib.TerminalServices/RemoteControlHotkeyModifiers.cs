//-----------------------------------------------------------------------
// <copyright file="RemoteControlHotkeyModifiers.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;

    /// <summary>
    /// The virtual modifier that represents the key to press to stop remote control of the session.
    /// </summary>
    [Flags]
    public enum RemoteControlHotkeyModifiers
    {
        /// <summary>
        /// The SHIFT key.
        /// </summary>
        Shift = 1,

        /// <summary>
        /// The CTRL key.
        /// </summary>
        Control = 2,

        /// <summary>
        /// The ALT key.
        /// </summary>
        Alt = 4
    }
}
