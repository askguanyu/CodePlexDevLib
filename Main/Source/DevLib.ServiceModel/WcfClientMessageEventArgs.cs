//-----------------------------------------------------------------------
// <copyright file="WcfClientMessageEventArgs.cs" company="YuGuan Corporation">
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
    /// WcfClientMessage EventArgs.
    /// </summary>
    [Serializable]
    public class WcfClientMessageEventArgs : WcfMessageBaseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientMessageEventArgs" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="state">The communication state.</param>
        public WcfClientMessageEventArgs(Message message, Guid messageId, bool isOneWay, ServiceEndpoint endpoint, ClientCredentials clientCredentials, CommunicationState state)
            : base(message, messageId, isOneWay, endpoint)
        {
            this.ClientCredentials = clientCredentials;
            this.State = state;
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
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase{}" /> object.
        /// </summary>
        public CommunicationState State
        {
            get;
            private set;
        }
    }
}
