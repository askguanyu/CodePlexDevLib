//-----------------------------------------------------------------------
// <copyright file="ExtensionPropertiesClass.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionProperties
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ExtensionProperties Class.
    /// </summary>
    /// <typeparam name="T">The type of the source to extend.</typeparam>
    public class ExtensionPropertiesClass<T> : IDisposable
    {
        /// <summary>
        /// Field _disposeAction.
        /// </summary>
        private readonly Action<T> _disposeAction;

        /// <summary>
        /// Field _extensionProperties.
        /// </summary>
        private readonly Dictionary<string, object> _extensionProperties;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionPropertiesClass{T}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="disposeAction">The dispose action.</param>
        public ExtensionPropertiesClass(T value, Action<T> disposeAction = null)
        {
            this.Value = value;
            this._disposeAction = disposeAction;
            this._extensionProperties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ExtensionPropertiesClass{T}"/> class.
        /// </summary>
        ~ExtensionPropertiesClass()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the source value.
        /// </summary>
        public T Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the property value with the specified property name.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        /// <returns>The specified property value.</returns>
        public object this[string propertyName]
        {
            get
            {
                object result = null;

                this.TryGetExtensionProperty(propertyName, out result);

                return result;
            }

            set
            {
                this.SetExtensionProperty(propertyName, value);
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ExtensionPropertiesClass{T}" /> to T.
        /// </summary>
        /// <param name="source">The extension properties class.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(ExtensionPropertiesClass<T> source)
        {
            return source.Value;
        }

        /// <summary>
        /// Performs an implicit conversion from T to <see cref="ExtensionPropertiesClass{T}" />.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator ExtensionPropertiesClass<T>(T source)
        {
            return new ExtensionPropertiesClass<T>(source);
        }

        /// <summary>
        /// Gets the extension property value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The specified property value.</returns>
        public object GetExtensionProperty(string propertyName)
        {
            this.CheckDisposed();

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (!this._extensionProperties.ContainsKey(propertyName))
            {
                throw new MissingMemberException(propertyName + " does not exist in the extension properties.");
            }

            return this._extensionProperties[propertyName];
        }

        /// <summary>
        /// Sets the extension property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        public void SetExtensionProperty(string propertyName, object value)
        {
            this.CheckDisposed();

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            this._extensionProperties[propertyName] = value;
        }

        /// <summary>
        /// Determines whether the specified property exist.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>true if the specified property exists; otherwise, false.</returns>
        public bool HasExtensionProperty(string propertyName)
        {
            this.CheckDisposed();

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            return this._extensionProperties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Gets all extension property names.
        /// </summary>
        /// <returns>A list of extension property names.</returns>
        public List<string> GetExtensionPropertyNames()
        {
            this.CheckDisposed();

            List<string> result = new List<string>();

            foreach (string key in this._extensionProperties.Keys)
            {
                result.Add(key);
            }

            return result;
        }

        /// <summary>
        /// Removes the extension property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>true if the property is successfully found and removed; otherwise, false. This method returns false if the property is not found.</returns>
        public bool RemoveExtensionProperty(string propertyName)
        {
            this.CheckDisposed();

            return this._extensionProperties.Remove(propertyName);
        }

        /// <summary>
        /// Gets the value associated with the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value associated with the specified property, if the property is found; otherwise, null.</param>
        /// <returns>true if the extension properties contains the specified property; otherwise, false.</returns>
        public bool TryGetExtensionProperty(string propertyName, out object value)
        {
            this.CheckDisposed();

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            return this._extensionProperties.TryGetValue(propertyName, out value);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ExtensionPropertiesClass{T}"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ExtensionPropertiesClass{T}"/> class.
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

                if (this._disposeAction != null)
                {
                    try
                    {
                        this._disposeAction(this.Value);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
                else
                {
                    IDisposable source = this.Value as IDisposable;

                    if (source != null)
                    {
                        try
                        {
                            source.Dispose();
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                        }
                    }
                }

                foreach (object item in this._extensionProperties.Values)
                {
                    IDisposable disposable = item as IDisposable;

                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                        }
                    }
                }

                this._extensionProperties.Clear();
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
                throw new ObjectDisposedException("DevLib.ExtensionProperties.ExtensionPropertiesClass{T}");
            }
        }
    }
}
