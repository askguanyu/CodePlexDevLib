//-----------------------------------------------------------------------
// <copyright file="ClientBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// Provides the base implementation client objects that.
    /// </summary>
    [Serializable]
    public class ClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientBase"/> class.
        /// </summary>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="innerChannel">The inner channel.</param>
        /// <param name="state">The state.</param>
        public ClientBase(ClientCredentials clientCredentials, ServiceEndpoint endpoint, IClientChannel innerChannel, CommunicationState state)
        {
            this.ClientCredentials = clientCredentials;
            this.Endpoint = endpoint;
            this.InnerChannel = innerChannel;
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
        /// Gets the target endpoint for the service to which the  client can connect.
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        public IClientChannel InnerChannel
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
