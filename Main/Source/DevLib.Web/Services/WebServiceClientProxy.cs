//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        /// Field _methods.
        /// </summary>
        private ReadOnlyCollection<MethodInfo> _methods;

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
        public ReadOnlyCollection<MethodInfo> Methods
        {
            get
            {
                if (this._methods == null)
                {
                    List<MethodInfo> result = new List<MethodInfo>();

                    foreach (MethodInfo item in this.ObjectType.GetMethods())
                    {
                        if (item.DeclaringType == this.ProxyType)
                        {
                            result.Add(item);
                        }
                    }

                    this._methods = result.AsReadOnly();
                }

                return this._methods;
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(MethodInfo methodInfo, params object[] parameters)
        {
            try
            {
                return base.Call(methodInfo, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(string methodName, params object[] parameters)
        {
            try
            {
                return base.Call(methodName, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(string methodName, Type[] types, object[] parameters)
        {
            try
            {
                return base.Call(methodName, types, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
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

                this.Call("Dispose");
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
