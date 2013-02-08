//-----------------------------------------------------------------------
// <copyright file="AsyncSocketErrorEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;

    /// <summary>
    /// AsyncSocketError EventArgs.
    /// </summary>
    public class AsyncSocketErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketErrorEventArgs" /> class.
        /// </summary>
        public AsyncSocketErrorEventArgs()
            : this(string.Empty, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketErrorEventArgs" /> class.
        /// </summary>
        /// <param name="message">Error Message.</param>
        /// <param name="exception">Instance of Exception.</param>
        /// <param name="errorCode">Instance of AsyncSocketErrorCodeEnum.</param>
        public AsyncSocketErrorEventArgs(string message, Exception exception, AsyncSocketErrorCodeEnum errorCode = AsyncSocketErrorCodeEnum.UnspecifiedException)
        {
            this.Message = message;
            this.Exception = exception;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets Error Message.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Exception.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Error Code.
        /// </summary>
        public AsyncSocketErrorCodeEnum ErrorCode
        {
            get;
            private set;
        }
    }
}
