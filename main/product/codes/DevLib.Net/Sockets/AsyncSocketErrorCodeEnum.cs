//-----------------------------------------------------------------------
// <copyright file="AsyncSocketErrorCodeEnum.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Sockets
{
    /// <summary>
    /// Enum AsyncSocketErrorCode.
    /// </summary>
    public enum AsyncSocketErrorCodeEnum
    {
        /// <summary>
        /// Represents UnspecifiedException.
        /// </summary>
        UnspecifiedException,

        /// <summary>
        /// Represents TcpServerStartException.
        /// </summary>
        TcpServerStartException,

        /// <summary>
        /// Represents TcpServerStopException.
        /// </summary>
        TcpServerStopException,

        /// <summary>
        /// Represents TcpServerAcceptSessionException.
        /// </summary>
        TcpServerAcceptSessionException,

        /// <summary>
        /// Represents TcpServerCloseSessionException.
        /// </summary>
        TcpServerCloseSessionException,

        /// <summary>
        /// Represents TcpServerSendException.
        /// </summary>
        TcpServerSendException,

        /// <summary>
        /// Represents TcpServerReceiveException.
        /// </summary>
        TcpServerReceiveException,

        /// <summary>
        /// Represents TcpClientStartException.
        /// </summary>
        TcpClientStartException,

        /// <summary>
        /// Represents TcpClientStopException.
        /// </summary>
        TcpClientStopException,

        /// <summary>
        /// Represents TcpClientConnectException.
        /// </summary>
        TcpClientConnectException,

        /// <summary>
        /// Represents TcpClientDisconnectException.
        /// </summary>
        TcpClientDisconnectException,

        /// <summary>
        /// Represents TcpClientSendException.
        /// </summary>
        TcpClientSendException,

        /// <summary>
        /// Represents TcpClientReceiveException.
        /// </summary>
        TcpClientReceiveException,

        /// <summary>
        /// Represents UdpServerStartException.
        /// </summary>
        UdpServerStartException,

        /// <summary>
        /// Represents UdpServerStopException.
        /// </summary>
        UdpServerStopException,

        /// <summary>
        /// Represents UdpServerReceiveException.
        /// </summary>
        UdpServerReceiveException,

        /// <summary>
        /// Represents UdpClientStartException.
        /// </summary>
        UdpClientStartException,

        /// <summary>
        /// Represents UdpClientStopException.
        /// </summary>
        UdpClientStopException,

        /// <summary>
        /// Represents UdpClientSendException.
        /// </summary>
        UdpClientSendException,
    }
}
