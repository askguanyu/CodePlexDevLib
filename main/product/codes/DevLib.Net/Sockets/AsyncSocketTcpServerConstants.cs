//-----------------------------------------------------------------------
// <copyright file="AsyncSocketTcpServerConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Sockets
{
    /// <summary>
    /// AsyncSocketTcpServer constant and default value.
    /// </summary>
    internal static class AsyncSocketTcpServerConstants
    {
        /// <summary>
        /// The maximum length of the pending connections queue.
        /// </summary>
        internal const int Backlog = int.MaxValue;

        /// <summary>
        /// Const Field TcpServerStartException.
        /// </summary>
        internal const string TcpServerStartException = "Failed: AsyncSocketTcpServer Start.";

        /// <summary>
        /// Const Field TcpServerStopException.
        /// </summary>
        internal const string TcpServerStopException = "Failed: AsyncSocketTcpServer Stop.";

        /// <summary>
        /// Const Field TcpServerAcceptSessionException.
        /// </summary>
        internal const string TcpServerAcceptSessionException = "Failed: AsyncSocketTcpServer Accept.";

        /// <summary>
        /// Const Field TcpServerSendException.
        /// </summary>
        internal const string TcpServerSendException = "Failed: AsyncSocketTcpServer Send.";

        /// <summary>
        /// Const Field TcpServerReceiveException.
        /// </summary>
        internal const string TcpServerReceiveException = "Failed: AsyncSocketTcpServer Receive.";

        /// <summary>
        /// Const Field TcpServerStartSucceeded.
        /// </summary>
        internal const string TcpServerStartSucceeded = "Succeeded: AsyncSocketTcpServer Start.";

        /// <summary>
        /// Const Field TcpServerStopSucceeded.
        /// </summary>
        internal const string TcpServerStopSucceeded = "Succeeded: AsyncSocketTcpServer Stop.";

        /// <summary>
        /// Const Field TcpServerReceiveTotalBytesStringFormat.
        /// </summary>
        internal const string TcpServerReceiveTotalBytesStringFormat = "Total bytes receive by AsyncSocketTcpServer: {0}";

        /// <summary>
        /// Const Field TcpServerSendTotalBytesStringFormat.
        /// </summary>
        internal const string TcpServerSendTotalBytesStringFormat = "Total bytes send by the AsyncSocketTcpServer: {0}";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
