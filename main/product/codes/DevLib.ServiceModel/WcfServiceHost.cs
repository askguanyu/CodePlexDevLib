﻿//-----------------------------------------------------------------------
// <copyright file="WcfServiceHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Class WcfServiceHost.
    /// </summary>
    [Serializable]
    public sealed class WcfServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _appDomain.
        /// </summary>
        [NonSerialized]
        private AppDomain _appDomain;

        /// <summary>
        /// Field _wcfServiceHostProxy.
        /// </summary>
        private WcfServiceHostProxy _wcfServiceHostProxy;

        /// <summary>
        /// Field _assemblyFile.
        /// </summary>
        private string _assemblyFile = null;

        /// <summary>
        /// Field _serviceType.
        /// </summary>
        private Type _serviceType = null;

        /// <summary>
        /// Field _contractType.
        /// </summary>
        private Type _contractType = null;

        /// <summary>
        /// Field _binding.
        /// </summary>
        [NonSerialized]
        private Binding _binding = null;

        /// <summary>
        /// Field _bindingType.
        /// </summary>
        private Type _bindingType = null;

        /// <summary>
        /// Field _configFile.
        /// </summary>
        private string _configFile = null;

        /// <summary>
        /// Field _baseAddress.
        /// </summary>
        private string _baseAddress = null;

        /// <summary>
        /// Field _tempConfigFile.
        /// </summary>
        private string _tempConfigFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// Default constructor of WcfServiceHost, create an isolated AppDomain to host Wcf service. Use Initialize method to initialize Wcf service.
        /// </summary>
        public WcfServiceHost()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, string configFile, string baseAddress = null, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, configFile, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, bindingType, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Type bindingType, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, contractType, bindingType, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, Binding binding, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, binding, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Binding binding, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, contractType, binding, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, string configFile, string baseAddress = null, bool autoOpen = false)
        {
            this.Initialize(serviceType, configFile, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(serviceType, bindingType, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Type bindingType, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(serviceType, contractType, bindingType, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Binding binding, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(serviceType, binding, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Binding binding, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(serviceType, contractType, binding, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        ~WcfServiceHost()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event Created.
        /// </summary>
        public event EventHandler<EventArgs> Created;

        /// <summary>
        /// Event Opening.
        /// </summary>
        public event EventHandler<EventArgs> Opening;

        /// <summary>
        /// Event Opened.
        /// </summary>
        public event EventHandler<EventArgs> Opened;

        /// <summary>
        /// Event Closing.
        /// </summary>
        public event EventHandler<EventArgs> Closing;

        /// <summary>
        /// Event Closed.
        /// </summary>
        public event EventHandler<EventArgs> Closed;

        /// <summary>
        /// Event Aborting.
        /// </summary>
        public event EventHandler<EventArgs> Aborting;

        /// <summary>
        /// Event Aborted.
        /// </summary>
        public event EventHandler<EventArgs> Aborted;

        /// <summary>
        /// Event Restarting.
        /// </summary>
        public event EventHandler<EventArgs> Restarting;

        /// <summary>
        /// Event Restarted.
        /// </summary>
        public event EventHandler<EventArgs> Restarted;

        /// <summary>
        /// Event Loaded.
        /// </summary>
        public event EventHandler<EventArgs> Loaded;

        /// <summary>
        /// Event Unloaded.
        /// </summary>
        public event EventHandler<EventArgs> Unloaded;

        /// <summary>
        /// Event Reloaded.
        /// </summary>
        public event EventHandler<EventArgs> Reloaded;

        /// <summary>
        /// Gets a value indicating whether isolated AppDomain is loaded.
        /// </summary>
        public bool IsAppDomainLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, string configFile, string baseAddress = null)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", configFile);
            }

            if (!string.IsNullOrEmpty(baseAddress) && !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress);
            }

            this._assemblyFile = assemblyFile;
            this._configFile = configFile;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type bindingType, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a System.ServiceModel.Channels.Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._assemblyFile = assemblyFile;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type contractType, Type bindingType, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (contractType == null)
            {
                throw new ArgumentNullException("contractType");
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!WcfServiceType.HasServiceContractAttribute(contractType))
            {
                throw new ArgumentException("The parameter contractType is not a Wcf contract.", "contractType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a System.ServiceModel.Channels.Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._assemblyFile = assemblyFile;
            this._contractType = contractType;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Binding binding, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._assemblyFile = assemblyFile;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type contractType, Binding binding, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (contractType == null)
            {
                throw new ArgumentNullException("contractType");
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (!WcfServiceType.HasServiceContractAttribute(contractType))
            {
                throw new ArgumentException("The parameter contractType is not a Wcf contract.", "contractType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._assemblyFile = assemblyFile;
            this._contractType = contractType;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, string configFile, string baseAddress = null)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", configFile);
            }

            if (!string.IsNullOrEmpty(baseAddress) && !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress);
            }

            this._serviceType = serviceType;
            this._configFile = configFile;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type bindingType, string baseAddress)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a System.ServiceModel.Channels.Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._serviceType = serviceType;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type contractType, Type bindingType, string baseAddress)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (contractType == null)
            {
                throw new ArgumentNullException("contractType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (!WcfServiceType.HasServiceContractAttribute(contractType))
            {
                throw new ArgumentException("The parameter contractType is not a Wcf contract.", "contractType");
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a System.ServiceModel.Channels.Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._serviceType = serviceType;
            this._contractType = contractType;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Binding binding, string baseAddress)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._serviceType = serviceType;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type contractType, Binding binding, string baseAddress)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (contractType == null)
            {
                throw new ArgumentNullException("contractType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (!WcfServiceType.HasServiceContractAttribute(contractType))
            {
                throw new ArgumentException("The parameter contractType is not a Wcf contract.", "contractType");
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

            this._serviceType = serviceType;
            this._contractType = contractType;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this.InitWcfServiceHostProxy();
        }

        /// <summary>
        /// Open Wcf service.
        /// </summary>
        public void Open()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Open();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Close Wcf service.
        /// </summary>
        public void Close()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Close();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Abort Wcf service.
        /// </summary>
        public void Abort()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Abort();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Restart Wcf service.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Restart();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Unload current isolated AppDomain.
        /// </summary>
        public void Unload()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                this._wcfServiceHostProxy.Dispose();
                this.UnSubscribeAllWcfServiceHostProxyEvent();
                this._wcfServiceHostProxy = null;
            }

            if (this._appDomain != null)
            {
                this.UnSubscribeDomainExitEvent();
                AppDomain.Unload(this._appDomain);
                this.IsAppDomainLoaded = false;
                this._appDomain = null;
                this.RaiseEvent(this.Unloaded, null);
            }

            this.CleanTempWcfConfigFile();
        }

        /// <summary>
        /// Reload current isolated AppDomain.
        /// </summary>
        public void Reload()
        {
            this.CheckDisposed();

            this.Unload();
            this.InitWcfServiceHostProxy();
            this.RaiseEvent(this.Reloaded, null);
        }

        /// <summary>
        /// Get current isolated AppDomain.
        /// </summary>
        /// <returns>Instance of AppDomain.</returns>
        public AppDomain GetAppDomain()
        {
            this.CheckDisposed();

            return this._appDomain;
        }

        /// <summary>
        /// Get Wcf service state list.
        /// </summary>
        /// <returns>Instance of List.</returns>
        public List<WcfServiceHostInfo> GetHostInfoList()
        {
            this.CheckDisposed();

            return this._wcfServiceHostProxy.GetHostInfoList();
        }

        /// <summary>
        /// Gives the <see cref="T:System.AppDomain" /> an infinite lifetime by preventing a lease from being created.
        /// </summary>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain.</exception>
        /// <returns>Always null.</returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method RemoveWcfConfigFileBaseAddressNode.
        /// </summary>
        /// <param name="sourceFileName">Source wcf config file name.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <returns>Temp Wcf config file full path.</returns>
        private string GetTempWcfConfigFile(string sourceFileName, string baseAddress)
        {
            string result = null;

            try
            {
                result = Path.GetTempFileName();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            if (string.IsNullOrEmpty(sourceFileName))
            {
                File.WriteAllText(result, @"<configuration></configuration>");

                return result;
            }

            if (string.IsNullOrEmpty(baseAddress))
            {
                try
                {
                    File.Copy(sourceFileName, result, true);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(sourceFileName);

                    var nodeList = xmlDocument.SelectNodes(@"configuration/system.serviceModel/services/service/host");

                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode item in nodeList)
                        {
                            item.RemoveAll();
                        }
                    }

                    xmlDocument.Save(result);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHost" /> class.
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

                if (this._wcfServiceHostProxy != null)
                {
                    this._wcfServiceHostProxy.Dispose();
                    this.UnSubscribeAllWcfServiceHostProxyEvent();
                    this._wcfServiceHostProxy = null;
                }

                if (this._appDomain != null)
                {
                    this.UnSubscribeDomainExitEvent();
                    AppDomain.Unload(this._appDomain);
                    this.IsAppDomainLoaded = false;
                    this._appDomain = null;
                }

                this.CleanTempWcfConfigFile();
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void RaiseEvent(EventHandler<EventArgs> eventHandler, EventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<EventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Method CreateDomain.
        /// </summary>
        private void InitWcfServiceHostProxy()
        {
            try
            {
                this.CleanTempWcfConfigFile();

                AppDomainSetup appDomainSetup = this.CloneDeep(AppDomain.CurrentDomain.SetupInformation);
                appDomainSetup.ApplicationName = Path.GetFileNameWithoutExtension(this._assemblyFile) ?? this._serviceType.FullName;
                appDomainSetup.ConfigurationFile = this._tempConfigFile = this.GetTempWcfConfigFile(this._configFile, this._baseAddress);
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;

                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

                this.SubscribeDomainExitEvent();

                this._wcfServiceHostProxy = this._appDomain.CreateInstanceAndUnwrap(
                                                                                    Assembly.GetExecutingAssembly().FullName,
                                                                                    typeof(WcfServiceHostProxy).FullName,
                                                                                    true,
                                                                                    BindingFlags.Default,
                                                                                    null,
                                                                                    new object[] { this._assemblyFile, this._serviceType, this._contractType, this._binding, this._bindingType, this._configFile, this._baseAddress },
                                                                                    null,
                                                                                    null,
                                                                                    null) as WcfServiceHostProxy;

                this.IsAppDomainLoaded = true;
                this.SubscribeAllWcfServiceHostProxyEvent();
                this.RaiseEvent(this.Loaded, null);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                this.Unload();
                throw;
            }
        }

        /// <summary>
        /// Method SubscribeDomainExitEvent.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void SubscribeDomainExitEvent()
        {
            this._appDomain.DomainUnload += this.DomainExit;
            this._appDomain.ProcessExit += this.DomainExit;
            this._appDomain.UnhandledException += this.DomainExit;
        }

        /// <summary>
        /// Method UnSubscribeDomainExitEvent.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void UnSubscribeDomainExitEvent()
        {
            this._appDomain.DomainUnload -= this.DomainExit;
            this._appDomain.ProcessExit -= this.DomainExit;
            this._appDomain.UnhandledException -= this.DomainExit;
        }

        /// <summary>
        /// Method SubscribeAllWcfServiceHostProxyEvent.
        /// </summary>
        private void SubscribeAllWcfServiceHostProxyEvent()
        {
            this._wcfServiceHostProxy.Created += (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHostProxy.Opening += (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHostProxy.Opened += (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHostProxy.Closing += (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHostProxy.Closed += (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHostProxy.Aborting += (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHostProxy.Aborted += (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHostProxy.Restarting += (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHostProxy.Restarted += (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method UnSubscribeAllWcfServiceHostProxyEvent.
        /// </summary>
        private void UnSubscribeAllWcfServiceHostProxyEvent()
        {
            this._wcfServiceHostProxy.Created -= (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHostProxy.Opening -= (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHostProxy.Opened -= (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHostProxy.Closing -= (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHostProxy.Closed -= (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHostProxy.Aborting -= (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHostProxy.Aborted -= (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHostProxy.Restarting -= (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHostProxy.Restarted -= (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method DomainExit.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void DomainExit(object sender, EventArgs e)
        {
            this.CleanTempWcfConfigFile();
        }

        /// <summary>
        /// Clean up temp wcf config file.
        /// </summary>
        private void CleanTempWcfConfigFile()
        {
            if (File.Exists(this._tempConfigFile))
            {
                try
                {
                    File.Delete(this._tempConfigFile);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        private T CloneDeep<T>(T source)
        {
            if (source == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceModel.WcfServiceHost");
            }
        }
    }
}
