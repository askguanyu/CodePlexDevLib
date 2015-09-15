//-----------------------------------------------------------------------
// <copyright file="AsyncSocketUdpClientConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Sockets
{
    /// <summary>
    /// AsyncSocketUdpClient constant and default value.
    /// </summary>
    internal static class AsyncSocketUdpClientConstants
    {
        /// <summary>
        /// Const Field UdpClientStartException.
        /// </summary>
        internal const string UdpClientStartException = "Failed: AsyncSocketUdpClient Start.";

        /// <summary>
        /// Const Field UdpClientStopException.
        /// </summary>
        internal const string UdpClientStopException = "Failed: AsyncSocketUdpClient Stop.";

        /// <summary>
        /// Const Field UdpClientSendException.
        /// </summary>
        internal const string UdpClientSendException = "Failed: AsyncSocketUdpClient Send.";

        /// <summary>
        /// Const Field UdpClientStartSucceeded.
        /// </summary>
        internal const string UdpClientStartSucceeded = "Succeeded: AsyncSocketUdpClient Start.";

        /// <summary>
        /// Const Field UdpClientStopSucceeded.
        /// </summary>
        internal const string UdpClientStopSucceeded = "Succeeded: AsyncSocketUdpClient Stop.";

        /// <summary>
        /// Const Field UdpClientReceiveTotalBytesStringFormat.
        /// </summary>
        internal const string UdpClientReceiveTotalBytesStringFormat = "Total bytes receive by AsyncSocketUdpClient: {0}";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
