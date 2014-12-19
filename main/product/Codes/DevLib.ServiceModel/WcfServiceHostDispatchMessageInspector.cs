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
        public event EventHandler<WcfServiceHostEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> SendingReply;

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="instanceContext">The instance context.</param>
        /// <returns>The object used to correlate state.</returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            Guid messageId = Guid.NewGuid();

            string message = null;

            if (request != null)
            {
                message = request.ToString();
            }
            else
            {
                message = string.Empty;
            }

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostDispatchMessageInspector.AfterReceiveRequest: " + messageId.ToString());
            Debug.WriteLine(message);

            this.RaiseEvent(this.ReceivingRequest, WcfServiceHostState.Receiving, request, message, messageId);

            return messageId;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="correlationState">State of the correlation.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            Guid messageId = Guid.Empty;

            try
            {
                messageId = (Guid)correlationState;
            }
            catch
            {
            }

            string message = null;

            if (reply != null)
            {
                message = reply.ToString();
            }
            else
            {
                message = string.Empty;
            }

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostDispatchMessageInspector.BeforeSendReply: " + messageId.ToString());
            Debug.WriteLine(message);

            this.RaiseEvent(this.SendingReply, WcfServiceHostState.Replying, reply, message, messageId);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="state">The state.</param>
        /// <param name="channelMessage">The channel message.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, WcfServiceHostState state, Message channelMessage, string message, Guid messageId)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, new WcfServiceHostEventArgs(null, state, null, channelMessage, message, messageId));
            }
        }
    }
}
