//-----------------------------------------------------------------------
// <copyright file="WcfClientBaseClientMessageInspector.cs" company="YuGuan Corporation">
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
    /// WcfClientBase ClientMessageInspector.
    /// </summary>
    [Serializable]
    public class WcfClientBaseClientMessageInspector : IClientMessageInspector
    {
        /// <summary>
        /// Field _serviceEndpoint.
        /// </summary>
        [NonSerialized]
        private readonly ServiceEndpoint _serviceEndpoint;

        /// <summary>
        /// Field _clientBase.
        /// </summary>
        private readonly ClientBase _clientBase;

        /// <summary>
        /// Field _oneWayActions.
        /// </summary>
        private readonly HashSet<string> _oneWayActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBaseClientMessageInspector"/> class.
        /// </summary>
        public WcfClientBaseClientMessageInspector()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBaseClientMessageInspector" /> class.
        /// This new instance will simulate ReceivingReply event for OneWay action.
        /// </summary>
        /// <param name="serviceEndpoint">The service endpoint.</param>
        /// <param name="clientBase">The client base.</param>
        public WcfClientBaseClientMessageInspector(ServiceEndpoint serviceEndpoint, ClientBase clientBase)
        {
            this._serviceEndpoint = serviceEndpoint;
            this._clientBase = clientBase;

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
        public event EventHandler<WcfClientMessageEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        public event EventHandler<WcfClientMessageEventArgs> ReceivingReply;

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

            Debug.WriteLine("DevLib.ServiceModel.WcfClientBaseClientMessageInspector.AfterReceiveReply: " + messageId.ToString());

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

            Debug.WriteLine("DevLib.ServiceModel.WcfClientBaseClientMessageInspector.BeforeSendRequest: " + messageId.ToString());

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
                Debug.WriteLine("DevLib.ServiceModel.WcfClientBaseClientMessageInspector.AfterReceiveReply(simulate reply for OneWay): " + messageId.ToString());

                this.RaiseEvent(this.ReceivingReply, request, messageId, isOneWay);
            }

            return messageId;
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        private void RaiseEvent(EventHandler<WcfClientMessageEventArgs> eventHandler, Message message, Guid messageId, bool isOneWay)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfClientMessageEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfClientMessageEventArgs(message, messageId, isOneWay, this._serviceEndpoint, this._clientBase));
            }
        }
    }
}
