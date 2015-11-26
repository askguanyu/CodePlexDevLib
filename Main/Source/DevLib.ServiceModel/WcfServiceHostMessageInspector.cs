//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostMessageInspector.cs" company="YuGuan Corporation">
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
    /// WcfServiceHost MessageInspector.
    /// </summary>
    [Serializable]
    public class WcfServiceHostMessageInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Field _serviceEndpoint.
        /// </summary>
        [NonSerialized]
        private readonly ServiceEndpoint _serviceEndpoint;

        /// <summary>
        /// Field _serviceHostBase.
        /// </summary>
        [NonSerialized]
        private readonly ServiceHostBase _serviceHostBase;

        /// <summary>
        /// Field _oneWayActions.
        /// </summary>
        private readonly HashSet<string> _oneWayActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostMessageInspector"/> class.
        /// </summary>
        public WcfServiceHostMessageInspector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostMessageInspector" /> class.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        public WcfServiceHostMessageInspector(ServiceEndpoint serviceEndpoint, ServiceHostBase serviceHostBase)
        {
            this._serviceEndpoint = serviceEndpoint;
            this._serviceHostBase = serviceHostBase;

            if (this._serviceEndpoint != null)
            {
                this._oneWayActions = new HashSet<string>();

                try
                {
                    foreach (var operation in this._serviceEndpoint.Contract.Operations)
                    {
                        if (operation.IsOneWay)
                        {
                            this._oneWayActions.Add(operation.Messages[0].Action);
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> SendingReply;

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

            bool isOneWay = false;

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostMessageInspector.AfterReceiveRequest: " + messageId.ToString());

            if (request != null)
            {
                if (this._oneWayActions != null)
                {
                    isOneWay = this._oneWayActions.Contains(request.Headers.Action);
                }

                Debug.WriteLine(request.ToString());
            }

            this.RaiseEvent(this.ReceivingRequest, request, messageId, isOneWay);

            return new CorrelationState(messageId, isOneWay);
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="correlationState">State of the correlation.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            CorrelationState state = correlationState as CorrelationState;

            Guid messageId = Guid.Empty;

            bool isOneWay = false;

            if (state != null)
            {
                messageId = state.MessageId;
                isOneWay = state.IsOneWay;
            }

            Debug.WriteLine("DevLib.ServiceModel.WcfServiceHostMessageInspector.BeforeSendReply: " + messageId.ToString());

            if (reply != null)
            {
                Debug.WriteLine(reply.ToString());
            }
            else
            {
                isOneWay = true;
            }

            this.RaiseEvent(this.SendingReply, reply, messageId, isOneWay);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostMessageEventArgs> eventHandler, Message message, Guid messageId, bool isOneWay)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostMessageEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostMessageEventArgs(message, messageId, isOneWay, this._serviceEndpoint, null));
            }
        }

        /// <summary>
        /// State of the correlation.
        /// </summary>
        private class CorrelationState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CorrelationState"/> class.
            /// </summary>
            /// <param name="messageId">The message identifier.</param>
            /// <param name="isOneWay">Whether the message is one way.</param>
            public CorrelationState(Guid messageId, bool isOneWay)
            {
                this.MessageId = messageId;
                this.IsOneWay = isOneWay;
            }

            /// <summary>
            /// Gets the message identifier.
            /// </summary>
            public Guid MessageId
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether the message is one way.
            /// </summary>
            public bool IsOneWay
            {
                get;
                private set;
            }
        }
    }
}
