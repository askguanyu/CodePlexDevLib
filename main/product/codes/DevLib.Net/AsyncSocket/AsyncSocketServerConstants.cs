//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServerConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// AsyncSocketServer constant and default value
    /// </summary>
    internal static class AsyncSocketServerConstants
    {
        /// <summary>
        /// read, write (don't alloc buffer space for accepts)
        /// </summary>
        internal const int OpsToPreAlloc = 2;

        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        internal const int Backlog = 128;

        /// <summary>
        ///
        /// </summary>
        internal const int NumConnections = 4096;

        /// <summary>
        ///
        /// </summary>
        internal const int BufferSize = 10240;

        /// <summary>
        ///
        /// </summary>
        internal const string AsyncSocketException = "Async communication exception";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketAsyncEventArgsPoolArgumentNullException = "Items added to a SocketAsyncEventArgsPool cannot be null";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketStartException = "Socket Server start failure";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketStopException = "Socket Server stop exception";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketStartSuccessfully = "Succeed: Socket server start successfully...";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientClosedStringFormat = "Client: {0} is closed or not connected";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketSendException = "Sends data asynchronously SocketException";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketAcceptedException = "Socket server accepted failure";

        /// <summary>
        ///
        /// </summary>
        internal const string ClientConnectionStringFormat = "There are {0} clients connected to the server";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketReceiveException = "Receives data asynchronously SocketException";

        /// <summary>
        ///
        /// </summary>
        internal const string SocketLastOperationException = "The last operation completed on the socket was not a receive or send";

        /// <summary>
        ///
        /// </summary>
        internal const string ServerReceiveTotalBytesStringFormat = "Total bytes receive by the server: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ServerSendTotalBytesStringFormat = "Total bytes send by the server: {0}";

        /// <summary>
        ///
        /// </summary>
        internal const string ExceptionStringFormat = "Exception: {0} failure with exception. Source: {1} | Message: {2} | StackTrace: {3}";
    }
}
