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
        public event EventHandler<WcfServiceHostEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> SendingReply;

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
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;

                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        WcfServiceHostDispatchMessageInspector inspector = new WcfServiceHostDispatchMessageInspector();
                        inspector.ReceivingRequest += (s, e) => this.RaiseEvent(this.ReceivingRequest, e);
                        inspector.SendingReply += (s, e) => this.RaiseEvent(this.SendingReply, e);

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
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="e">The <see cref="WcfServiceHostEventArgs"/> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, WcfServiceHostEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, e);
            }
        }
    }
}
