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
            this.SourceException = exception;
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
        /// Gets SourceException.
        /// </summary>
        public Exception SourceException
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

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return string.Format(
                                 "[ErrorCode:] {0}\r\n[Message:] {1}\r\n[Exception:] {2}",
                                 this.ErrorCode.ToString(),
                                 this.Message ?? string.Empty,
                                 this.SourceException == null ? string.Empty : this.SourceException.ToString());
        }
    }
}
