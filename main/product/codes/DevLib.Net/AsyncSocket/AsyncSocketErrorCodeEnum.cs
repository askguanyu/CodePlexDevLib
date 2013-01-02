//-----------------------------------------------------------------------
// <copyright file="AsyncSocketErrorCodeEnum.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// Async Socket Error Code Enum.
    /// </summary>
    public enum AsyncSocketErrorCodeEnum
    {
        /// <summary>
        /// Represents Server Start Exception.
        /// </summary>
        ServerStartException,

        /// <summary>
        /// Represents Server Stop Exception.
        /// </summary>
        ServerStopException,

        /// <summary>
        /// Represents Server Connect Exception.
        /// </summary>
        ServerConnectException,

        /// <summary>
        /// Represents Server Disconnect Exception.
        /// </summary>
        ServerDisconnectException,

        /// <summary>
        /// Represents Server Accept Exception.
        /// </summary>
        ServerAcceptException,

        /// <summary>
        /// Represents Server SendBack Exception.
        /// </summary>
        ServerSendBackException,

        /// <summary>
        /// Represents Server Receive Exception.
        /// </summary>
        ServerReceiveException,

        /// <summary>
        /// Represents Client Start Exception.
        /// </summary>
        ClientStartException,

        /// <summary>
        /// Represents Client Stop Exception.
        /// </summary>
        ClientStopException,

        /// <summary>
        /// Represents Client Connect Exception.
        /// </summary>
        ClientConnectException,

        /// <summary>
        /// Represents Client Disconnect Exception.
        /// </summary>
        ClientDisconnectException,

        /// <summary>
        /// Represents Client Accept Exception.
        /// </summary>
        ClientAcceptException,

        /// <summary>
        /// Represents Client Send Exception.
        /// </summary>
        ClientSendException,

        /// <summary>
        /// Represents Client Receive Exception.
        /// </summary>
        ClientReceiveException,

        /// <summary>
        /// Represents Socket No Exist.
        /// </summary>
        SocketNoExist,

        /// <summary>
        /// Represents Throw Socket Exception.
        /// </summary>
        ThrowSocketException,
    }
}
