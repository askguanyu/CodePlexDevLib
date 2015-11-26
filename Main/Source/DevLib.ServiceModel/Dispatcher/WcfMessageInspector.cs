﻿//-----------------------------------------------------------------------
// <copyright file="WcfMessageInspector.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel.Dispatcher
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
    /// WcfClient MessageInspector.
    /// </summary>
    [Serializable]
    public class WcfMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        /// <summary>
        /// Field _serviceEndpoint.
        /// </summary>
        [NonSerialized]
        private readonly ServiceEndpoint _serviceEndpoint;

        /// <summary>
        /// Field _clientCredentials.
        /// </summary>
        [NonSerialized]
        private readonly ClientCredentials _clientCredentials;

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
        /// Initializes a new instance of the <see cref="WcfMessageInspector" /> class.
        /// </summary>
        public WcfMessageInspector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspector" /> class.
        /// This new instance will simulate ReceivingReply event for OneWay action.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        public WcfMessageInspector(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
        {
            this._serviceEndpoint = serviceEndpoint;
            this._clientCredentials = clientCredentials;

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
        /// Initializes a new instance of the <see cref="WcfMessageInspector" /> class.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        public WcfMessageInspector(ServiceEndpoint serviceEndpoint, ServiceHostBase serviceHostBase)
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
        /// Occurs before send request.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> ReceivingReply;

        /// <summary>
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> SendingReply;

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            Guid messageId = Guid.Empty;

            try
            {
                messageId = (Guid)correlationState;
            }
            catch
            {
            }

            Debug.WriteLine("DevLib.ServiceModel.Dispatcher.WcfMessageInspector.AfterReceiveReply: " + messageId.ToString());

            if (reply != null)
            {
                Debug.WriteLine(reply.ToString());
            }

            this.RaiseEvent(this.ReceivingReply, reply, messageId, false);
        }

        /// <summary>
        /// Enables inspection or modification of a message before a request message is sent to a service.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The WCF client object channel.</param>
        /// <returns>The object that is returned as the correlationState argument of the <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)" /> method. This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid" /> to ensure that no two correlationState objects are the same.</returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            Guid messageId = Guid.NewGuid();

            bool isOneWay = false;

            Debug.WriteLine("DevLib.ServiceModel.Dispatcher.WcfMessageInspector.BeforeSendRequest: " + messageId.ToString());

            if (request != null)
            {
                if (this._oneWayActions != null)
                {
                    isOneWay = this._oneWayActions.Contains(request.Headers.Action);
                }

                Debug.WriteLine(request.ToString());
            }

            this.RaiseEvent(this.SendingRequest, request, messageId, isOneWay);

            if (isOneWay)
            {
                Debug.WriteLine("DevLib.ServiceModel.Dispatcher.WcfMessageInspector.AfterReceiveReply(simulate reply for OneWay): " + messageId.ToString());

                this.RaiseEvent(this.ReceivingReply, request, messageId, isOneWay);
            }

            return messageId;
        }

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

            Debug.WriteLine("DevLib.ServiceModel.Dispatcher.WcfMessageInspector.AfterReceiveRequest: " + messageId.ToString());

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

            Debug.WriteLine("DevLib.ServiceModel.Dispatcher.WcfMessageInspector.BeforeSendReply: " + messageId.ToString());

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
        private void RaiseEvent(EventHandler<WcfMessageInspectorEventArgs> eventHandler, Message message, Guid messageId, bool isOneWay)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfMessageInspectorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfMessageInspectorEventArgs(message, messageId, isOneWay, this._serviceEndpoint, this._clientCredentials, this._serviceHostBase));
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
