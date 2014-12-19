//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    /// <summary>
    /// Represents service client proxy object.
    /// </summary>
    [Serializable]
    public abstract class DynamicClientProxyBase : MarshalByRefObject, IWcfClientBase
    {
        /// <summary>
        /// Field ClientCredentialsPropertyName.
        /// </summary>
        private const string ClientCredentialsPropertyName = "ClientCredentials";

        /// <summary>
        /// Field EndpointPropertyName.
        /// </summary>
        private const string EndpointPropertyName = "Endpoint";

        /// <summary>
        /// Field InnerChannelPropertyName.
        /// </summary>
        private const string InnerChannelPropertyName = "InnerChannel";

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientProxyBase(Type proxyType, Binding binding, string remoteUri)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this.ProxyType = proxyType;

            Type[] paramTypes = new Type[2];
            paramTypes[0] = typeof(Binding);
            paramTypes[1] = typeof(EndpointAddress);

            object[] paramValues = new object[2];
            paramValues[0] = binding;
            paramValues[1] = new EndpointAddress(remoteUri);

            this.ParamTypes = paramTypes;
            this.ParamValues = paramValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="bindingType">Type of the binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientProxyBase(Type proxyType, Type bindingType, string remoteUri)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            this.ProxyType = proxyType;

            Type[] paramTypes = new Type[2];
            paramTypes[0] = typeof(Binding);
            paramTypes[1] = typeof(EndpointAddress);

            object[] paramValues = new object[2];
            paramValues[0] = WcfServiceType.GetBinding(bindingType);
            paramValues[1] = new EndpointAddress(remoteUri);

            this.ParamTypes = paramTypes;
            this.ParamValues = paramValues;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        ~DynamicClientProxyBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs before send request.
        /// </summary>
        public event EventHandler<WcfClientBaseEventArgs> SendingRequest;

        /// <summary>
        /// Occurs after receive reply.
        /// </summary>
        public event EventHandler<WcfClientBaseEventArgs> ReceivingReply;

        /// <summary>
        /// Gets the type of current service client proxy.
        /// </summary>
        public Type ProxyType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the instance of current service client proxy.
        /// </summary>
        public object ProxyInstance
        {
            get
            {
                this.RefreshCachedProxy();
                return this.CachedProxy;
            }
        }

        /// <summary>
        /// Gets all the public methods of the current service client proxy.
        /// </summary>
        public List<MethodInfo> Methods
        {
            get
            {
                List<MethodInfo> result = new List<MethodInfo>();

                foreach (var item in this.ProxyType.GetMethods())
                {
                    if (item.DeclaringType == this.ProxyType)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }
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
        /// Gets or sets a delegate to configure ClientCredentials.
        /// </summary>
        public Action<DataContractSerializerOperationBehavior> SetDataContractResolverAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the client credentials used to call an operation.
        /// </summary>
        public ClientCredentials ClientCredentials
        {
            get
            {
                return (ClientCredentials)this.GetProperty(ClientCredentialsPropertyName);
            }
        }

        /// <summary>
        /// Gets the endpoint for the client.
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get
            {
                return (ServiceEndpoint)this.GetProperty(EndpointPropertyName);
            }
        }

        /// <summary>
        /// Gets or sets BindingFlags for invoke attribute.
        /// </summary>
        public BindingFlags InvokeAttr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current endpoint for the service to which the client connected.
        /// </summary>
        public ServiceEndpoint CurrentEndpoint
        {
            get
            {
                if (this._disposed || this.CachedProxy == null)
                {
                    return null;
                }
                else
                {
                    return (ServiceEndpoint)this.GetProperty(this.CachedProxy, EndpointPropertyName);
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        public IClientChannel InnerChannel
        {
            get
            {
                return (IClientChannel)this.GetProperty(this.ProxyInstance, InnerChannelPropertyName);
            }
        }

        /// <summary>
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase`1" /> object.
        /// </summary>
        public CommunicationState State
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
        /// Gets or sets parameter types for constructor.
        /// </summary>
        protected Type[] ParamTypes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets parameter values for constructor.
        /// </summary>
        protected object[] ParamValues
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the cached proxy.
        /// </summary>
        protected object CachedProxy
        {
            get;
            set;
        }

        /// <summary>
        /// Invokes Constructor of the object.
        /// </summary>
        /// <returns>New instance.</returns>
        public object CallConstructor()
        {
            return this.CallConstructor(Type.EmptyTypes, new object[0]);
        }

        /// <summary>
        /// Invokes Constructor of the object.
        /// </summary>
        /// <param name="paramTypes">Constructor parameter types.</param>
        /// <param name="paramValues">Constructor parameter values.</param>
        /// <returns>New instance.</returns>
        public object CallConstructor(Type[] paramTypes, object[] paramValues)
        {
            ConstructorInfo ctor = this.ProxyType.GetConstructor(paramTypes);

            if (ctor == null)
            {
                throw new ArgumentException(DynamicClientProxyConstants.ProxyCtorNotFound, "paramTypes");
            }

            return ctor.Invoke(paramValues);
        }

        /// <summary>
        /// Gets property value by name.
        /// </summary>
        /// <param name="propertyName">Property name to get.</param>
        /// <returns>Property value.</returns>
        public object GetProperty(string propertyName)
        {
            object result = this.ProxyType.InvokeMember(
                propertyName,
                BindingFlags.GetProperty | this.InvokeAttr,
                null /* Binder */,
                this.ProxyInstance,
                null /* args */);

            return result;
        }

        /// <summary>
        /// Sets property value by name.
        /// </summary>
        /// <param name="propertyName">Property name to set.</param>
        /// <param name="value">Property value.</param>
        /// <returns>Null value.</returns>
        public object SetProperty(string propertyName, object value)
        {
            object result = this.ProxyType.InvokeMember(
                propertyName,
                BindingFlags.SetProperty | this.InvokeAttr,
                null /* Binder */,
                this.ProxyInstance,
                new object[] { value });

            return result;
        }

        /// <summary>
        /// Gets field value by name.
        /// </summary>
        /// <param name="fieldName">Field name to get.</param>
        /// <returns>Field value.</returns>
        public object GetField(string fieldName)
        {
            object result = this.ProxyType.InvokeMember(
                fieldName,
                BindingFlags.GetField | this.InvokeAttr,
                null /* Binder */,
                this.ProxyInstance,
                null /* args */);

            return result;
        }

        /// <summary>
        /// Sets field value by name.
        /// </summary>
        /// <param name="fieldName">Field name to set.</param>
        /// <param name="value">Field value.</param>
        /// <returns>Null value.</returns>
        public object SetField(string fieldName, object value)
        {
            object result = this.ProxyType.InvokeMember(
                fieldName,
                BindingFlags.SetField | this.InvokeAttr,
                null /* Binder */,
                this.ProxyInstance,
                new object[] { value });

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object CallMethod(string methodName, params object[] parameters)
        {
            object result = this.ProxyType.InvokeMember(
                methodName,
                BindingFlags.InvokeMethod | this.InvokeAttr,
                null /* Binder */,
                this.ProxyInstance,
                parameters /* args */);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object CallMethod(string methodName, Type[] types, object[] parameters)
        {
            if (types.Length != parameters.Length)
            {
                throw new ArgumentException(DynamicClientProxyConstants.ParameterValueMismatch);
            }

            MethodInfo methodInfo = this.ProxyType.GetMethod(methodName, types);

            if (methodInfo == null)
            {
                throw new ArgumentException(string.Format(DynamicClientProxyConstants.MethodNotFoundStringFormat, methodName), "methodName");
            }

            object result = methodInfo.Invoke(this.ProxyInstance, this.InvokeAttr, null, parameters, null);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object CallMethod(MethodInfo methodInfo, params object[] parameters)
        {
            return methodInfo.Invoke(this.ProxyInstance, this.InvokeAttr, null, parameters, null);
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from the created state into the opened state.
        /// </summary>
        public void Open()
        {
            try
            {
                (this.ProxyInstance as ICommunicationObject).Open();
            }
            catch
            {
                this.CloseProxy();
                throw;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        public void Close()
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
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition immediately from its current state into the closed state.
        /// </summary>
        public void Abort()
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
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxyBase" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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
        /// Closes the proxy.
        /// </summary>
        protected void CloseProxy()
        {
            if (this.CachedProxy != null)
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
        /// Method RefreshCachedProxy.
        /// </summary>
        protected void RefreshCachedProxy()
        {
            this.CheckDisposed();

            if (this.CachedProxy == null)
            {
                this.CachedProxy = this.CreateProxyInstance();
            }
            else
            {
                this.ConfigureProxyInstance(this.CachedProxy);
            }
        }

        /// <summary>
        /// Creates the proxy instance.
        /// </summary>
        /// <returns>The proxy instance.</returns>
        protected object CreateProxyInstance()
        {
            object result = this.CallConstructor(this.ParamTypes, this.ParamValues);

            ServiceEndpoint endpoint = (ServiceEndpoint)this.GetProperty(result, EndpointPropertyName);

            WcfClientBaseEndpointBehavior wcfClientBaseEndpointBehavior = endpoint.Behaviors.Find<WcfClientBaseEndpointBehavior>();

            if (wcfClientBaseEndpointBehavior == null)
            {
                wcfClientBaseEndpointBehavior = new WcfClientBaseEndpointBehavior();
                wcfClientBaseEndpointBehavior.SendingRequest += (s, e) => this.RaiseEvent(this.SendingRequest, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);
                wcfClientBaseEndpointBehavior.ReceivingReply += (s, e) => this.RaiseEvent(this.ReceivingReply, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);

                endpoint.Behaviors.Add(wcfClientBaseEndpointBehavior);
            }
            else
            {
                wcfClientBaseEndpointBehavior.SendingRequest -= (s, e) => this.RaiseEvent(this.SendingRequest, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);
                wcfClientBaseEndpointBehavior.ReceivingReply -= (s, e) => this.RaiseEvent(this.ReceivingReply, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);

                wcfClientBaseEndpointBehavior.SendingRequest += (s, e) => this.RaiseEvent(this.SendingRequest, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);
                wcfClientBaseEndpointBehavior.ReceivingReply += (s, e) => this.RaiseEvent(this.ReceivingReply, endpoint.Name, endpoint.Address, endpoint.ListenUri, e);
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
            }

            this.ConfigureProxyInstance(result);

            return result;
        }

        /// <summary>
        /// Configures the proxy instance.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        private void ConfigureProxyInstance(object proxy)
        {
            if (this.SetClientCredentialsAction != null)
            {
                this.SetClientCredentialsAction((ClientCredentials)this.GetProperty(proxy, ClientCredentialsPropertyName));
            }

            ServiceEndpoint endpoint = (ServiceEndpoint)this.GetProperty(proxy, EndpointPropertyName);

            if (this.SetDataContractResolverAction != null)
            {
                foreach (OperationDescription operationDescription in endpoint.Contract.Operations)
                {
                    DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                    if (serializerBehavior == null)
                    {
                        serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription);
                        operationDescription.Behaviors.Add(serializerBehavior);
                    }

                    this.SetDataContractResolverAction(serializerBehavior);
                }
            }
        }

        /// <summary>
        /// Gets property value by name.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Property name to get.</param>
        /// <returns>Property value.</returns>
        private object GetProperty(object source, string propertyName)
        {
            object result = this.ProxyType.InvokeMember(
                propertyName,
                BindingFlags.GetProperty | this.InvokeAttr,
                null /* Binder */,
                source,
                null /* args */);

            return result;
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="name">The name.</param>
        /// <param name="address">The address.</param>
        /// <param name="listenUri">The listen URI.</param>
        /// <param name="e">The <see cref="WcfClientBaseEventArgs"/> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfClientBaseEventArgs> eventHandler, string name, EndpointAddress address, Uri listenUri, WcfClientBaseEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfClientBaseEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfClientBaseEventArgs(name, address, listenUri, e.ChannelMessage, e.Message, e.MessageId));
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceModel.DynamicClientProxyBase");
            }
        }
    }
}
