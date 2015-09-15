//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostDispatchMessageInspector.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// WcfServiceHost DispatchMessageInspector.
    /// </summary>
    [Serializable]
    public class WcfServiceHostDispatchMessageInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Field _oneWayActions.
        /// </summary>
        private readonly HashSet<string> _oneWayActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostDispatchMessageInspector"/> class.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint.</param>
        public WcfServiceHostDispatchMessageInspector(ServiceEndpoint serviceEndpoint)
        {
            this._oneWayActions = new HashSet<string>();

            foreach (var operation in serviceEndpoint.Contract.Operations)
            {
                if (operation.IsOneWay)
                {
                    this._oneWayActions.Add(operation.Messages[0].Action);
                }
            }
        }

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

            bool isOneWay = false;

            if (request != null)
            {
                message = request.ToString();

                isOneWay = this._oneWayActions.Contains(request.Headers.Action);
            }
            else
            {
                message = string.Empty;
            }

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostDispatchMessageInspector.AfterReceiveRequest: " + messageId.ToString());
            Debug.WriteLine(message);

            this.RaiseEvent(this.ReceivingRequest, WcfServiceHostState.Receiving, request, message, messageId, isOneWay);

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

            bool? isOneWay = false;

            if (reply != null)
            {
                message = reply.ToString();

                if (reply.IsFault)
                {
                    isOneWay = null;
                }
            }
            else
            {
                message = string.Empty;

                isOneWay = true;
            }

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostDispatchMessageInspector.BeforeSendReply: " + messageId.ToString());
            Debug.WriteLine(message);

            this.RaiseEvent(this.SendingReply, WcfServiceHostState.Replying, reply, message, messageId, isOneWay);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="state">The state.</param>
        /// <param name="channelMessage">The channel message.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, WcfServiceHostState state, Message channelMessage, string message, Guid messageId, bool? isOneWay)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, new WcfServiceHostEventArgs(null, state, null, channelMessage, message, messageId, isOneWay));
            }
        }
    }
}
