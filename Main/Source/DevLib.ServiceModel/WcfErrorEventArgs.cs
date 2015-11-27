//-----------------------------------------------------------------------
// <copyright file="WcfErrorEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// WcfError EventArgs.
    /// </summary>
    public class WcfErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public WcfErrorEventArgs(Exception exception)
        {
            this.InnerException = exception;
        }

        /// <summary>
        /// Gets the inner exception.
        /// </summary>
        public Exception InnerException
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.InnerException != null ? string.Format("Exception={0}, Source={1}", this.InnerException.ToString(), this.InnerException.Source ?? string.Empty) : "Exception=null, Source=null";
        }
    }
}
