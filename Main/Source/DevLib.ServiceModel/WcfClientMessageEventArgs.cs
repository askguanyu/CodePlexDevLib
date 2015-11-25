//-----------------------------------------------------------------------
// <copyright file="WcfClientMessageEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
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
        /// <param name="clientBase">The client base.</param>
        public WcfClientMessageEventArgs(Message message, Guid messageId, bool isOneWay, ServiceEndpoint endpoint, ClientBase clientBase)
            : base(message, messageId, isOneWay, endpoint)
        {
            this.Client = clientBase;
        }

        /// <summary>
        /// Gets the client credentials used to call an operation.
        /// </summary>
        public ClientBase Client
        {
            get;
            private set;
        }
    }
}
