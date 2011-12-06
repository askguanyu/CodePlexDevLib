//-----------------------------------------------------------------------
// <copyright file="AsyncSocketClientConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// AsyncSocketClient constant and default value
    /// </summary>
    internal static class AsyncSocketClientConstants
    {
        /// <summary>
        ///
        /// </summary>
        internal const int BufferSize = 0x10240;

        /// <summary>
        ///
        /// </summary>
        internal const string ClientSendBytesStringFormat = "Bytes send by the client: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientConnectSuccessfully = "Client connect successfully...";

        /// <summary>
        ///
        /// </summary>
        internal const string DebugStringFormat = "Debug: {0}.";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientConnectExceptionStringFormat = "Client connect failure with exception: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientDisconnectExceptionStringFormat = "Client disconnect failure with exception: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientSendExceptionStringFormat = "Client send failure with exception: {0}";
    }
}
