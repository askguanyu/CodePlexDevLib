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
        internal const string ExceptionStringFormat = "Exception: {0} failure with exception. Source: {1} | Message: {2} | StackTrace: {3}";
    }
}
