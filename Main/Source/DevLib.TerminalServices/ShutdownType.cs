﻿//-----------------------------------------------------------------------
// <copyright file="ShutdownType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// The type of terminal server shutdown to perform as specified in a call to <see cref="TerminalServer.Shutdown" />.
    /// </summary>
    public enum ShutdownType
    {
        /// <summary>
        /// Logs off all sessions on the server other than the one calling <see cref="TerminalServer.Shutdown" />, preventing any new connections until the server is restarted.
        /// </summary>
        LogoffAllSessions = 0x00000001,

        /// <summary>
        /// Shuts down the server without powering it off.
        /// </summary>
        Shutdown = 0x00000002,

        /// <summary>
        /// Reboots the server.
        /// </summary>
        Reboot = 0x00000004,

        /// <summary>
        /// Shuts down and powers off the server.
        /// </summary>
        PowerOff = 0x00000008,

        /// <summary>
        /// This value is not yet supported by Windows.
        /// </summary>
        FastReboot = 0x00000010,
    }
}
