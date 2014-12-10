//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    /// <summary>
    /// Represents service client proxy object.
    /// </summary>
    [Serializable]
    public class DynamicClientProxy : DynamicClientObject, IDisposable
    {
        /// <summary>
        /// Field SetClientCredentialsActionPropertyName.
        /// </summary>
        private const string SetClientCredentialsActionPropertyName = "SetClientCredentialsAction";

        /// <summary>
        /// Field SetDataContractResolverActionPropertyName.
        /// </summary>
        private const string SetDataContractResolverActionPropertyName = "SetDataContractResolverAction";

        /// <summary>
        /// Field ClientCredentialsPropertyName.
        /// </summary>
        private const string ClientCredentialsPropertyName = "ClientCredentials";

        /// <summary>
        /// Field EndpointPropertyName.
        /// </summary>
        private const string EndpointPropertyName = "Endpoint";

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        /// <param name="binding">Service client binding.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        public DynamicClientProxy(Type proxyType, Binding binding, string remoteUri)
            : base(proxyType)
        {
            Type[] paramTypes = new Type[2];
            paramTypes[0] = typeof(Binding);
            paramTypes[1] = typeof(EndpointAddress);

            object[] paramValues = new object[2];
            paramValues[0] = binding;
            paramValues[1] = new EndpointAddress(remoteUri);

            this.CallConstructor(paramTypes, paramValues);

            this.IsClientBase = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        /// <param name="bindingType">Service client binding type.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        public DynamicClientProxy(Type proxyType, Type bindingType, string remoteUri)
            : base(proxyType)
        {
            Type[] paramTypes = new Type[2];
            paramTypes[0] = typeof(Binding);
            paramTypes[1] = typeof(EndpointAddress);

            object[] paramValues = new object[2];
            paramValues[0] = WcfServiceType.GetBinding(bindingType);
            paramValues[1] = new EndpointAddress(remoteUri);

            this.CallConstructor(paramTypes, paramValues);

            this.IsClientBase = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        /// <param name="obj">Service client proxy object.</param>
        internal DynamicClientProxy(object obj)
            : base(obj)
        {
            this.IsClientBase = false;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        ~DynamicClientProxy()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets a value indicating whether current proxy is created by GetClientBaseProxy or not.
        /// </summary>
        public bool IsClientBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type of current service client proxy.
        /// </summary>
        public Type ProxyType
        {
            get
            {
                return this.ObjectType;
            }
        }

        /// <summary>
        /// Gets the instance of current service client proxy.
        /// </summary>
        public object Proxy
        {
            get
            {
                return this.ObjectInstance;
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

                foreach (var item in this.ObjectType.GetMethods())
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
            get
            {
                if (this.IsClientBase)
                {
                    return null;
                }
                else
                {
                    return (Action<ClientCredentials>)this.GetProperty(SetClientCredentialsActionPropertyName);
                }
            }

            set
            {
                if (!this.IsClientBase)
                {
                    this.SetProperty(SetClientCredentialsActionPropertyName, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a delegate to configure ClientCredentials.
        /// </summary>
        public Action<DataContractSerializerOperationBehavior> SetDataContractResolverAction
        {
            get
            {
                if (this.IsClientBase)
                {
                    return null;
                }
                else
                {
                    return (Action<DataContractSerializerOperationBehavior>)this.GetProperty(SetDataContractResolverActionPropertyName);
                }
            }

            set
            {
                if (!this.IsClientBase)
                {
                    this.SetProperty(SetDataContractResolverActionPropertyName, value);
                }
            }
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
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DynamicClientProxy" /> class.
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

                try
                {
                    this.CallMethod("Abort");
                }
                catch
                {
                }
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
                throw new ObjectDisposedException("DevLib.ServiceModel.DynamicClientProxy");
            }
        }
    }
}
