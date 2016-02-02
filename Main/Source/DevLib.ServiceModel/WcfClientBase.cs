//-----------------------------------------------------------------------
// <copyright file="WcfClientBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    /// <summary>
    /// Class WcfClientBase.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public abstract class WcfClientBase<TChannel> : IWcfClientBase where TChannel : class
    {
        /// <summary>
        /// Field _InstanceType.
        /// </summary>
        private static readonly Type InstanceType = WcfClientUtilities.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

        /// <summary>
        /// Field _endpointConfigurationName.
        /// </summary>
        private readonly string _endpointConfigurationName;

        /// <summary>
        /// Field _remoteAddress.
        /// </summary>
        private readonly EndpointAddress _remoteAddress;

        /// <summary>
        /// Field _binding.
        /// </summary>
        private readonly Binding _binding;

        /// <summary>
        /// Field _overloadCreateProxyInstance.
        /// </summary>
        private readonly int _overloadCreateProxyInstance;

        /// <summary>
        /// Field _proxySyncRoot.
        /// </summary>
        private readonly object _proxySyncRoot = new object();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _proxy.
        /// </summary>
        private TChannel _proxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        protected WcfClientBase()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this._overloadCreateProxyInstance = 0;
            this._endpointConfigurationName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        protected WcfClientBase(string endpointConfigurationName)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this._overloadCreateProxyInstance = 1;
            this._endpointConfigurationName = endpointConfigurationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        protected WcfClientBase(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this._overloadCreateProxyInstance = 2;
            this._endpointConfigurationName = endpointConfigurationName;
            this._remoteAddress = remoteAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        protected WcfClientBase(Binding binding, EndpointAddress remoteAddress)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this._overloadCreateProxyInstance = 3;
            this._endpointConfigurationName = string.Empty;
            this._binding = binding;
            this._remoteAddress = remoteAddress;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        ~WcfClientBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs before send request.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> ReceivingReply;

        /// <summary>
        /// Occurs when has error.
        /// </summary>
        public event EventHandler<WcfErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets or sets a delegate to configure Binding.
        /// </summary>
        public Action<Binding> SetBindingAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a delegate to configure ClientCredentials.
        /// </summary>
        public Action<ClientCredentials> SetClientCredentialsAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a delegate to configure DataContractSerializerOperationBehavior.
        /// </summary>
        public Action<DataContractSerializerOperationBehavior> SetDataContractResolverAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether ignore message inspection.
        /// </summary>
        public bool IgnoreMessageInspect
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether ignore message validation.
        /// </summary>
        public bool IgnoreMessageValidate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the client credentials used to call an operation.
        /// </summary>
        public virtual ClientCredentials ClientCredentials
        {
            get
            {
                this.CheckDisposed();

                if (this._proxy == null)
                {
                    return (this._proxy as ClientBase<TChannel>).ClientCredentials;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the target endpoint for the service to which the client can connect.
        /// </summary>
        public virtual ServiceEndpoint Endpoint
        {
            get
            {
                this.CheckDisposed();

                if (this._proxy == null)
                {
                    return (this._proxy as ClientBase<TChannel>).Endpoint;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        public virtual IClientChannel InnerChannel
        {
            get
            {
                this.CheckDisposed();

                if (this._proxy == null)
                {
                    return (this._proxy as ClientBase<TChannel>).InnerChannel;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase`1" /> object.
        /// </summary>
        public virtual CommunicationState State
        {
            get
            {
                if (this._proxy != null)
                {
                    return (this._proxy as ICommunicationObject).State;
                }
                else
                {
                    return CommunicationState.Closed;
                }
            }
        }

        /// <summary>
        /// Gets or sets user defined tag on the proxy.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        protected TChannel Proxy
        {
            get
            {
                this.CheckDisposed();

                if (this._proxy == null)
                {
                    lock (this._proxySyncRoot)
                    {
                        if (this._proxy == null)
                        {
                            this._proxy = this.CreateProxyInstance();
                        }
                    }
                }

                return this._proxy;
            }
        }

        /// <summary>
        /// Gets EndpointConfigurationName.
        /// </summary>
        protected string EndpointConfigurationName
        {
            get
            {
                return this._endpointConfigurationName;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition immediately from its current state into the closed state.
        /// </summary>
        public virtual void Abort()
        {
            try
            {
                if (this._proxy != null)
                {
                    (this._proxy as ICommunicationObject).Abort();
                }
            }
            finally
            {
                this._proxy = null;
            }
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from its current state into the closed state.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                if (this._proxy != null)
                {
                    (this._proxy as ICommunicationObject).Close();
                }
            }
            finally
            {
                this._proxy = null;
            }
        }

        /// <summary>
        ///  Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from the created state into the opened state.
        /// </summary>
        public virtual void Open()
        {
            this.CheckDisposed();

            try
            {
                (this.Proxy as ICommunicationObject).Open();
            }
            catch
            {
                this.CloseProxy();
                throw;
            }
        }

        /// <summary>
        /// Close proxy.
        /// </summary>
        protected void CloseProxy()
        {
            if (this._proxy != null)
            {
                ICommunicationObject communicationObject = this._proxy as ICommunicationObject;

                if (communicationObject != null)
                {
                    try
                    {
                        communicationObject.Close();
                    }
                    catch
                    {
                        communicationObject.Abort();
                    }
                }

                this._proxy = null;
            }
        }

        /// <summary>
        /// Close proxy instance.
        /// </summary>
        /// <param name="value">TChannel instance.</param>
        protected void CloseProxyInstance(TChannel value)
        {
            if (value != null)
            {
                ICommunicationObject communicationObject = value as ICommunicationObject;

                if (communicationObject != null)
                {
                    try
                    {
                        communicationObject.Close();
                    }
                    catch
                    {
                        communicationObject.Abort();
                    }
                }

                value = null;
            }

            this.CloseProxy();
        }

        /// <summary>
        /// Method CreateProxyInstance.
        /// </summary>
        /// <returns>Instance of TChannel.</returns>
        protected virtual TChannel CreateProxyInstance()
        {
            this.CheckDisposed();

            TChannel result = null;

            switch (this._overloadCreateProxyInstance)
            {
                case 0:
                    result = (TChannel)Activator.CreateInstance(InstanceType);
                    break;

                case 1:
                    result = (TChannel)Activator.CreateInstance(InstanceType, this._endpointConfigurationName);
                    break;

                case 2:
                    result = (TChannel)Activator.CreateInstance(InstanceType, this._endpointConfigurationName, this._remoteAddress);
                    break;

                case 3:
                    result = (TChannel)Activator.CreateInstance(InstanceType, this._binding, this._remoteAddress);
                    break;

                default:
                    result = (TChannel)Activator.CreateInstance(InstanceType);
                    break;
            }

            this.InitProxyInstance(result as ClientBase<TChannel>);

            return result;
        }

        /// <summary>
        /// Method HandleFaultException.
        /// </summary>
        /// <typeparam name="TDetail">The serializable error detail type.</typeparam>
        /// <param name="faultException">Instance of FaultException{TDetail}.</param>
        protected virtual void HandleFaultException<TDetail>(FaultException<TDetail> faultException)
        {
            Exception exception = faultException.Detail as Exception;

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Initializes the proxy instance.
        /// </summary>
        /// <param name="clientBase">The clientBase.</param>
        private void InitProxyInstance(ClientBase<TChannel> clientBase)
        {
            if (this.SetClientCredentialsAction != null)
            {
                this.SetClientCredentialsAction(clientBase.ClientCredentials);
            }

            ServiceEndpoint endpoint = clientBase.Endpoint;

            if (this.SetBindingAction != null)
            {
                this.SetBindingAction(endpoint.Binding);
            }

            WcfMessageInspectorEndpointBehavior wcfMessageInspectorEndpointBehavior = endpoint.Behaviors.Find<WcfMessageInspectorEndpointBehavior>();

            if (wcfMessageInspectorEndpointBehavior == null)
            {
                wcfMessageInspectorEndpointBehavior = new WcfMessageInspectorEndpointBehavior(clientBase.ClientCredentials);

                wcfMessageInspectorEndpointBehavior.IgnoreMessageInspect = this.IgnoreMessageInspect;
                wcfMessageInspectorEndpointBehavior.IgnoreMessageValidate = this.IgnoreMessageValidate;

                wcfMessageInspectorEndpointBehavior.SendingRequest += (s, e) => this.RaiseEvent(this.SendingRequest, clientBase, endpoint, clientBase.ClientCredentials, e);
                wcfMessageInspectorEndpointBehavior.ReceivingReply += (s, e) => this.RaiseEvent(this.ReceivingReply, clientBase, endpoint, clientBase.ClientCredentials, e);
                wcfMessageInspectorEndpointBehavior.ErrorOccurred += (s, e) => this.RaiseEvent(this.ErrorOccurred, clientBase, e);

                endpoint.Behaviors.Add(wcfMessageInspectorEndpointBehavior);
            }

            foreach (OperationDescription operationDescription in endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                if (serializerBehavior == null)
                {
                    serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription);
                    serializerBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    serializerBehavior.IgnoreExtensionDataObject = true;

                    operationDescription.Behaviors.Add(serializerBehavior);
                }
                else
                {
                    serializerBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    serializerBehavior.IgnoreExtensionDataObject = true;
                }

                if (this.SetDataContractResolverAction != null)
                {
                    this.SetDataContractResolverAction(serializerBehavior);
                }
            }
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientCredentials">The client credentials.</param>
        /// <param name="e">The <see cref="WcfMessageInspectorEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfMessageInspectorEventArgs> eventHandler, object sender, ServiceEndpoint endpoint, ClientCredentials clientCredentials, WcfMessageInspectorEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfMessageInspectorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(sender, new WcfMessageInspectorEventArgs(e.Message, e.MessageId, e.IsOneWay, e.ValidationError, endpoint, clientCredentials, null));
            }
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="WcfErrorEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfErrorEventArgs> eventHandler, object sender, WcfErrorEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfErrorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(sender, e);
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfClientBase{TChannel}" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                this.CloseProxy();
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceModel.WcfClientBase");
            }
        }
    }
}
