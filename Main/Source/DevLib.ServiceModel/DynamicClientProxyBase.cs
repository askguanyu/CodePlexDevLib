//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
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
        private object _proxy;

        /// <summary>
        /// Field _methods.
        /// </summary>
        private ReadOnlyCollection<MethodInfo> _methods;

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
            paramValues[0] = WcfBinding.GetBinding(bindingType);
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
        public object Proxy
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
        /// Gets all the public methods of the current service client proxy.
        /// </summary>
        public ReadOnlyCollection<MethodInfo> Methods
        {
            get
            {
                if (this._methods == null)
                {
                    this._methods = new ReadOnlyCollection<MethodInfo>(
                        this
                        .ProxyType
                        .GetMethods()
                        .Where(item => item.DeclaringType == this.ProxyType)
                        .ToArray());
                }

                return this._methods;
            }
        }

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
        /// Gets the underlying <see cref="T:System.ServiceModel.IClientChannel" /> implementation.
        /// </summary>
        public IClientChannel InnerChannel
        {
            get
            {
                return (IClientChannel)this.GetProperty(InnerChannelPropertyName);
            }
        }

        /// <summary>
        /// Gets the current state of the <see cref="T:System.ServiceModel.ClientBase`1" /> object.
        /// </summary>
        public CommunicationState State
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
        /// Gets or sets BindingFlags for invoke attribute.
        /// </summary>
        public BindingFlags InvokeAttr
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
            this.CheckDisposed();

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
            this.CheckDisposed();

            object result = null;

            if (this._proxy != null)
            {
                result = this.ProxyType.InvokeMember(
                    propertyName,
                    BindingFlags.GetProperty | this.InvokeAttr,
                    null /* Binder */,
                    this._proxy,
                    null /* args */);
            }

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
            this.CheckDisposed();

            object result = null;

            if (this._proxy != null)
            {
                result = this.ProxyType.InvokeMember(
                    propertyName,
                    BindingFlags.SetProperty | this.InvokeAttr,
                    null /* Binder */,
                    this._proxy,
                    new object[] { value });
            }

            return result;
        }

        /// <summary>
        /// Gets field value by name.
        /// </summary>
        /// <param name="fieldName">Field name to get.</param>
        /// <returns>Field value.</returns>
        public object GetField(string fieldName)
        {
            this.CheckDisposed();

            object result = null;

            if (this._proxy != null)
            {
                result = this.ProxyType.InvokeMember(
                    fieldName,
                    BindingFlags.GetField | this.InvokeAttr,
                    null /* Binder */,
                    this._proxy,
                    null /* args */);
            }

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
            this.CheckDisposed();

            object result = null;

            if (this._proxy != null)
            {
                result = this.ProxyType.InvokeMember(
                    fieldName,
                    BindingFlags.SetField | this.InvokeAttr,
                    null /* Binder */,
                    this._proxy,
                    new object[] { value });
            }

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object Call(string methodName, params object[] parameters)
        {
            return this.CallMethod(this._proxy, methodName, parameters);
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object Call(string methodName, Type[] types, object[] parameters)
        {
            return this.CallMethod(this._proxy, methodName, types, parameters);
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public virtual object Call(MethodInfo methodInfo, params object[] parameters)
        {
            return this.CallMethod(this._proxy, methodInfo, parameters);
        }

        /// <summary>
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition from the created state into the opened state.
        /// </summary>
        public void Open()
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
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        public void Close()
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
        /// Causes the <see cref="T:System.ServiceModel.ClientBase`1" /> object to transition immediately from its current state into the closed state.
        /// </summary>
        public void Abort()
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
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxyBase" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        protected object CallMethod(object proxy, string methodName, params object[] parameters)
        {
            object result = this.ProxyType.InvokeMember(
                methodName,
                BindingFlags.InvokeMethod | this.InvokeAttr,
                null /* Binder */,
                proxy,
                parameters /* args */);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        protected object CallMethod(object proxy, string methodName, Type[] types, object[] parameters)
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

            object result = methodInfo.Invoke(proxy, this.InvokeAttr, null, parameters, null);

            return result;
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        protected object CallMethod(object proxy, MethodInfo methodInfo, params object[] parameters)
        {
            return methodInfo.Invoke(proxy, this.InvokeAttr, null, parameters, null);
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
        /// Closes the proxy instance.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void CloseProxyInstance(object value)
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
        /// Creates the proxy instance.
        /// </summary>
        /// <returns>The proxy instance.</returns>
        protected object CreateProxyInstance()
        {
            this.CheckDisposed();

            object result = this.CallConstructor(this.ParamTypes, this.ParamValues);

            this.InitProxyInstance(result);

            return result;
        }

        /// <summary>
        /// Initializes the proxy instance.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        private void InitProxyInstance(object proxy)
        {
            if (this.SetClientCredentialsAction != null)
            {
                this.SetClientCredentialsAction(this.GetProperty<ClientCredentials>(proxy, ClientCredentialsPropertyName));
            }

            ServiceEndpoint endpoint = this.GetProperty<ServiceEndpoint>(proxy, EndpointPropertyName);

            if (this.SetBindingAction != null)
            {
                this.SetBindingAction(endpoint.Binding);
            }

            WcfMessageInspectorEndpointBehavior wcfMessageInspectorEndpointBehavior = endpoint.Behaviors.Find<WcfMessageInspectorEndpointBehavior>();

            if (wcfMessageInspectorEndpointBehavior == null)
            {
                ClientCredentials clientCredentials = this.GetProperty<ClientCredentials>(proxy, ClientCredentialsPropertyName);

                wcfMessageInspectorEndpointBehavior = new WcfMessageInspectorEndpointBehavior(clientCredentials);

                wcfMessageInspectorEndpointBehavior.IgnoreMessageInspect = this.IgnoreMessageInspect;
                wcfMessageInspectorEndpointBehavior.IgnoreMessageValidate = this.IgnoreMessageValidate;

                wcfMessageInspectorEndpointBehavior.SendingRequest += (s, e) => this.RaiseEvent(this.SendingRequest, proxy, endpoint, clientCredentials, e);
                wcfMessageInspectorEndpointBehavior.ReceivingReply += (s, e) => this.RaiseEvent(this.ReceivingReply, proxy, endpoint, clientCredentials, e);
                wcfMessageInspectorEndpointBehavior.ErrorOccurred += (s, e) => this.RaiseEvent(this.ErrorOccurred, proxy, e);

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
        /// Gets property value by name.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Property name to get.</param>
        /// <returns>Property value.</returns>
        private TResult GetProperty<TResult>(object source, string propertyName)
        {
            object result = this.ProxyType.InvokeMember(
                propertyName,
                BindingFlags.GetProperty | this.InvokeAttr,
                null /* Binder */,
                source,
                null /* args */);

            return (TResult)result;
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
