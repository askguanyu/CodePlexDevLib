//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;

    /// <summary>
    /// Represents service client proxy object.
    /// </summary>
    [Serializable]
    public class WebServiceClientProxy : WebServiceClientObject, IDisposable
    {
        /// <summary>
        /// Field UrlPropertyName.
        /// </summary>
        private const string UrlPropertyName = "Url";

        /// <summary>
        /// Field CredentialsPropertyName.
        /// </summary>
        private const string CredentialsPropertyName = "Credentials";

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        public WebServiceClientProxy(Type proxyType)
            : base(proxyType)
        {
            this.CallConstructor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        /// <param name="url">The base URL of the Web service the client is requesting.</param>
        public WebServiceClientProxy(Type proxyType, string url)
            : base(proxyType)
        {
            this.CallConstructor();
            this.Url = url;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        ~WebServiceClientProxy()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the base URL of the Web service the client is requesting.
        /// </summary>
        public string Url
        {
            get
            {
                return (string)this.GetProperty(UrlPropertyName);
            }

            set
            {
                this.SetProperty(UrlPropertyName, value);
            }
        }

        /// <summary>
        /// Gets or sets security credentials for XML Web service client authentication.
        /// </summary>
        public ICredentials Credentials
        {
            get
            {
                return (ICredentials)this.GetProperty(CredentialsPropertyName);
            }

            set
            {
                this.SetProperty(CredentialsPropertyName, value);
            }
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
        /// Releases all resources used by the current instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WebServiceClientProxy" /> class.
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

                this.CallMethod("Dispose");
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
                throw new ObjectDisposedException("DevLib.Web.Services.WebServiceClientProxy");
            }
        }
    }
}
