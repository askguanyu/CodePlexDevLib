//-----------------------------------------------------------------------
// <copyright file="WcfMessageInspectorEndpointBehavior.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// WcfMessageInspector EndpointBehavior.
    /// </summary>
    [Serializable]
    public class WcfMessageInspectorEndpointBehavior : Attribute, IEndpointBehavior
    {
        /// <summary>
        /// Field _serviceHostBase.
        /// </summary>
        [NonSerialized]
        private readonly ServiceHostBase _serviceHostBase;

        /// <summary>
        /// Field _clientCredentials.
        /// </summary>
        [NonSerialized]
        private readonly ClientCredentials _clientCredentials;

        /// <summary>
        /// Field _clientState.
        /// </summary>
        private readonly CommunicationState _clientState;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspectorEndpointBehavior"/> class.
        /// </summary>
        public WcfMessageInspectorEndpointBehavior()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspectorEndpointBehavior" /> class.
        /// </summary>
        /// <param name="serviceHostBase">The service host.</param>
        public WcfMessageInspectorEndpointBehavior(ServiceHostBase serviceHostBase)
        {
            this._serviceHostBase = serviceHostBase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspectorEndpointBehavior"/> class.
        /// </summary>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="clientState">State of the client.</param>
        public WcfMessageInspectorEndpointBehavior(ClientCredentials clientCredentials, CommunicationState clientState)
        {
            this._clientCredentials = clientCredentials;
            this._clientState = clientState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspectorEndpointBehavior"/> class.
        /// </summary>
        /// <param name="serviceHostBase">The service host base.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="clientState">State of the client.</param>
        public WcfMessageInspectorEndpointBehavior(ServiceHostBase serviceHostBase, ClientCredentials clientCredentials, CommunicationState clientState)
        {
            this._serviceHostBase = serviceHostBase;
            this._clientCredentials = clientCredentials;
            this._clientState = clientState;
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
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> SendingReply;

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
            WcfClientMessageInspector inspector = new WcfClientMessageInspector(endpoint, this._clientCredentials, this._clientState);

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
            WcfServiceHostMessageInspector inspector = new WcfServiceHostMessageInspector(endpoint, this._serviceHostBase);

            inspector.ReceivingRequest += (s, e) => this.RaiseEvent(this.ReceivingRequest, endpoint, e);
            inspector.SendingReply += (s, e) => this.RaiseEvent(this.SendingReply, endpoint, e);

            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
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
                temp(this, new WcfClientMessageEventArgs(e.Message, e.MessageId, e.IsOneWay, endpoint, this._clientCredentials, this._clientState));
            }
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="e">The <see cref="WcfServiceHostMessageEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostMessageEventArgs> eventHandler, ServiceEndpoint endpoint, WcfServiceHostMessageEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostMessageEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostMessageEventArgs(e.Message, e.MessageId, e.IsOneWay, endpoint, this._serviceHostBase));
            }
        }
    }
}
