//-----------------------------------------------------------------------
// <copyright file="WcfMessageInspectorEventArgs.cs" company="YuGuan Corporation">
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
    /// WcfMessageInspector EventArgs.
    /// </summary>
    [Serializable]
    public class WcfMessageInspectorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfMessageInspectorEventArgs" /> class.
        /// </summary>
        /// <param name="message">The message of the service endpoint.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        public WcfMessageInspectorEventArgs(Message message, Guid messageId, bool isOneWay, ServiceEndpoint endpoint, ClientCredentials clientCredentials, ServiceHostBase serviceHostBase)
        {
            this.Message = message;
            this.MessageId = messageId;
            this.Endpoint = endpoint;
            this.IsOneWay = isOneWay;
            this.ClientCredentials = clientCredentials;
            this.ServiceHost = serviceHostBase;
        }

        /// <summary>
        /// Gets the unit of communication between endpoints in a distributed environment.
        /// </summary>
        public Message Message
        {
            get;
            private set;
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

        /// <summary>
        /// Gets the target endpoint for the service to which the WCF client can connect.
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the client credentials used to call an operation.
        /// </summary>
        public ClientCredentials ClientCredentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service host.
        /// </summary>
        public ServiceHostBase ServiceHost
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("MessageId={0}, IsOneWay={1}, Message=\r\n{2}", this.MessageId.ToString(), this.IsOneWay.ToString(), this.Message != null ? this.Message.ToString() : string.Empty);
        }
    }
}
