//-----------------------------------------------------------------------
// <copyright file="WcfErrorHandler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// Wcf ErrorHandler.
    /// </summary>
    public class WcfErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfErrorHandler"/> class.
        /// </summary>
        public WcfErrorHandler()
        {
        }

        /// <summary>
        /// Occurs when has error.
        /// </summary>
        public event EventHandler<WcfErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>true if Windows Communication Foundation (WCF) should not abort the session (if there is one) and instance context if the instance context is not Single; otherwise, false. The default is false.</returns>
        public bool HandleError(Exception error)
        {
            this.RaiseEvent(this.ErrorOccurred, error);

            return true;
        }

        /// <summary>
        /// Provides the fault.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="version">The version.</param>
        /// <param name="fault">The fault.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="exception">The exception.</param>
        private void RaiseEvent(EventHandler<WcfErrorEventArgs> eventHandler, Exception exception)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfErrorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfErrorEventArgs(exception));
            }
        }
    }
}
