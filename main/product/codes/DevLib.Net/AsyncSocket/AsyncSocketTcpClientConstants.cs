//-----------------------------------------------------------------------
// <copyright file="AsyncSocketTcpClientConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// AsyncSocketTcpClient constant and default value.
    /// </summary>
    internal static class AsyncSocketTcpClientConstants
    {
        /// <summary>
        /// Const Field TcpClientStartException.
        /// </summary>
        internal const string TcpClientStartException = "Failed: AsyncSocketTcpClient Start.";

        /// <summary>
        /// Const Field TcpClientStopException.
        /// </summary>
        internal const string TcpClientStopException = "Failed: AsyncSocketTcpClient Stop.";

        /// <summary>
        /// Const Field TcpClientSendException.
        /// </summary>
        internal const string TcpClientSendException = "Failed: AsyncSocketTcpClient Send.";

        /// <summary>
        /// Const Field TcpClientConnectException.
        /// </summary>
        internal const string TcpClientConnectException = "Failed: AsyncSocketTcpClient Connect.";

        /// <summary>
        /// Const Field TcpClientReceiveException.
        /// </summary>
        internal const string TcpClientReceiveException = "Failed: AsyncSocketTcpClient Receive.";

        /// <summary>
        /// Const Field TcpClientStartSucceeded.
        /// </summary>
        internal const string TcpClientStartSucceeded = "Succeeded: AsyncSocketTcpClient Start.";

        /// <summary>
        /// Const Field TcpClientStopSucceeded.
        /// </summary>
        internal const string TcpClientStopSucceeded = "Succeeded: AsyncSocketTcpClient Stop.";

        /// <summary>
        /// Const Field TcpClientReceiveTotalBytesStringFormat.
        /// </summary>
        internal const string TcpClientReceiveTotalBytesStringFormat = "Total bytes receive by AsyncSocketTcpClient: {0}";

        /// <summary>
        /// Const Field TcpClientSendTotalBytesStringFormat.
        /// </summary>
        internal const string TcpClientSendTotalBytesStringFormat = "Total bytes send by the AsyncSocketTcpClient: {0}";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failed with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
