//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServerConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// AsyncSocketServer constant and default value.
    /// </summary>
    internal static class AsyncSocketServerConstants
    {
        /// <summary>
        /// read, write (don't alloc buffer space for accepts).
        /// </summary>
        internal const int OpsToPreAlloc = 2;

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        internal const int Backlog = 128;

        /// <summary>
        /// Const Field NumConnections.
        /// </summary>
        internal const int NumConnections = 4096;

        /// <summary>
        /// Const Field BufferSize.
        /// </summary>
        internal const int BufferSize = 10240;

        /// <summary>
        /// Const Field AsyncSocketException.
        /// </summary>
        internal const string AsyncSocketException = "Async communication exception";

        /// <summary>
        /// Const Field SocketAsyncEventArgsPoolArgumentNullException.
        /// </summary>
        internal const string SocketAsyncEventArgsPoolArgumentNullException = "Items added to a SocketAsyncEventArgsPool cannot be null";

        /// <summary>
        /// Const Field SocketStartException.
        /// </summary>
        internal const string SocketStartException = "Socket Server start failure";

        /// <summary>
        /// Const Field SocketStopException.
        /// </summary>
        internal const string SocketStopException = "Socket Server stop exception";

        /// <summary>
        /// Const Field SocketStartSuccessfully.
        /// </summary>
        internal const string SocketStartSuccessfully = "Succeed: Socket server start successfully...";

        /// <summary>
        /// Const Field ClientClosedStringFormat.
        /// </summary>
        internal const string ClientClosedStringFormat = "Client: {0} is closed or not connected";

        /// <summary>
        /// Const Field SocketSendException.
        /// </summary>
        internal const string SocketSendException = "Sends data asynchronously SocketException";

        /// <summary>
        /// Const Field SocketAcceptedException.
        /// </summary>
        internal const string SocketAcceptedException = "Socket server accepted failure";

        /// <summary>
        /// Const Field ClientConnectionStringFormat.
        /// </summary>
        internal const string ClientConnectionStringFormat = "There are {0} clients connected to the server";

        /// <summary>
        /// Const Field SocketReceiveException.
        /// </summary>
        internal const string SocketReceiveException = "Receives data asynchronously SocketException";

        /// <summary>
        /// Const Field SocketLastOperationException.
        /// </summary>
        internal const string SocketLastOperationException = "The last operation completed on the socket was not a receive or send";

        /// <summary>
        /// Const Field ServerReceiveTotalBytesStringFormat.
        /// </summary>
        internal const string ServerReceiveTotalBytesStringFormat = "Total bytes receive by the server: {0}";

        /// <summary>
        /// Const Field ServerSendTotalBytesStringFormat.
        /// </summary>
        internal const string ServerSendTotalBytesStringFormat = "Total bytes send by the server: {0}";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
