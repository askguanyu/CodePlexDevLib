//-----------------------------------------------------------------------
// <copyright file="WcfClientBaseEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// WcfClientBase EventArgs.
    /// </summary>
    [Serializable]
    public class WcfClientBaseEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBaseEventArgs" /> class.
        /// </summary>
        /// <param name="name">The name of the service endpoint.</param>
        /// <param name="address">The endpoint address for the service endpoint.</param>
        /// <param name="listenUri">The URI at which the service endpoint listens.</param>
        /// <param name="channelMessage">The unit of communication between endpoints in a distributed environment.</param>
        /// <param name="message">The message of the service endpoint.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="isOneWay">Whether the message is one way.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        public WcfClientBaseEventArgs(string name, EndpointAddress address, Uri listenUri, Message channelMessage, string message, Guid? messageId, bool isOneWay, string username, string password)
        {
            this.Name = name;
            this.Address = address;
            this.ListenUri = listenUri;
            this.ChannelMessage = channelMessage;
            this.Message = message;
            this.MessageId = messageId ?? Guid.Empty;
            this.IsOneWay = isOneWay;
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        /// Gets the name of the service endpoint.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the endpoint address for the service endpoint.
        /// </summary>
        public EndpointAddress Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the URI at which the service endpoint listens.
        /// </summary>
        public Uri ListenUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unit of communication between endpoints in a distributed environment.
        /// </summary>
        public Message ChannelMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message of the service endpoint.
        /// </summary>
        public string Message
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
        /// Gets the user name.
        /// </summary>
        public string Username
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get;
            private set;
        }
    }
}
