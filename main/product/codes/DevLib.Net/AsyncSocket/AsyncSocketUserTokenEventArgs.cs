//-----------------------------------------------------------------------
// <copyright file="AsyncSocketUserTokenEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// This class is designed for use as the object to be assigned to the SocketAsyncEventArgs.UserToken property.
    /// </summary>
    public class AsyncSocketUserTokenEventArgs : EventArgs
    {
        /// <summary>
        ///
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Constructor of AsyncUserToken
        /// </summary>
        public AsyncSocketUserTokenEventArgs() : this(null) { }

        /// <summary>
        /// Constructor of AsyncUserToken
        /// </summary>
        /// <param name="socket">Socket context</param>
        public AsyncSocketUserTokenEventArgs(Socket socket)
        {
            this.ReadEventArgs = new SocketAsyncEventArgs();
            this.ReadEventArgs.UserToken = this;
            if (null != socket)
            {
                this._socket = socket;
            }
        }

        /// <summary>
        /// Gets or sets SocketAsyncEventArgs
        /// </summary>
        public SocketAsyncEventArgs ReadEventArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets length of received data bytes
        /// </summary>
        public int ReceivedBytesSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets offset of buffer
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets received buffer
        /// </summary>
        public byte[] ReceivedBuffer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets received raw Data
        /// </summary>
        public byte[] ReceivedRawData
        {
            get
            {
                byte[] data = new byte[this.ReceivedBytesSize];
                Array.Copy(this.ReceivedBuffer, this.Offset, data, 0, this.ReceivedBytesSize);
                return data;
            }
        }

        /// <summary>
        /// Gets or sets user defined operation flag
        /// </summary>
        public object Operation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets socket context
        /// </summary>
        public Socket Socket
        {
            get
            {
                return _socket;
            }

            set
            {
                if (value != null)
                {
                    _socket = value;
                    this.EndPoint = (IPEndPoint)_socket.RemoteEndPoint;
                }
            }
        }

        /// <summary>
        /// Gets or sets connection Id
        /// </summary>
        public Guid ConnectionId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets connected client EndPoint
        /// </summary>
        public IPEndPoint EndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Set BytesReceived
        /// </summary>
        /// <param name="bytesReceived">Bytes Received</param>
        public void SetBytesReceived(int bytesReceived)
        {
            this.ReceivedBytesSize = bytesReceived;
        }

        /// <summary>
        /// Set Buffer
        /// </summary>
        /// <param name="buffer">buffer</param>
        public void SetBuffer(byte[] buffer, int offset)
        {
            this.ReceivedBuffer = buffer;
            this.Offset = offset;
            this.ReceivedBytesSize = 0;
        }
    }
}
