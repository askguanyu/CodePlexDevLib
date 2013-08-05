//-----------------------------------------------------------------------
// <copyright file="RemoteMessageBoxResult.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Specifies the user's response to a remote message box.
    /// </summary>
    public enum RemoteMessageBoxResult
    {
        /// <summary>
        /// The user pressed the "OK" button.
        /// </summary>
        Ok = 1,

        /// <summary>
        /// The user pressed the "Cancel" button.
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// The user pressed the "Abort" button.
        /// </summary>
        Abort = 3,

        /// <summary>
        /// The user pressed the "Retry" button.
        /// </summary>
        Retry = 4,

        /// <summary>
        /// The user pressed the "Ignore" button.
        /// </summary>
        Ignore = 5,

        /// <summary>
        /// The user pressed the "Yes" button.
        /// </summary>
        Yes = 6,

        /// <summary>
        /// The user pressed the "No" button.
        /// </summary>
        No = 7,

        /// <summary>
        /// The timeout period expired before the user responded to the message box.
        /// </summary>
        Timeout = 0x7D00,

        /// <summary>
        /// The <c>synchronous</c> parameter of <see cref="TerminalServicesSession.MessageBox" /> was set to false, so the method returned immediately, without waiting for a response from the user.
        /// </summary>
        Asynchronous = 0x7D01,
    }
}
