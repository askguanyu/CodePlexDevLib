//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostServiceBehavior.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    /// <summary>
    /// WcfServiceHost ServiceBehavior.
    /// </summary>
    [Serializable]
    public class WcfServiceHostServiceBehavior : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostMessageEventArgs> SendingReply;

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        ServiceEndpoint serviceEndpoint = serviceDescription.Endpoints.Find(endpointDispatcher.EndpointAddress.Uri);

                        WcfServiceHostDispatchMessageInspector inspector = new WcfServiceHostDispatchMessageInspector(serviceEndpoint, serviceHostBase);

                        inspector.ReceivingRequest += (s, e) => this.RaiseEvent(this.ReceivingRequest, serviceEndpoint, serviceHostBase, e);
                        inspector.SendingReply += (s, e) => this.RaiseEvent(this.SendingReply, serviceEndpoint, serviceHostBase, e);

                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
                    }
                }
            }
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="e">The <see cref="WcfServiceHostMessageEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostMessageEventArgs> eventHandler, ServiceEndpoint endpoint, ServiceHostBase serviceHost, WcfServiceHostMessageEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostMessageEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostMessageEventArgs(e.Message, e.MessageId, endpoint, e.IsOneWay, serviceHost));
            }
        }
    }
}
