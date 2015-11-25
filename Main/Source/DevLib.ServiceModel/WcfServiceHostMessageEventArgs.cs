//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostMessageEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    /// <summary>
    /// WcfServiceHostMessage EventArgs.
    /// </summary>
    [Serializable]
    public class WcfServiceHostMessageEventArgs : WcfMessageBaseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostMessageEventArgs" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        /// <param name="serviceHost">The service host.</param>
        public WcfServiceHostMessageEventArgs(Message message, Guid messageId, ServiceEndpoint endpoint, bool isOneWay, ServiceHostBase serviceHost)
            : base(message, messageId, endpoint, isOneWay)
        {
            this.ServiceHost = serviceHost;
        }

        /// <summary>
        /// Gets the service host.
        /// </summary>
        public ServiceHostBase ServiceHost
        {
            get;
            private set;
        }
    }
}
