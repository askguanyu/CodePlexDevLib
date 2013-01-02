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
        /// Const Field BufferSize.
        /// </summary>
        internal const int BufferSize = 10240;

        /// <summary>
        /// Const Field ClientSendBytesStringFormat.
        /// </summary>
        internal const string ClientSendBytesStringFormat = "AsyncSocketClient: Bytes send by the client: {0}";

        /// <summary>
        /// Const Field ClientConnectSuccessfully.
        /// </summary>
        internal const string ClientConnectSuccessfully = "Succeed: AsyncSocketClient connect successfully...";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
