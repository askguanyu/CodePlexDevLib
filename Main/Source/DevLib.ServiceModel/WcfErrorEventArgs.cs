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
        /// Initializes a new instance of the <see cref="WcfErrorEventArgs" /> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="source">The source.</param>
        public WcfErrorEventArgs(Exception exception, string source)
        {
            this.Exception = exception;
            this.Source = source;
        }

        /// <summary>
        /// Gets the inner exception.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the source that causes the error.
        /// </summary>
        public string Source
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
            return string.Format("Exception={0}, Source={1}", this.Exception != null ? this.Exception.ToString() : "null", this.Source ?? "null");
        }
    }
}
