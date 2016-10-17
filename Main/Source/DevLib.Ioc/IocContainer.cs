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
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
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

            this.CheckType(type);

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

            this.CheckType(type);

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

            this.CheckType(type);

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

            this.CheckType(type);

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
        /// Registers the specified type with the assemblies.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        public virtual void RegisterAssembly(Type type, bool withTypeName, params Assembly[] assemblies)
        {
            this.RegisterAssembly(type, null, withTypeName, assemblies);
        }

        /// <summary>
        /// Registers the specified type with the assemblies.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        public virtual void RegisterAssembly(Type type, Type[] typeArguments, bool withTypeName, params Assembly[] assemblies)
        {
            this.CheckDisposed();

            this.CheckType(type);

            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            foreach (Assembly assembly in assemblies)
            {
                InternalLogger.Log("Register Assembly", assembly.FullName);

                Type[] types = assembly.GetTypes();

                this.InnerRegister(type, types, typeArguments, withTypeName);
            }
        }

        /// <summary>
        /// Registers the specified type with the assemblies.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        public virtual void RegisterAssembly<T>(bool withTypeName, params Assembly[] assemblies)
        {
            this.RegisterAssembly(typeof(T), withTypeName, assemblies);
        }

        /// <summary>
        /// Registers the specified type with the assemblies.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        public virtual void RegisterAssembly<T>(Type[] typeArguments, bool withTypeName, params Assembly[] assemblies)
        {
            this.RegisterAssembly(typeof(T), typeArguments, withTypeName, assemblies);
        }

        /// <summary>
        /// Registers the specified type with the files.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="filenames">The files to scan.</param>
        public virtual void RegisterFile(Type type, bool withTypeName, params string[] filenames)
        {
            this.RegisterFile(type, null, withTypeName, filenames);
        }

        /// <summary>
        /// Registers the specified type with the files.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="filenames">The files to scan.</param>
        public virtual void RegisterFile(Type type, Type[] typeArguments, bool withTypeName, params string[] filenames)
        {
            this.CheckDisposed();

            this.CheckType(type);

            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            }

            foreach (string item in filenames)
            {
                string filename = Path.GetFullPath(item);

                try
                {
                    this.InnerRegister(type, filename, typeArguments, withTypeName);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e, filename);
                }
            }
        }

        /// <summary>
        /// Registers the specified type with the files.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="filenames">The files to scan.</param>
        public virtual void RegisterFile<T>(bool withTypeName, params string[] filenames)
        {
            this.RegisterFile(typeof(T), withTypeName, filenames);
        }

        /// <summary>
        /// Registers the specified type with the files.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="filenames">The files to scan.</param>
        public virtual void RegisterFile<T>(Type[] typeArguments, bool withTypeName, params string[] filenames)
        {
            this.RegisterFile(typeof(T), typeArguments, withTypeName, filenames);
        }

        /// <summary>
        /// Registers the specified type with the directories.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="recursive">true to scan all files from the directories and all subdirectories; otherwise, only scan files from the directory.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="paths">The paths to scan.</param>
        public virtual void RegisterDirectory(Type type, bool recursive, bool withTypeName, params string[] paths)
        {
            this.RegisterDirectory(type, null, recursive, withTypeName, paths);
        }

        /// <summary>
        /// Registers the specified type with the directories.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="recursive">true to scan all files from the directory and all subdirectories; otherwise, only scan files from the directory.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="paths">The paths to scan.</param>
        public virtual void RegisterDirectory(Type type, Type[] typeArguments, bool recursive, bool withTypeName, params string[] paths)
        {
            this.CheckDisposed();

            this.CheckType(type);

            if (paths == null || paths.Length < 1)
            {
                paths = new[] { Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath) };
            }

            foreach (string item in paths)
            {
                if (Directory.Exists(item))
                {
                    foreach (string file in Directory.GetFiles(item, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        string filename = Path.GetFullPath(file);

                        try
                        {
                            this.InnerRegister(type, filename, typeArguments, withTypeName);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Log(e, filename);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers the specified type with the directories.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="recursive">true to scan all files from the directory and all subdirectories; otherwise, only scan files from the directory.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="paths">The paths to scan.</param>
        public virtual void RegisterDirectory<T>(bool recursive, bool withTypeName, params string[] paths)
        {
            this.RegisterDirectory(typeof(T), recursive, withTypeName, paths);
        }

        /// <summary>
        /// Registers the specified type with the directories.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="recursive">true to scan all files from the directory and all subdirectories; otherwise, only scan files from the directory.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        /// <param name="paths">The paths to scan.</param>
        public virtual void RegisterDirectory<T>(Type[] typeArguments, bool recursive, bool withTypeName, params string[] paths)
        {
            this.RegisterDirectory(typeof(T), typeArguments, recursive, withTypeName, paths);
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The registered instance.</returns>
        public virtual object Resolve(Type type, bool createNew = false)
        {
            return this.DoGetInstance(type, createNew);
        }

        /// <summary>
        /// Resolves the specified type.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The registered instance.</returns>
        public virtual T Resolve<T>(bool createNew = false)
        {
            return (T)this.Resolve(typeof(T), createNew);
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The registered instance.</returns>
        public virtual object Resolve(Type type, string name, bool createNew = false)
        {
            return this.DoGetInstance(type, name, createNew);
        }

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The registered instance.</returns>
        public virtual T Resolve<T>(string name, bool createNew = false)
        {
            return (T)this.Resolve(typeof(T), name, createNew);
        }

        /// <summary>
        /// Determines whether this specified type can resolve.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>true if can resolve; otherwise, false.</returns>
        public virtual bool CanResolve(Type type)
        {
            this.CheckDisposed();

            this.CheckType(type);

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

            this.CheckType(type);

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
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve(Type type, out object value, bool createNew = false)
        {
            this.CheckDisposed();

            this.CheckType(type);

            lock (((ICollection)this._registrations).SyncRoot)
            {
                object result = null;

                OrderedDictionary valueDictionary;

                if (this._registrations.TryGetValue(type, out valueDictionary) && valueDictionary != null && valueDictionary.Count > 0)
                {
                    if (valueDictionary.Count == 1)
                    {
                        IocRegistrationBuilder builder = valueDictionary[0] as IocRegistrationBuilder;

                        if (builder != null)
                        {
                            try
                            {
                                result = builder.GetValue(this, createNew);
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
                                result = builder.GetValue(this, createNew);
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
                            result = resolver.Resolve(type, createNew);
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
                    this._parent.TryResolve(type, out result, createNew);
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
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve<T>(out T value, bool createNew = false)
        {
            object outValue;

            bool result = this.TryResolve(typeof(T), out outValue, createNew);

            value = (T)outValue;

            return result;
        }

        /// <summary>
        /// Tries to resolve the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="value">The registered instance.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>true if resolve succeeded; otherwise, false.</returns>
        public virtual bool TryResolve(Type type, string name, out object value, bool createNew = false)
        {
            this.CheckDisposed();

            this.CheckType(type);

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
                            result = builder.GetValue(this, createNew);
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
                            result = resolver.Resolve(type, name, createNew);
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
                    this._parent.TryResolve(type, name, out result, createNew);
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
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>true if resolve succeeded; otherwise, false. </returns>
        public virtual bool TryResolve<T>(string name, out T value, bool createNew = false)
        {
            object outValue;

            bool result = this.TryResolve(typeof(T), name, out outValue, createNew);

            value = (T)outValue;

            return result;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
        public virtual object GetService(Type serviceType, string name, bool createNew = false)
        {
            return this.Resolve(serviceType, name, createNew);
        }

        /// <summary>
        /// Unregisters the specified type, all named registrations will be kept.
        /// </summary>
        /// <param name="type">The type to unregister.</param>
        public virtual void Unregister(Type type)
        {
            this.CheckDisposed();

            this.CheckType(type);

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

            this.CheckType(type);

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

            this.CheckType(type);

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
        /// InnerRegister method.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="types">The types to scan.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        protected virtual void InnerRegister(Type type, Type[] types, Type[] typeArguments, bool withTypeName)
        {
            bool checkGeneric = typeArguments != null && typeArguments.Length > 0;

            foreach (Type item in types)
            {
                if (!item.IsAbstract && (checkGeneric ? item.IsGenericType : !item.IsGenericType) && this.IsInheritFrom(item, type))
                {
                    this.Register(type, container => container.CreateInstance(item, typeArguments), withTypeName ? item.FullName : item.AssemblyQualifiedName);
                }
            }
        }

        /// <summary>
        /// InnerRegister method.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="filename">The file to scan.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="withTypeName">true to use type full name; false to use AssemblyQualifiedName.</param>
        protected virtual void InnerRegister(Type type, string filename, Type[] typeArguments, bool withTypeName)
        {
            string assemblyFullName = AssemblyName.GetAssemblyName(filename).FullName;

            InternalLogger.Log("Register File", filename, assemblyFullName);

            Assembly assembly = null;

            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemblyFullName.Equals(item.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    assembly = item;
                    break;
                }
            }

            if (assembly == null)
            {
                assembly = Assembly.Load(File.ReadAllBytes(filename));
            }

            Type[] types = assembly.GetTypes();

            this.InnerRegister(type, types, typeArguments, withTypeName);
        }

        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected virtual object CreateInstance(Type type, Type[] typeArguments)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            object result = null;

            if (typeArguments != null && typeArguments.Length > 0)
            {
                type = type.MakeGenericType(typeArguments);
            }

            try
            {
                result = Activator.CreateInstance(type);
            }
            catch
            {
                try
                {
                    result = FormatterServices.GetUninitializedObject(type);
                }
                catch
                {
                }
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType, bool createNew = false)
        {
            this.CheckDisposed();

            this.CheckType(serviceType);

            object result;

            if (!this.TryResolve(serviceType, out result, createNew) || result == null)
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
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType, string key, bool createNew = false)
        {
            this.CheckDisposed();

            this.CheckType(serviceType);

            object result;

            if (!this.TryResolve(serviceType, key, out result, createNew) || result == null)
            {
                throw new InvalidOperationException(string.Format("The registration [{0} with name {1}] does not exist.", serviceType.FullName, key));
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType, bool createNew = false)
        {
            this.CheckDisposed();

            this.CheckType(serviceType);

            List<object> result = new List<object>();

            lock (((ICollection)this._registrations).SyncRoot)
            {
                OrderedDictionary value;

                if (this._registrations.TryGetValue(serviceType, out value) && value != null && value.Count > 0)
                {
                    foreach (IocRegistrationBuilder item in value.Values)
                    {
                        result.Add(item.GetValue(this, createNew));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<TService> DoGetAllInstances<TService>(bool createNew = false)
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
                        result.Add((TService)item.GetValue(this, createNew));
                    }
                }
            }

            return result;
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
        /// Check whether the source type inherits from baseType.
        /// </summary>
        /// <param name="subType">The sub type.</param>
        /// <param name="baseType">The base type.</param>
        /// <returns>true if the sub type inherits from baseType; otherwise, false.</returns>
        private bool IsInheritFrom(Type subType, Type baseType)
        {
            if (subType.Equals(baseType)
            || ((subType.AssemblyQualifiedName != null || baseType.AssemblyQualifiedName != null) && subType.AssemblyQualifiedName == baseType.AssemblyQualifiedName))
            {
                return true;
            }

            if (!baseType.IsGenericType || !subType.IsGenericType)
            {
                if (baseType.IsAssignableFrom(subType))
                {
                    return true;
                }
            }
            else if (subType.GetGenericTypeDefinition().Equals(baseType.GetGenericTypeDefinition()))
            {
                return true;
            }

            foreach (Type item in subType.GetInterfaces())
            {
                if (this.IsInheritFrom(item, baseType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Thread safety raise event.
        /// </summary>
        /// <param name="source">Source EventHandler.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        private void RaiseEvent(EventHandler source, object sender, EventArgs e = null)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// Checks the type is not null.
        /// </summary>
        /// <param name="type">The type to check.</param>
        private void CheckType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
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
