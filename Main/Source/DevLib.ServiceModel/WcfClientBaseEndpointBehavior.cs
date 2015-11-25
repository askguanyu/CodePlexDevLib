//-----------------------------------------------------------------------
// <copyright file="WcfClientBaseEndpointBehavior.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// WcfClientBase EndpointBehavior.
    /// </summary>
    [Serializable]
    public class WcfClientBaseEndpointBehavior : Attribute, IEndpointBehavior
    {
        /// <summary>
        /// Occurs before send request.
        /// </summary>
        public event EventHandler<WcfClientMessageEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        public event EventHandler<WcfClientMessageEventArgs> ReceivingReply;

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            WcfClientBaseClientMessageInspector inspector = new WcfClientBaseClientMessageInspector(endpoint);

            inspector.SendingRequest += (s, e) => this.RaiseEvent(this.SendingRequest, endpoint, e);
            inspector.ReceivingReply += (s, e) => this.RaiseEvent(this.ReceivingReply, endpoint, e);

            clientRuntime.MessageInspectors.Add(inspector);
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="e">The <see cref="WcfClientMessageEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfClientMessageEventArgs> eventHandler, ServiceEndpoint endpoint, WcfClientMessageEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfClientMessageEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfClientMessageEventArgs(e.Message, e.MessageId, endpoint, e.IsOneWay, null));
            }
        }
    }
}
