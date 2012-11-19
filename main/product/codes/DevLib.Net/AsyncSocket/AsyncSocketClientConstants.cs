//-----------------------------------------------------------------------
// <copyright file="AsyncSocketClientConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// AsyncSocketClient constant and default value.
    /// </summary>
    internal static class AsyncSocketClientConstants
    {
        /// <summary>
        ///
        /// </summary>
        internal const int BufferSize = 10240;

        /// <summary>
        ///
        /// </summary>
        internal const string ClientSendBytesStringFormat = "AsyncSocketClient: Bytes send by the client: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientConnectSuccessfully = "Succeed: AsyncSocketClient connect successfully...";

        /// <summary>
        ///
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
