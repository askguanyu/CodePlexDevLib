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

    /// <summary>
    /// Class WcfClientBase.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public abstract class WcfClientBase<TChannel> : IWcfClientBase where TChannel : class
    {
        /// <summary>
        /// Field _InstanceType.
        /// </summary>
        private static readonly Type InstanceType = WcfClientType.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

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
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

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
        /// Gets the client credentials used to call an operation.
        /// </summary>
        public virtual ClientCredentials ClientCredentials
        {
            get
            {
                return (this.Proxy as ClientBase<TChannel>).ClientCredentials;
            }
        }

        /// <summary>
        /// Gets the current endpoint for the service to which the client connected.
        /// </summary>
        public virtual ServiceEndpoint CurrentEndpoint
        {
            get
            {
                if (this._disposed || this.CachedProxy == null)
                {
                    return null;
                }
                else
                {
                    return (this.CachedProxy as ClientBase<TChannel>).Endpoint;
                }
            }
        }

        /// <summary>
        /// Gets the target endpoint for the service to which the client can connect.
        /// </summary>
        public virtual ServiceEndpoint Endpoint
        {
            get
            {
                if (this._disposed)
                {
                    return null;
                }
                else
                {
                    return (this.Proxy as ClientBase<TChannel>).Endpoint;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        public virtual IClientChannel InnerChannel
        {
            get
            {
                return (this.Proxy as ClientBase<TChannel>).InnerChannel;
            }
        }

        /// <summary>
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase`1" /> object.
        /// </summary>
        public virtual CommunicationState State
        {
            get
            {
                if (this.CachedProxy != null)
                {
                    return (this.CachedProxy as ICommunicationObject).State;
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
        /// Gets or sets Proxy caching.
        /// </summary>
        protected TChannel CachedProxy
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
                this.RefreshCachedProxy();
                return this.CachedProxy;
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
                if (this.CachedProxy != null)
                {
                    (this.CachedProxy as ICommunicationObject).Abort();
                }
            }
            finally
            {
                this.CloseProxy();
            }
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from its current state into the closed state.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                if (this.CachedProxy != null)
                {
                    (this.CachedProxy as ICommunicationObject).Close();
                }
            }
            finally
            {
                this.CloseProxy();
            }
        }

        /// <summary>
        ///  Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from the created state into the opened state.
        /// </summary>
        public virtual void Open()
        {
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
            if (this.CachedProxy != null && this.CachedProxy != default(TChannel))
            {
                ICommunicationObject communicationObject = this.CachedProxy as ICommunicationObject;

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

                this.CachedProxy = null;
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
                ICommunicationObject communicationObject = this.CachedProxy as ICommunicationObject;

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
        }

        /// <summary>
        /// Method RefreshCachedProxy.
        /// </summary>
        protected void RefreshCachedProxy()
        {
            this.CheckDisposed();

            if (this.CachedProxy == null || this.CachedProxy == default(TChannel))
            {
                this.CachedProxy = this.CreateProxyInstance();
            }
        }

        /// <summary>
        /// Method CreateProxyInstance.
        /// </summary>
        /// <returns>Instance of TChannel.</returns>
        protected virtual TChannel CreateProxyInstance()
        {
            switch (this._overloadCreateProxyInstance)
            {
                case 0:
                    return (TChannel)Activator.CreateInstance(InstanceType);
                case 1:
                    return (TChannel)Activator.CreateInstance(InstanceType, this._endpointConfigurationName);
                case 2:
                    return (TChannel)Activator.CreateInstance(InstanceType, this._endpointConfigurationName, this._remoteAddress);
                case 3:
                    return (TChannel)Activator.CreateInstance(InstanceType, this._binding, this._remoteAddress);
                default:
                    return (TChannel)Activator.CreateInstance(InstanceType);
            }
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
