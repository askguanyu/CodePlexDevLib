//-----------------------------------------------------------------------
// <copyright file="AsyncSocketSessionEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Net;

    /// <summary>
    /// AsyncSocketSession EventArgs.
    /// </summary>
    public class AsyncSocketSessionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketSessionEventArgs" /> class.
        /// </summary>
        public AsyncSocketSessionEventArgs()
            : this(-1, null, null, 0, 0, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketSessionEventArgs" /> class.
        /// </summary>
        /// <param name="sessionId">Connected client Id.</param>
        /// <param name="sessionIPEndPoint">Connected client IPEndPoint.</param>
        public AsyncSocketSessionEventArgs(int sessionId, IPEndPoint sessionIPEndPoint)
            : this(sessionId, sessionIPEndPoint, null, 0, 0, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketSessionEventArgs" /> class.
        /// </summary>
        /// <param name="sessionId">Connected client Id.</param>
        /// <param name="sessionIPEndPoint">Connected client IPEndPoint.</param>
        /// <param name="buffer">Buffer to use with an asynchronous socket method.</param>
        /// <param name="bytesTransferred">The number of bytes transferred in the socket operation.</param>
        /// <param name="offset">Offset of the buffer.</param>
        /// <param name="userToken">A user or application object associated with this asynchronous socket operation.</param>
        public AsyncSocketSessionEventArgs(int sessionId, IPEndPoint sessionIPEndPoint, byte[] buffer, int bytesTransferred, int offset, object userToken = null)
        {
            this.SessionId = sessionId;
            this.SessionIPEndPoint = sessionIPEndPoint;
            this.Buffer = buffer;
            this.BytesTransferred = bytesTransferred;
            this.Offset = offset;
            this.UserToken = userToken;
        }

        /// <summary>
        /// Gets connected client Id.
        /// </summary>
        public int SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Session IPEndPoint.
        /// </summary>
        public IPEndPoint SessionIPEndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a user or application object associated with this asynchronous socket operation.
        /// </summary>
        public object UserToken
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of bytes transferred in the socket operation.
        /// </summary>
        public int BytesTransferred
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the offset of the buffer.
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data buffer to use with an asynchronous socket method.
        /// </summary>
        public byte[] Buffer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets transferred data.
        /// </summary>
        public byte[] DataTransferred
        {
            get
            {
                if (this.Buffer != null)
                {
                    byte[] result = new byte[this.BytesTransferred];
                    Array.Copy(this.Buffer, this.Offset, result, 0, this.BytesTransferred);
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
