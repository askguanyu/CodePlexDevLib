using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevLib.Net.AsyncSocket
{
    /// <summary>
    /// Async Socket Error EventArgs Class
    /// </summary>
    public class AsyncSocketErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor of AsyncSocketErrorEventArgs
        /// </summary>
        public AsyncSocketErrorEventArgs() : this(string.Empty, null) { }

        /// <summary>
        /// Constructor of AsyncSocketErrorEventArgs
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="exception">Exception object</param>
        /// <param name="errorCode">AsyncSocketServerErrorCodeEnum</param>
        public AsyncSocketErrorEventArgs(string message, Exception exception, AsyncSocketErrorCodeEnum errorCode = AsyncSocketErrorCodeEnum.ThrowSocketException)
        {
            this.Message = message;
            this.Exception = exception;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets Error Message
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Exception
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Error Code
        /// </summary>
        public AsyncSocketErrorCodeEnum ErrorCode
        {
            get;
            set;
        }
    }
}
