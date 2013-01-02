//-----------------------------------------------------------------------
// <copyright file="AsyncSocketErrorEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;

    /// <summary>
    /// Async Socket Error EventArgs Class.
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
        /// <param name="exception">Exception object.</param>
        /// <param name="errorCode">Instance of AsyncSocketErrorCodeEnum.</param>
        public AsyncSocketErrorEventArgs(string message, Exception exception, AsyncSocketErrorCodeEnum errorCode = AsyncSocketErrorCodeEnum.ThrowSocketException)
        {
            this.Message = message;
            this.Exception = exception;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets Error Message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Exception.
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Error Code.
        /// </summary>
        public AsyncSocketErrorCodeEnum ErrorCode
        {
            get;
            set;
        }
    }
}
