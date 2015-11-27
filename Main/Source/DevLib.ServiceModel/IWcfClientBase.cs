//-----------------------------------------------------------------------
// <copyright file="IWcfClientBase.cs" company="YuGuan Corporation">
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
    /// Defines the interface for the inner ClientBase.
    /// </summary>
    public interface IWcfClientBase : IDisposable
    {
        /// <summary>
        /// Occurs before send request.
        /// </summary>
        event EventHandler<WcfMessageInspectorEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        event EventHandler<WcfMessageInspectorEventArgs> ReceivingReply;

        /// <summary>
        /// Occurs when has error.
        /// </summary>
        event EventHandler<WcfErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets or sets a delegate to configure Binding.
        /// </summary>
        Action<Binding> SetBindingAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a delegate to configure ClientCredentials.
        /// </summary>
        Action<ClientCredentials> SetClientCredentialsAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a delegate to configure DataContractSerializerOperationBehavior.
        /// </summary>
        Action<DataContractSerializerOperationBehavior> SetDataContractResolverAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="T:System.ServiceModel.Description.ClientCredentials" /> that represents the proof of identity presented by the client.
        /// </summary>
        ClientCredentials ClientCredentials
        {
            get;
        }

        /// <summary>
        /// Gets the target endpoint for the service to which the client can connect.
        /// </summary>
        ServiceEndpoint Endpoint
        {
            get;
        }

        /// <summary>
        /// Gets the current endpoint for the service to which the client connected.
        /// </summary>
        ServiceEndpoint CurrentEndpoint
        {
            get;
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        IClientChannel InnerChannel
        {
            get;
        }

        /// <summary>
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase`1" /> object.
        /// </summary>
        CommunicationState State
        {
            get;
        }

        /// <summary>
        /// Gets or sets user defined tag on the proxy.
        /// </summary>
        object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition immediately from its current state into the closed state.
        /// </summary>
        void Abort();

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from its current state into the closed state.
        /// </summary>
        void Close();

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from the created state into the opened state.
        /// </summary>
        void Open();
    }
}
