//-----------------------------------------------------------------------
// <copyright file="IocContainer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Inversion of Control container.
    /// </summary>
    [DebuggerDisplay("Name={ContainerName}, Registrations={RegistrationsCount}, RegisterTypes={_registrations.Count}, Resolvers={_resolvers.Count}, Parent={_parent.ContainerName}")]
    public class IocContainer : ServiceLocatorImplBase, IServiceLocator, IResolver, IDisposable
    {
        /// <summary>
        /// Field NullStringKey.
        /// </summary>
        public const string NullStringKey = "23DEA637-6844-4451-AC06-A5570F37F2D4";

        /// <summary>
        /// Field _registrations.
        /// </summary>
        private readonly Dictionary<Type, OrderedDictionary> _registrations = new Dictionary<Type, OrderedDictionary>();

        /// <summary>
        /// Field _resolvers.
        /// </summary>
        private readonly List<IResolver> _resolvers = new List<IResolver>();

        /// <summary>
        /// Field _parent.
        /// </summary>
        private readonly IocContainer _parent;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer"/> class.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        public IocContainer(string containerName = null)
        {
            this.ContainerName = containerName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="containerName">Name of the container.</param>
        private IocContainer(IocContainer parent, string containerName = null)
        {
            this._parent = parent;
            this.ContainerName = containerName;

            if (this._parent != null)
            {
                this._parent.Disposed += this.OnParentDisposed;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="IocContainer" /> class.
        /// </summary>
        ~IocContainer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs when disposed.
        /// </summary>
        private event EventHandler Disposed;

        /// <summary>
        /// Gets the registrations count.
        /// </summary>
        public int RegistrationsCount
        {
            get
            {
                int result = 0;

                foreach (OrderedDictionary item in this._registrations.Values)
                {
                    if (item != null)
                    {
                        result += item.Count;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the resolve providers.
        /// </summary>
        public ReadOnlyCollection<IResolver> ResolveProviders
        {
            get
            {
                return this._resolvers.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the name of the container.
        /// </summary>
        public string ContainerName
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds the resolve provider.
        /// </summary>
        /// <param name="resolveProvider">The resolve provider.</param>
        public void AddResolveProvider(IResolver resolveProvider)
        {
            this.CheckDisposed();

            this._resolvers.Add(resolveProvider);
        }

        /// <summary>
        /// Registers the specified type with builder function.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="builder">Delegate method to create a new instance.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register(Type type, Converter<IocContainer, object> builder)
        {
            this.CheckDisposed();

            return this.InnerRegister(type, new IocRegistrationBuilder(type, container => builder(container)));
        }

        /// <summary>
        /// Registers the specified type with builder function.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="builder">Delegate method to create a new instance.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register<T>(Converter<IocContainer, T> builder)
        {
            this.CheckDisposed();

            Type type = typeof(T);

            return this.InnerRegister(type, new IocRegistrationBuilder(type, container => (T)builder(container)));
        }

        /// <summary>
        /// Registers the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="builder">Delegate method to create a new instance.</param>
        /// <param name="name">The name of registration to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register(Type type, Converter<IocContainer, object> builder, string name)
        {
            this.CheckDisposed();

            return this.InnerRegister(type, new IocRegistrationBuilder(type, container => builder(container), name), name);
        }

        /// <summary>
        /// Registers the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="builder">Delegate method to create a new instance.</param>
        /// <param name="name">The name of registration to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register<T>(Converter<IocContainer, T> builder, string name)
        {
            this.CheckDisposed();

            Type type = typeof(T);

            return this.InnerRegister(type, new IocRegistrationBuilder(type, container => (T)builder(container), name), name);
        }

        /// <summary>
        /// Registers the specified type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register(Type type, object instance)
        {
            this.CheckDisposed();

            return this.InnerRegister(type, new IocRegistrationBuilder(type, instance));
        }

        /// <summary>
        /// Registers the specified type.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register<T>(T instance)
        {
            this.CheckDisposed();

            Type type = typeof(T);

            return this.InnerRegister(type, new IocRegistrationBuilder(type, instance));
        }

        /// <summary>
        /// Registers the specified type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <param name="name">The name of registration to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register(Type type, object instance, string name)
        {
            this.CheckDisposed();

            return this.InnerRegister(type, new IocRegistrationBuilder(type, instance, name), name);
        }

        /// <summary>
        /// Registers the specified type.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <param name="name">The name of registration to register.</param>
        /// <returns>IocRegistration instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "reviewed")]
        public virtual IocRegistration Register<T>(T instance, string name)
        {
            this.CheckDisposed();

            Type type = typeof(T);

            return this.InnerRegister(type, new IocRegistrationBuilder(type, instance, name), name);
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The registered instance.</returns>
        public virtual object Resolve(Type type)
        {
            return this.DoGetInstance(type);
        }

        /// <summary>
        /// Resolves the specified type.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The registered instance.</returns>
        public virtual T Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>The registered instance.</returns>
        public virtual object Resolve(Type type, string name)
        {
            return this.DoGetInstance(type, name);
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>The registered instance.</returns>
        public virtual T Resolve<T>(string name)
        {
            return (T)this.Resolve(typeof(T), name);
        }

        /// <summary>
        /// Determines whether this specified type can resolve.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public virtual bool CanResolve(Type type)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                return this._registrations.TryGetValue(type, out value) && value != null && value.Count > 0;
            }
        }

        /// <summary>
        /// Determines whether this specified type can resolve.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public virtual bool CanResolve<T>()
        {
            return this.CanResolve(typeof(T));
        }

        /// <summary>
        /// Determines whether this specified type can resolve.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public virtual bool CanResolve(Type type, string name)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                return this._registrations.TryGetValue(type, out value)
                    && value != null
                    && value.Count > 0
                    && value.Contains(name)
                    && value[name] != null;
            }
        }

        /// <summary>
        /// Determines whether this specified type can resolve.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public virtual bool CanResolve<T>(string name)
        {
            return this.CanResolve(typeof(T), name);
        }

        /// <summary>
        /// Tries to resolve the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="value">The registered instance.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve(Type type, out object value)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                object result = null;

                OrderedDictionary valueDictionary;

                if (this._registrations.TryGetValue(type, out valueDictionary) && valueDictionary != null && valueDictionary.Count > 0)
                {
                    if (valueDictionary.Count == 1)
                    {
                        IocRegistrationBuilder builder = valueDictionary[0] as IocRegistrationBuilder;

                        if (builder != null && !builder.IsEvaluated)
                        {
                            try
                            {
                                result = builder.GetValue(this);
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                        }
                    }
                    else
                    {
                        List<IocRegistrationBuilder> builders = new List<IocRegistrationBuilder>(valueDictionary.Count);

                        foreach (IocRegistrationBuilder item in valueDictionary.Values)
                        {
                            builders.Add(item);
                        }

                        IocRegistrationBuilder builder = builders.FindLast(b => b.HasInstance || !b.IsEvaluated);

                        if (builder != null)
                        {
                            try
                            {
                                result = builder.GetValue(this);
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                        }
                    }
                }

                if (result == null)
                {
                    foreach (IResolver resolver in this._resolvers)
                    {
                        try
                        {
                            result = resolver.Resolve(type);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                        }

                        if (result != null)
                        {
                            break;
                        }
                    }
                }

                if (result == null && this._parent != null)
                {
                    this._parent.TryResolve(type, out result);
                }

                value = result;

                return result != null;
            }
        }

        /// <summary>
        /// Tries to resolve the specified type.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="value">The registered instance.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve<T>(out T value)
        {
            object outValue;

            bool result = this.TryResolve(typeof(T), out outValue);

            value = (T)outValue;

            return result;
        }

        /// <summary>
        /// Tries to resolve the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="value">The registered instance.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve(Type type, string name, out object value)
        {
            this.CheckDisposed();

            if (name == null)
            {
                name = NullStringKey;
            }

            lock (((ICollection)this._registrations).SyncRoot)
            {
                object result = null;

                OrderedDictionary valueDictionary;

                if (this._registrations.TryGetValue(type, out valueDictionary)
                    && valueDictionary != null
                    && valueDictionary.Count > 0
                    && valueDictionary.Contains(name))
                {
                    IocRegistrationBuilder builder = valueDictionary[name] as IocRegistrationBuilder;

                    if (builder != null)
                    {
                        try
                        {
                            result = builder.GetValue(this);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                        }
                    }
                }

                if (result == null)
                {
                    foreach (IResolver resolver in this._resolvers)
                    {
                        try
                        {
                            result = resolver.Resolve(type, name);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e);
                        }

                        if (result != null)
                        {
                            break;
                        }
                    }
                }

                if (result == null && this._parent != null)
                {
                    this._parent.TryResolve(type, name, out result);
                }

                value = result;

                return result != null;
            }
        }

        /// <summary>
        /// Tries to resolve the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="value">The registered instance.</param>
        /// <returns>true if resolve succeeded; otherwise, false. </returns>
        public virtual bool TryResolve<T>(string name, out T value)
        {
            object outValue;

            bool result = this.TryResolve(typeof(T), name, out outValue);

            value = (T)outValue;

            return result;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
        public virtual object GetService(Type serviceType, string name)
        {
            return this.Resolve(serviceType, name);
        }

        /// <summary>
        /// Unregisters the specified type, all named registrations will be kept.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        public virtual void Unregister(Type type)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                if (this._registrations.TryGetValue(type, out value)
                    && value != null
                    && value.Count > 0)
                {
                    List<DictionaryEntry> result = new List<DictionaryEntry>();

                    foreach (DictionaryEntry item in value)
                    {
                        IocRegistrationBuilder builder = item.Value as IocRegistrationBuilder;

                        if (builder != null && !builder.IsNamed)
                        {
                            result.Add(item);
                        }
                    }

                    foreach (DictionaryEntry item in result)
                    {
                        IDisposable disposable = item.Value as IDisposable;

                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }

                        value.Remove(item.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters the specified type, all named registrations will be kept.
        /// </summary>
        /// <typeparam name="T">The type to unregister.</typeparam>
        public virtual void Unregister<T>()
        {
            this.Unregister(typeof(T));
        }

        /// <summary>
        /// Unregisters the specified type.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        /// <param name="name">The name of the registration.</param>
        public virtual void Unregister(Type type, string name)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                if (this._registrations.TryGetValue(type, out value)
                    && value != null
                    && value.Count > 0
                    && value.Contains(name)
                    && value[name] != null)
                {
                    IDisposable disposable = value[name] as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }

                    value.Remove(name);
                }
            }
        }

        /// <summary>
        /// Unregisters the specified type.
        /// </summary>
        /// <typeparam name="T">The type to unregister.</typeparam>
        /// <param name="name">The name of the registration.</param>
        public virtual void Unregister<T>(string name)
        {
            this.Unregister(typeof(T), name);
        }

        /// <summary>
        /// Destroys all registrations of the specified type.
        /// </summary>
        /// <param name="type">Type to destroy.</param>
        public virtual void Destroy(Type type)
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary valueDictionary;

                if (this._registrations.TryGetValue(type, out valueDictionary) && valueDictionary != null)
                {
                    foreach (IDisposable registrationBuilder in valueDictionary.Values)
                    {
                        registrationBuilder.Dispose();
                    }

                    valueDictionary.Clear();
                }

                this._registrations.Remove(type);
            }
        }

        /// <summary>
        /// Destroys all registrations of the specified type.
        /// </summary>
        /// <typeparam name="T">Type to destroy.</typeparam>
        public virtual void Destroy<T>()
        {
            this.Destroy(typeof(T));
        }

        /// <summary>
        /// Clears all registrations of this container.
        /// </summary>
        public virtual void Clear()
        {
            this.CheckDisposed();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                foreach (OrderedDictionary item in this._registrations.Values)
                {
                    foreach (IDisposable registrationBuilder in item.Values)
                    {
                        registrationBuilder.Dispose();
                    }

                    item.Clear();
                }

                this._registrations.Clear();
            }
        }

        /// <summary>
        /// Creates the child container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>Child container instance.</returns>
        public IocContainer CreateChildContainer(string containerName = null)
        {
            this.CheckDisposed();

            return new IocContainer(this, containerName);
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
        /// Thread safety raise event.
        /// </summary>
        /// <param name="source">Source EventHandler.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        public void RaiseEvent(EventHandler source, object sender, EventArgs e = null)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// InnerRegister method.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="builder">The registration builder.</param>
        /// <param name="name">The name to register..</param>
        /// <returns>Registration instance.</returns>
        protected virtual IocRegistration InnerRegister(Type type, IocRegistrationBuilder builder, string name)
        {
            try
            {
                if (name == null)
                {
                    name = NullStringKey;
                }

                lock (((ICollection)this._registrations).SyncRoot)
                {
                    if (!this._registrations.ContainsKey(type) || this._registrations[type] == null)
                    {
                        this._registrations[type] = new OrderedDictionary();
                    }

                    this._registrations[type][name] = builder;
                }

                return new IocRegistration(
                    registration =>
                    {
                        lock (((ICollection)this._registrations).SyncRoot)
                        {
                            this._registrations[type].Remove(registration.Name);
                        }

                        builder.Dispose();
                    },
                    name);
            }
            catch (Exception e)
            {
                InvalidOperationException invalidOperationException = new InvalidOperationException(string.Format("Failed to register service [{0}]", type.FullName), e);

                InternalLogger.Log(invalidOperationException);

                throw invalidOperationException;
            }
        }

        /// <summary>
        /// InnerRegister method.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="builder">The registration builder.</param>
        /// <returns>Registration instance.</returns>
        protected virtual IocRegistration InnerRegister(Type type, IocRegistrationBuilder builder)
        {
            try
            {
                string id = Guid.NewGuid().ToString();

                lock (((ICollection)this._registrations).SyncRoot)
                {
                    if (!this._registrations.ContainsKey(type) || this._registrations[type] == null)
                    {
                        this._registrations[type] = new OrderedDictionary();
                    }

                    this._registrations[type][id] = builder;
                }

                return new IocRegistration(
                registration =>
                {
                    lock (((ICollection)this._registrations).SyncRoot)
                    {
                        this._registrations[type].Remove(registration.Name);
                    }

                    builder.Dispose();
                },
                    id);
            }
            catch (Exception e)
            {
                InvalidOperationException invalidOperationException = new InvalidOperationException(string.Format("Failed to register service [{0}]", type.FullName), e);

                InternalLogger.Log(invalidOperationException);

                throw invalidOperationException;
            }
        }

        /// <summary>
        /// Called when parent disposed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnParentDisposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType)
        {
            this.CheckDisposed();

            object result;

            if (!this.TryResolve(serviceType, out result) || result == null)
            {
                throw new InvalidOperationException(string.Format("The registration [{0}] does not exist.", serviceType.FullName));
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            this.CheckDisposed();

            object result;

            if (!this.TryResolve(serviceType, key, out result) || result == null)
            {
                throw new InvalidOperationException(string.Format("The registration [{0} with name {1}] does not exist.", serviceType.FullName, key));
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            this.CheckDisposed();

            List<object> result = new List<object>();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                if (this._registrations.TryGetValue(serviceType, out value) && value != null && value.Count > 0)
                {
                    foreach (IocRegistrationBuilder item in value.Values)
                    {
                        result.Add(item.GetValue(this));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<TService> DoGetAllInstances<TService>()
        {
            this.CheckDisposed();

            Type serviceType = typeof(TService);

            List<TService> result = new List<TService>();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                if (this._registrations.TryGetValue(serviceType, out value) && value != null && value.Count > 0)
                {
                    foreach (IocRegistrationBuilder item in value.Values)
                    {
                        result.Add((TService)item.GetValue(this));
                    }
                }
            }

            return result;
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

                if (this._parent != null)
                {
                    this._parent.Disposed -= this.OnParentDisposed;
                }

                lock (((ICollection)this._registrations).SyncRoot)
                {
                    foreach (OrderedDictionary item in this._registrations.Values)
                    {
                        foreach (IDisposable registrationBuilder in item.Values)
                        {
                            registrationBuilder.Dispose();
                        }

                        item.Clear();
                    }

                    this._registrations.Clear();
                }

                this._resolvers.Clear();

                this.RaiseEvent(this.Disposed, this, EventArgs.Empty);
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
