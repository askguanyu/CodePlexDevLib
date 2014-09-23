//-----------------------------------------------------------------------
// <copyright file="ConnectState.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Specifies the connection state of a Remote Desktop Services session.
    /// </summary>
    public enum ConnectState
    {
        /// <summary>
        /// A user is logged on to the WinStation.
        /// </summary>
        Active,

        /// <summary>
        /// The WinStation is connected to the client.
        /// </summary>
        Connected,

        /// <summary>
        /// The WinStation is in the process of connecting to the client.
        /// </summary>
        ConnectQuery,

        /// <summary>
        /// The WinStation is shadowing another WinStation.
        /// </summary>
        Shadow,

        /// <summary>
        /// The WinStation is active but the client is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The WinStation is waiting for a client to connect.
        /// </summary>
        Idle,

        /// <summary>
        /// The WinStation is listening for a connection.
        /// A listener session waits for requests for new client connections.
        /// No user is logged on a listener session.
        /// A listener session cannot be reset, shadowed, or changed to a regular client session.
        /// </summary>
        Listen,

        /// <summary>
        /// The WinStation is being reset.
        /// </summary>
        Reset,

        /// <summary>
        /// The WinStation is down due to an error.
        /// </summary>
        Down,

        /// <summary>
        /// The WinStation is initializing.
        /// </summary>
        Init
    }
}
