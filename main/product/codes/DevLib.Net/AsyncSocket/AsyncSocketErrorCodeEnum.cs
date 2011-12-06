using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// Async Socket Error Code Enum
    /// </summary>
    public enum AsyncSocketErrorCodeEnum
    {
        ServerStartException,
        ServerStopException,
        ServerConnectException,
        ServerDisconnectException,
        ServerAcceptException,
        ServerSendBackException,
        ServerReceiveException,
        ClientStartException,
        ClientStopException,
        ClientConnectException,
        ClientDisconnectException,
        ClientAcceptException,
        ClientSendException,
        ClientReceiveException,
        SocketNoExist,
        ThrowSocketException,
    };
}
