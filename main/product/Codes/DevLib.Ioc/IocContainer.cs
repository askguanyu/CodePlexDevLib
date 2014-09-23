//-----------------------------------------------------------------------
// <copyright file="IocContainer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Inversion of Control container.
    /// </summary>
    public class IocContainer : IDisposable
    {
        /// <summary>
        /// Field DefaultLabel.
        /// </summary>
        private static readonly string DefaultLabel = Guid.NewGuid().ToString();

        /// <summary>
        /// Field _container.
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, IocRegistration>> _container = new Dictionary<Type, Dictionary<string, IocRegistration>>();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _ignoreCase.
        /// </summary>
        private bool _ignoreCase = false;

        /// <summary>
        /// Field _readerWriterLock.
        /// </summary>
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer" /> class.
        /// </summary>
        public IocContainer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer" /> class.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case when resolve object by label; otherwise, false.</param>
        public IocContainer(bool ignoreCase)
        {
            this._ignoreCase = ignoreCase;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="IocContainer" /> class.
        /// </summary>
        ~IocContainer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Register a type mapping with the container.
        /// </summary>
        /// <typeparam name="T">Type of instance to register.</typeparam>
        /// <param name="instance">Object to return.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>The current IocContainer instance.</returns>
        public IocContainer Register<T>(T instance, string label = null)
        {
            this.CheckDisposed();

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._container.ContainsKey(instanceType)
                    && this._container[instanceType] != null
                    && this._container[instanceType].ContainsKey(label)
                    && this._container[instanceType][label] != null)
                {
                    if (this._container[instanceType][label].HasInstance)
                    {
                        throw new InvalidOperationException(string.Format("An instance of type {0} for label {1} already exists. Unregister the instance first.", instanceType.FullName, label));
                    }
                    else
                    {
                        this._container[instanceType][label].Instance = instance;

                        return this;
                    }
                }
                else
                {
                    if (!this._container.ContainsKey(instanceType) || this._container[instanceType] == null)
                    {
                        this._container.Add(instanceType, new Dictionary<string, IocRegistration>(this._ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.CurrentCulture));
                    }

                    if (!this._container[instanceType].ContainsKey(label) || this._container[instanceType][label] == null)
                    {
                        this._container[instanceType].Add(label, new IocRegistration());
                    }

                    this._container[instanceType][label].Instance = instance;

                    return this;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Register a type mapping with the container.
        /// </summary>
        /// <typeparam name="T">Type to register.</typeparam>
        /// <param name="creation">Delegate method to create a new instance.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>The current IocContainer instance.</returns>
        public IocContainer Register<T>(CreationFunc creation, string label = null)
        {
            this.CheckDisposed();

            if (creation == null)
            {
                throw new ArgumentNullException("creation");
            }

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._container.ContainsKey(instanceType)
                    && this._container[instanceType] != null
                    && this._container[instanceType].ContainsKey(label)
                    && this._container[instanceType][label] != null)
                {
                    if (this._container[instanceType][label].HasCreation)
                    {
                        throw new InvalidOperationException(string.Format("A definition of type {0} for label {1} already exists. Unregister the definition first.", instanceType.FullName, label));
                    }
                    else
                    {
                        this._container[instanceType][label].Creation = creation;

                        return this;
                    }
                }
                else
                {
                    if (!this._container.ContainsKey(instanceType) || this._container[instanceType] == null)
                    {
                        this._container.Add(instanceType, new Dictionary<string, IocRegistration>(this._ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.CurrentCulture));
                    }

                    if (!this._container[instanceType].ContainsKey(label) || this._container[instanceType][label] == null)
                    {
                        this._container[instanceType].Add(label, new IocRegistration());
                    }

                    this._container[instanceType][label].Creation = creation;

                    return this;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Resolve an instance of the requested type from the container.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <param name="createNew">true to call delegate method to create a new instance; false to return a shared instance.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>The retrieved object.</returns>
        public T Resolve<T>(bool createNew = false, string label = null)
        {
            this.CheckDisposed();

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (!this._container.ContainsKey(instanceType)
                    || this._container[instanceType] == null
                    || !this._container[instanceType].ContainsKey(label)
                    || this._container[instanceType][label] == null)
                {
                    throw new InvalidOperationException(string.Format("Type {0} for label {1} does not exist as a registration.", instanceType.FullName, label));
                }
                else
                {
                    if (createNew)
                    {
                        if (this._container[instanceType][label].HasCreation)
                        {
                            return (T)this._container[instanceType][label].Creation.Invoke();
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("A definition of type {0} for label {1} does not exist as a registration.", instanceType.FullName, label));
                        }
                    }
                    else
                    {
                        if (this._container[instanceType][label].HasInstance)
                        {
                            return (T)this._container[instanceType][label].Instance;
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("An instance of type {0} for label {1} does not exist as a registration.", instanceType.FullName, label));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Resolve all from the container.
        /// </summary>
        /// <returns>The retrieved IocRegistration list.</returns>
        public List<IocRegistration> ResolveAll()
        {
            this.CheckDisposed();

            List<IocRegistration> result = new List<IocRegistration>();

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                foreach (var itemType in this._container)
                {
                    try
                    {
                        foreach (var item in itemType.Value)
                        {
                            try
                            {
                                result.Add(item.Value);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }

            return result;
        }

        /// <summary>
        /// Check whether it is possible to resolve a type.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <param name="createNew">true to check delegate method exists; false to check a shared instance exists.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public bool CanResolve<T>(bool createNew = false, string label = null)
        {
            this.CheckDisposed();

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (!this._container.ContainsKey(instanceType)
                    || this._container[instanceType] == null
                    || !this._container[instanceType].ContainsKey(label)
                    || this._container[instanceType][label] == null)
                {
                    return false;
                }
                else
                {
                    return createNew ? this._container[instanceType][label].HasCreation : this._container[instanceType][label].HasInstance;
                }
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Try to resolve an instance of the requested type from the container.
        /// </summary>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <param name="instance">The retrieved object, if it is possible to resolve one.</param>
        /// <param name="createNew">true to call delegate method to create a new instance; false to return a shared instance.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>true if resolve successfully; otherwise, false.</returns>
        public bool TryResolve<T>(out T instance, bool createNew = false, string label = null)
        {
            this.CheckDisposed();

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (!this._container.ContainsKey(instanceType)
                    || this._container[instanceType] == null
                    || !this._container[instanceType].ContainsKey(label)
                    || this._container[instanceType][label] == null)
                {
                    instance = default(T);

                    return false;
                }
                else
                {
                    if (createNew)
                    {
                        if (this._container[instanceType][label].HasCreation)
                        {
                            instance = (T)this._container[instanceType][label].Creation.Invoke();

                            return true;
                        }
                        else
                        {
                            instance = default(T);

                            return false;
                        }
                    }
                    else
                    {
                        if (this._container[instanceType][label].HasInstance)
                        {
                            instance = (T)this._container[instanceType][label].Instance;

                            return true;
                        }
                        else
                        {
                            instance = default(T);

                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                instance = default(T);

                return false;
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Unregister a type mapping with the container.
        /// </summary>
        /// <typeparam name="T">Type to unregister.</typeparam>
        /// <param name="instance">true to unregister the shared instance; false to unregister delegate method.</param>
        /// <param name="label">A unique label that allows multiple implementations of the same type.</param>
        /// <returns>true if unregister successfully; otherwise, false.</returns>
        public bool Unregister<T>(bool instance = true, string label = null)
        {
            this.CheckDisposed();

            if (label == null)
            {
                label = DefaultLabel;
            }

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._container.ContainsKey(instanceType)
                    && this._container[instanceType] != null
                    && this._container[instanceType].ContainsKey(label)
                    && this._container[instanceType][label] != null)
                {
                    if (instance)
                    {
                        this._container[instanceType][label].Instance = null;
                        return true;
                    }
                    else
                    {
                        this._container[instanceType][label].Creation = null;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return false;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Destroy a type mapping with the container.
        /// </summary>
        /// <typeparam name="T">Type to destroy.</typeparam>
        /// <returns>true if destroy successfully; otherwise, false.</returns>
        public bool Destroy<T>()
        {
            this.CheckDisposed();

            Type instanceType = typeof(T);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._container.ContainsKey(instanceType)
                    && this._container[instanceType] != null)
                {
                    this._container[instanceType].Clear();
                    this._container[instanceType] = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return false;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Removes all values from container.
        /// </summary>
        public void Clear()
        {
            this.CheckDisposed();

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                foreach (var item in this._container)
                {
                    try
                    {
                        item.Value.Clear();
                    }
                    catch
                    {
                    }
                }

                this._container.Clear();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IocContainer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IocContainer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IocContainer" /> class.
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

                if (this._readerWriterLock != null)
                {
                    this._readerWriterLock.ReleaseLock();
                    this._readerWriterLock = null;
                }

                if (this._container != null)
                {
                    this._container.Clear();
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
                throw new ObjectDisposedException("DevLib.Ioc.IocContainer");
            }
        }
    }
}
