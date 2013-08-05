//-----------------------------------------------------------------------
// <copyright file="ITerminalServerHandle.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;

    /// <summary>
    /// Wraps the native terminal server handle.
    /// </summary>
    internal interface ITerminalServerHandle : IDisposable
    {
        /// <summary>
        /// Gets the underlying terminal server handle provided by Windows in a call to WTSOpenServer.
        /// </summary>
        IntPtr Handle
        {
            get;
        }

        /// <summary>
        /// Gets the name of the terminal server for this connection.
        /// </summary>
        string ServerName
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the connection to the server is currently open.
        /// </summary>
        bool IsOpen
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the handle is for the local server.
        /// </summary>
        bool IsLocal
        {
            get;
        }

        /// <summary>
        /// Opens the terminal server handle.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the terminal server handle.
        /// </summary>
        void Close();
    }
}