//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostDispatchMessageInspector.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// WcfServiceHost DispatchMessageInspector.
    /// </summary>
    [Serializable]
    public class WcfServiceHostDispatchMessageInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Receiving;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Replying;

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="instanceContext">The instance context.</param>
        /// <returns>The object used to correlate state.</returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            string message = null;

            if (request != null)
            {
                message = request.ToString();
            }
            else
            {
                message = string.Empty;
            }

            Debug.WriteLine(message);

            this.RaiseEvent(this.Receiving, WcfServiceHostState.Receiving, message);

            return null;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="correlationState">State of the correlation.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            string message = null;

            if (reply != null)
            {
                message = reply.ToString();
            }
            else
            {
                message = string.Empty;
            }

            Debug.WriteLine(message);

            this.RaiseEvent(this.Replying, WcfServiceHostState.Replying, message);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, WcfServiceHostState state, string message)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(null, state, message));
            }
        }
    }
}
