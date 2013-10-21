//-----------------------------------------------------------------------
// <copyright file="AsyncSocketUdpServerConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Sockets
{
    /// <summary>
    /// AsyncSocketUdpServer constant and default value.
    /// </summary>
    internal static class AsyncSocketUdpServerConstants
    {
        /// <summary>
        /// Const Field UdpServerStartException.
        /// </summary>
        internal const string UdpServerStartException = "Failed: AsyncSocketUdpServer Start.";

        /// <summary>
        /// Const Field UdpServerStopException.
        /// </summary>
        internal const string UdpServerStopException = "Failed: AsyncSocketUdpServer Stop.";

        /// <summary>
        /// Const Field UdpServerReceiveException.
        /// </summary>
        internal const string UdpServerReceiveException = "Failed: AsyncSocketUdpServer Receive.";

        /// <summary>
        /// Const Field UdpServerStartSucceeded.
        /// </summary>
        internal const string UdpServerStartSucceeded = "Succeeded: AsyncSocketUdpServer Start.";

        /// <summary>
        /// Const Field UdpServerStopSucceeded.
        /// </summary>
        internal const string UdpServerStopSucceeded = "Succeeded: AsyncSocketUdpServer Stop.";

        /// <summary>
        /// Const Field UdpServerReceiveTotalBytesStringFormat.
        /// </summary>
        internal const string UdpServerReceiveTotalBytesStringFormat = "Total bytes receive by AsyncSocketUdpServer: {0}";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
