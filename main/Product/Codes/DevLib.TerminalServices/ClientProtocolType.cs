//-----------------------------------------------------------------------
// <copyright file="ClientProtocolType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// The protocol that a client is using to connect to a terminal server.
    /// </summary>
    public enum ClientProtocolType : short
    {
        /// <summary>
        /// The client is directly connected to the console session.
        /// </summary>
        Console = 0,

        /// <summary>
        /// This value exists for legacy purposes.
        /// </summary>
        Legacy = 1,

        /// <summary>
        /// The client is connected via the RDP protocol.
        /// </summary>
        Rdp = 2,
    }
}
