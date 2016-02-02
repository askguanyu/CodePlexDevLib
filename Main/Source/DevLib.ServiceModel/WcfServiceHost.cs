//-----------------------------------------------------------------------
// <copyright file="WcfServiceHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Net;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Class WcfServiceHost.
    /// </summary>
    [Serializable]
    public class WcfServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field ServiceHostSyncRoot.
        /// </summary>
        private static readonly object ServiceHostSyncRoot = new object();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _assemblyFile.
        /// </summary>
        private string _assemblyFile = null;

        /// <summary>
        /// Field _serviceType.
        /// </summary>
        private Type _serviceType = null;

        /// <summary>
        /// Field _serviceInstance.
        /// </summary>
        [NonSerialized]
        private object _serviceInstance = null;

        /// <summary>
        /// Field _contractTypes.
        /// </summary>
        private Type[] _contractTypes = null;

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
        private string _tempConfigFile = null;

        /// <summary>
        /// Field _isInitialized.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Field _serviceHostList.
        /// </summary>
        private List<ServiceHost> _serviceHostList = new List<ServiceHost>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// Default constructor of WcfServiceHost. Use Initialize method to initialize Wcf service.
        /// </summary>
        public WcfServiceHost()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, string configFile, string baseAddress, bool openNow = false)
        {
            this.Initialize(assemblyFile, configFile, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, string configFile, int port, string path = null, bool openNow = false)
        {
            this.Initialize(assemblyFile, configFile, port, path);

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, bindingType, port, path);

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, binding, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type[] contractTypes, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, contractTypes, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type[] contractTypes, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, contractTypes, bindingType, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type[] contractTypes, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, contractTypes, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type[] contractTypes, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(assemblyFile, contractTypes, binding, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, port, path);

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, string configFile, string baseAddress, bool openNow = false)
        {
            this.Initialize(serviceType, configFile, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, string configFile, int port, string path = null, bool openNow = false)
        {
            this.Initialize(serviceType, configFile, port, path);

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, bindingType, port, path);

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, binding, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type[] contractTypes, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, contractTypes, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type[] contractTypes, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, contractTypes, bindingType, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type[] contractTypes, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, contractTypes, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type[] contractTypes, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(serviceType, contractTypes, binding, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(object singletonInstance, string configFile, string baseAddress, bool openNow = false)
        {
            this.Initialize(singletonInstance, configFile, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(object singletonInstance, string configFile, int port, string path = null, bool openNow = false)
        {
            this.Initialize(singletonInstance, configFile, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, bindingType, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, binding, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type[] contractTypes, Type bindingType, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, contractTypes, bindingType, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type[] contractTypes, Type bindingType, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, contractTypes, bindingType, port, path);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type[] contractTypes, Binding binding, string baseAddress, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, contractTypes, binding, baseAddress);

            if (openNow)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(object singletonInstance, Type[] contractTypes, Binding binding, int port, string path = null, bool openNow = false, Action<Binding> setBindingAction = null, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.SetBindingAction = setBindingAction;
            this.SetServiceCredentialsAction = setServiceCredentialsAction;
            this.SetDataContractResolverAction = setDataContractResolverAction;

            this.Initialize(singletonInstance, contractTypes, binding, port, path);

            if (openNow)
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
        public event EventHandler<WcfServiceHostEventArgs> Created;

        /// <summary>
        /// Event Opening.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opening;

        /// <summary>
        /// Event Opened.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opened;

        /// <summary>
        /// Event Closing.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closing;

        /// <summary>
        /// Event Closed.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closed;

        /// <summary>
        /// Event Aborting.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborting;

        /// <summary>
        /// Event Aborted.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborted;

        /// <summary>
        /// Event Restarting.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Restarting;

        /// <summary>
        /// Event Restarted.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Restarted;

        /// <summary>
        /// Occurs after receive request.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> ReceivingRequest;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfMessageInspectorEventArgs> SendingReply;

        /// <summary>
        /// Occurs when has error.
        /// </summary>
        public event EventHandler<WcfErrorEventArgs> ErrorOccurred;

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
        /// Gets a value indicating whether service host is opened or not.
        /// </summary>
        public bool IsOpened
        {
            get;
            private set;
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
        /// Gets or sets a delegate to configure ServiceCredentials.
        /// </summary>
        public Action<ServiceCredentials> SetServiceCredentialsAction
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
        /// Gets or sets a delegate to configure WebHttpBehavior.
        /// </summary>
        public Action<WebHttpBehavior> SetWebHttpBehaviorAction
        {
            get;
            set;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(WcfServiceUtilities.LoadWcfTypes(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, string configFile, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckConfigFile(configFile);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = baseAddress;
            this._tempConfigFile = this.GetTempWcfConfigFile(this._configFile, this._baseAddress);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, string configFile, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckConfigFile(configFile);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(WcfServiceUtilities.LoadWcfTypes(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = this.BuildUri(port, contractName, path);
            this._tempConfigFile = this.GetTempWcfConfigFile(this._configFile, this._baseAddress);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type bindingType, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckBindingType(bindingType);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, Type bindingType, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckBindingType(bindingType);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(WcfServiceUtilities.LoadWcfTypes(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = bindingType;
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Binding binding, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckBindingInstance(binding);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._binding = binding;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, Binding binding, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckBindingInstance(binding);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(WcfServiceUtilities.LoadWcfTypes(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._binding = binding;
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type[] contractTypes, Type bindingType, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingType(bindingType);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractTypes = contractTypes;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, Type[] contractTypes, Type bindingType, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckBindingType(bindingType);
            this.CheckContractTypes(contractTypes);
            this.CheckPort(port);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractTypes = contractTypes;
            this._bindingType = bindingType;
            this._baseAddress = this.BuildUri(port, contractTypes[0].FullName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, Type[] contractTypes, Binding binding, string baseAddress)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingInstance(binding);
            this.CheckUri(baseAddress);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractTypes = contractTypes;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(string assemblyFile, Type[] contractTypes, Binding binding, int port, string path)
        {
            this.CheckAssemblyFile(assemblyFile);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingInstance(binding);
            this.CheckPort(port);

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractTypes = contractTypes;
            this._binding = binding;
            this._baseAddress = this.BuildUri(port, contractTypes[0].FullName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, string configFile, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckConfigFile(configFile);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = baseAddress;
            this._tempConfigFile = this.GetTempWcfConfigFile(this._configFile, this._baseAddress);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, string configFile, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckConfigFile(configFile);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = this.BuildUri(port, contractName, path);
            this._tempConfigFile = this.GetTempWcfConfigFile(this._configFile, this._baseAddress);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type bindingType, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckBindingType(bindingType);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, Type bindingType, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckBindingType(bindingType);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._bindingType = bindingType;
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Binding binding, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckBindingInstance(binding);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, Binding binding, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckBindingInstance(binding);
            this.CheckPort(port);

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceUtilities.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._binding = binding;
            this._baseAddress = this.BuildUri(port, contractName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type[] contractTypes, Type bindingType, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingType(bindingType);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._contractTypes = contractTypes;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, Type[] contractTypes, Type bindingType, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingType(bindingType);
            this.CheckPort(port);

            this._serviceType = serviceType;
            this._contractTypes = contractTypes;
            this._bindingType = bindingType;
            this._baseAddress = this.BuildUri(port, contractTypes[0].FullName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, Type[] contractTypes, Binding binding, string baseAddress)
        {
            this.CheckServiceType(serviceType);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingInstance(binding);
            this.CheckUri(baseAddress);

            this._serviceType = serviceType;
            this._contractTypes = contractTypes;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(Type serviceType, Type[] contractTypes, Binding binding, int port, string path)
        {
            this.CheckServiceType(serviceType);
            this.CheckContractTypes(contractTypes);
            this.CheckBindingInstance(binding);
            this.CheckPort(port);

            this._serviceType = serviceType;
            this._contractTypes = contractTypes;
            this._binding = binding;
            this._baseAddress = this.BuildUri(port, contractTypes[0].FullName, path);

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), baseAddress);
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), port, path);
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, string configFile, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), configFile, baseAddress);
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, string configFile, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), configFile, port, path);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, Type bindingType, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), bindingType, baseAddress);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, Type bindingType, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), bindingType, port, path);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, Binding binding, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), binding, baseAddress);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, Binding binding, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), binding, port, path);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, Type[] contractTypes, Type bindingType, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), baseAddress);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, Type[] contractTypes, Type bindingType, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), contractTypes, bindingType, port, path);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(object singletonInstance, Type[] contractTypes, Binding binding, string baseAddress)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), contractTypes, binding, baseAddress);
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        /// <param name="path">The BaseAddress Uri path.</param>
        public void Initialize(object singletonInstance, Type[] contractTypes, Binding binding, int port, string path)
        {
            this.CheckServiceInstance(singletonInstance);

            this._serviceInstance = singletonInstance;

            this.Initialize(singletonInstance.GetType(), contractTypes, binding, port, path);
        }

        /// <summary>
        /// Open Wcf service.
        /// </summary>
        public void Open()
        {
            this.CheckDisposed();

            this.CheckInitialized();

            if (this.IsOpened)
            {
                return;
            }

            this.InitWcfServiceHostProxy();

            if (this._serviceHostList.Count > 0)
            {
                try
                {
                    foreach (ServiceHost serviceHost in this._serviceHostList)
                    {
                        if (!(serviceHost.State == CommunicationState.Opening || serviceHost.State == CommunicationState.Opened))
                        {
                            this.RaiseEvent(this.Opening, serviceHost, WcfServiceHostState.Opening);

                            serviceHost.Open();

                            this.RaiseEvent(this.Opened, serviceHost, WcfServiceHostState.Opened);

                            InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Open", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }

                    this.IsOpened = true;
                }
                catch (Exception e)
                {
                    this.IsOpened = false;

                    InternalLogger.Log(e);

                    throw;
                }
            }
            else
            {
                this.IsOpened = false;
            }
        }

        /// <summary>
        /// Close Wcf service.
        /// </summary>
        public void Close()
        {
            this.CheckDisposed();

            this.CheckInitialized();

            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(this.Closing, serviceHost, WcfServiceHostState.Closing);

                    try
                    {
                        serviceHost.Close();

                        InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Close", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        serviceHost.Abort();

                        InternalLogger.Log(e);
                    }

                    this.RaiseEvent(this.Closed, serviceHost, WcfServiceHostState.Closed);
                }
            }

            this.IsOpened = false;
        }

        /// <summary>
        /// Abort Wcf service.
        /// </summary>
        public void Abort()
        {
            this.CheckDisposed();

            this.CheckInitialized();

            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(this.Aborting, serviceHost, WcfServiceHostState.Aborting);

                    try
                    {
                        serviceHost.Abort();

                        InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    this.RaiseEvent(this.Aborted, serviceHost, WcfServiceHostState.Aborted);
                }
            }

            this.IsOpened = false;
        }

        /// <summary>
        /// Restart Wcf service.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            this.CheckInitialized();

            this.InitWcfServiceHostProxy();

            if (this._serviceHostList.Count > 0)
            {
                try
                {
                    foreach (ServiceHost serviceHost in this._serviceHostList)
                    {
                        if (!(serviceHost.State == CommunicationState.Opening || serviceHost.State == CommunicationState.Opened))
                        {
                            this.RaiseEvent(this.Restarting, serviceHost, WcfServiceHostState.Restarting);

                            serviceHost.Open();

                            this.RaiseEvent(this.Restarted, serviceHost, WcfServiceHostState.Restarted);

                            InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Restart", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }

                    this.IsOpened = true;
                }
                catch (Exception e)
                {
                    this.IsOpened = false;

                    InternalLogger.Log(e);

                    throw;
                }
            }
            else
            {
                this.IsOpened = false;
            }
        }

        /// <summary>
        /// Get Wcf service state list.
        /// </summary>
        /// <returns>Instance of List.</returns>
        public List<WcfServiceHostInfo> GetHostInfoList()
        {
            this.CheckDisposed();

            List<WcfServiceHostInfo> result = new List<WcfServiceHostInfo>();

            foreach (var item in this._serviceHostList)
            {
                result.Add(new WcfServiceHostInfo() { ServiceType = item.Description.ServiceType.FullName, BaseAddress = item.BaseAddresses[0].AbsoluteUri, State = item.State, Credentials = item.Credentials });
            }

            return result;
        }

        /// <summary>
        /// Gives the <see cref="T:System.AppDomain" /> an infinite lifetime by preventing a lease from being created.
        /// </summary>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain.</exception>
        /// <returns>Always null.</returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.AllFlags)]
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

            if (File.Exists(sourceFileName) && !string.IsNullOrEmpty(baseAddress))
            {
                try
                {
                    result = Path.GetTempFileName();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }

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
                    InternalLogger.Log(e);
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

                if (this._serviceHostList != null && this._serviceHostList.Count > 0)
                {
                    foreach (var serviceHost in this._serviceHostList)
                    {
                        if (serviceHost != null)
                        {
                            try
                            {
                                serviceHost.Close();
                            }
                            catch
                            {
                                serviceHost.Abort();
                            }
                        }
                    }

                    this._serviceHostList.Clear();

                    this._serviceHostList = null;
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
        /// <param name="serviceHost">The service host.</param>
        /// <param name="state">Instance of WcfServiceHostState.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, ServiceHostBase serviceHost, WcfServiceHostState state)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(serviceHost, state));
            }
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="serviceHost">The service host.</param>
        /// <param name="e">The <see cref="WcfMessageInspectorEventArgs" /> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfMessageInspectorEventArgs> eventHandler, ServiceEndpoint endpoint, ServiceHostBase serviceHost, WcfMessageInspectorEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfMessageInspectorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfMessageInspectorEventArgs(e.Message, e.MessageId, e.IsOneWay, e.ValidationError, endpoint, null, serviceHost));
            }
        }

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="e">The <see cref="WcfErrorEventArgs"/> instance containing the event data.</param>
        private void RaiseEvent(EventHandler<WcfErrorEventArgs> eventHandler, WcfErrorEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler<WcfErrorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Initializes the WCF service host proxy.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private void InitWcfServiceHostProxy()
        {
            if (this._serviceHostList.Count > 0)
            {
                foreach (ServiceHost serviceHost in this._serviceHostList)
                {
                    try
                    {
                        serviceHost.Abort();
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
            }

            this._serviceHostList.Clear();

            IList<Type> serviceTypeList = null;

            if (File.Exists(this._configFile))
            {
                if (File.Exists(this._assemblyFile))
                {
                    serviceTypeList = WcfServiceUtilities.LoadWcfTypes(this._assemblyFile, this._configFile);
                }
                else
                {
                    serviceTypeList = new Type[1] { this._serviceType };
                }

                lock (ServiceHostSyncRoot)
                {
                    try
                    {
                        foreach (Type serviceType in serviceTypeList)
                        {
                            WcfServiceHostProxy.SetConfigFile(this._tempConfigFile ?? this._configFile);

                            WcfServiceHostProxy serviceHost = null;

                            if (this._serviceInstance == null)
                            {
                                serviceHost = string.IsNullOrEmpty(this._baseAddress) ? new WcfServiceHostProxy(serviceType) : new WcfServiceHostProxy(serviceType, new Uri(this._baseAddress));
                            }
                            else
                            {
                                serviceHost = string.IsNullOrEmpty(this._baseAddress) ? new WcfServiceHostProxy(this._serviceInstance) : new WcfServiceHostProxy(this._serviceInstance, new Uri(this._baseAddress));
                            }

                            if (this.SetServiceCredentialsAction != null)
                            {
                                this.SetServiceCredentialsAction(serviceHost.Credentials);
                            }

                            foreach (var endpoint in serviceHost.Description.Endpoints)
                            {
                                foreach (var operationDescription in endpoint.Contract.Operations)
                                {
                                    if (this.SetDataContractResolverAction != null)
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

                                if (this.SetBindingAction != null)
                                {
                                    this.SetBindingAction(endpoint.Binding);
                                }

                                if (endpoint.Binding is WebHttpBinding)
                                {
                                    WebHttpBehavior webHttpBehavior = endpoint.Behaviors.Find<WebHttpBehavior>();

                                    if (webHttpBehavior == null)
                                    {
                                        webHttpBehavior = new WebHttpBehavior();
                                        endpoint.Behaviors.Add(webHttpBehavior);
                                    }

                                    if (this.SetWebHttpBehaviorAction != null)
                                    {
                                        this.SetWebHttpBehaviorAction(webHttpBehavior);
                                    }
                                }

                                WcfMessageInspectorEndpointBehavior wcfMessageInspectorEndpointBehavior = endpoint.Behaviors.Find<WcfMessageInspectorEndpointBehavior>();

                                if (wcfMessageInspectorEndpointBehavior == null)
                                {
                                    wcfMessageInspectorEndpointBehavior = new WcfMessageInspectorEndpointBehavior(serviceHost);

                                    wcfMessageInspectorEndpointBehavior.IgnoreMessageInspect = this.IgnoreMessageInspect;
                                    wcfMessageInspectorEndpointBehavior.IgnoreMessageValidate = this.IgnoreMessageValidate;

                                    wcfMessageInspectorEndpointBehavior.ReceivingRequest += (s, e) => this.RaiseEvent(this.ReceivingRequest, endpoint, serviceHost, e);
                                    wcfMessageInspectorEndpointBehavior.SendingReply += (s, e) => this.RaiseEvent(this.SendingReply, endpoint, serviceHost, e);
                                    wcfMessageInspectorEndpointBehavior.ErrorOccurred += (s, e) => this.RaiseEvent(this.ErrorOccurred, e);

                                    endpoint.Behaviors.Add(wcfMessageInspectorEndpointBehavior);
                                }
                            }

                            this._serviceHostList.Add(serviceHost);

                            this.RaiseEvent(this.Created, serviceHost, WcfServiceHostState.Created);

                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.InitWcfServiceHostProxy", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        throw;
                    }
                }
            }
            else
            {
                if (File.Exists(this._assemblyFile))
                {
                    serviceTypeList = WcfServiceUtilities.LoadWcfTypes(this._assemblyFile);
                }
                else
                {
                    serviceTypeList = new Type[1] { this._serviceType };
                }

                Uri baseAddressUri = new Uri(this._baseAddress);

                Binding binding = this._binding ?? WcfBinding.GetBinding(this._bindingType);

                if (this.SetBindingAction != null)
                {
                    this.SetBindingAction(binding);
                }

                lock (ServiceHostSyncRoot)
                {
                    WcfServiceHostProxy.SetConfigFile(null);

                    try
                    {
                        foreach (Type serviceType in serviceTypeList)
                        {
                            IList<Type> contractList = this._contractTypes;

                            if (contractList == null)
                            {
                                contractList = WcfServiceUtilities.GetServiceContract(serviceType);
                            }

                            WcfServiceHostProxy serviceHost = this._serviceInstance == null ? new WcfServiceHostProxy(serviceType, baseAddressUri) : new WcfServiceHostProxy(this._serviceInstance, baseAddressUri);

                            serviceHost.Description.Endpoints.Clear();

                            foreach (Type serviceContract in contractList)
                            {
                                serviceHost.AddServiceEndpoint(serviceContract, binding, baseAddressUri);
                            }

                            if (this.SetServiceCredentialsAction != null)
                            {
                                this.SetServiceCredentialsAction(serviceHost.Credentials);
                            }

                            ServiceDebugBehavior serviceDebugBehavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

                            if (serviceDebugBehavior == null)
                            {
                                serviceDebugBehavior = new ServiceDebugBehavior();
                                serviceHost.Description.Behaviors.Add(serviceDebugBehavior);
                            }

                            serviceDebugBehavior.IncludeExceptionDetailInFaults = true;

                            ServiceThrottlingBehavior serviceThrottlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();

                            if (serviceThrottlingBehavior == null)
                            {
                                serviceThrottlingBehavior = new ServiceThrottlingBehavior();
                                serviceHost.Description.Behaviors.Add(serviceThrottlingBehavior);
                            }

                            serviceThrottlingBehavior.MaxConcurrentCalls = int.MaxValue;
                            serviceThrottlingBehavior.MaxConcurrentInstances = int.MaxValue;
                            serviceThrottlingBehavior.MaxConcurrentSessions = int.MaxValue;

                            if (baseAddressUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                            {
                                ServiceMetadataBehavior serviceMetadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                                if (serviceMetadataBehavior == null)
                                {
                                    serviceMetadataBehavior = new ServiceMetadataBehavior();
                                    serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                                }

                                serviceMetadataBehavior.HttpGetEnabled = true;
                            }

                            if (baseAddressUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                            {
                                ServiceMetadataBehavior serviceMetadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                                if (serviceMetadataBehavior == null)
                                {
                                    serviceMetadataBehavior = new ServiceMetadataBehavior();
                                    serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                                }

                                serviceMetadataBehavior.HttpGetEnabled = true;
                                serviceMetadataBehavior.HttpsGetEnabled = true;
                            }

                            foreach (var endpoint in serviceHost.Description.Endpoints)
                            {
                                foreach (var operationDescription in endpoint.Contract.Operations)
                                {
                                    DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                                    if (serializerBehavior == null)
                                    {
                                        serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription);
                                        operationDescription.Behaviors.Add(serializerBehavior);
                                    }

                                    serializerBehavior.MaxItemsInObjectGraph = int.MaxValue;
                                    serializerBehavior.IgnoreExtensionDataObject = true;

                                    if (this.SetDataContractResolverAction != null)
                                    {
                                        this.SetDataContractResolverAction(serializerBehavior);
                                    }
                                }

                                if (endpoint.Binding is WebHttpBinding)
                                {
                                    WebHttpBehavior webHttpBehavior = endpoint.Behaviors.Find<WebHttpBehavior>();

                                    if (webHttpBehavior == null)
                                    {
                                        webHttpBehavior = new WebHttpBehavior();
                                        endpoint.Behaviors.Add(webHttpBehavior);
                                    }

                                    if (this.SetWebHttpBehaviorAction != null)
                                    {
                                        this.SetWebHttpBehaviorAction(webHttpBehavior);
                                    }
                                }

                                WcfMessageInspectorEndpointBehavior wcfMessageInspectorEndpointBehavior = endpoint.Behaviors.Find<WcfMessageInspectorEndpointBehavior>();

                                if (wcfMessageInspectorEndpointBehavior == null)
                                {
                                    wcfMessageInspectorEndpointBehavior = new WcfMessageInspectorEndpointBehavior(serviceHost);

                                    wcfMessageInspectorEndpointBehavior.IgnoreMessageInspect = this.IgnoreMessageInspect;
                                    wcfMessageInspectorEndpointBehavior.IgnoreMessageValidate = this.IgnoreMessageValidate;

                                    wcfMessageInspectorEndpointBehavior.ReceivingRequest += (s, e) => this.RaiseEvent(this.ReceivingRequest, endpoint, serviceHost, e);
                                    wcfMessageInspectorEndpointBehavior.SendingReply += (s, e) => this.RaiseEvent(this.SendingReply, endpoint, serviceHost, e);
                                    wcfMessageInspectorEndpointBehavior.ErrorOccurred += (s, e) => this.RaiseEvent(this.ErrorOccurred, e);

                                    endpoint.Behaviors.Add(wcfMessageInspectorEndpointBehavior);
                                }
                            }

                            this._serviceHostList.Add(serviceHost);

                            this.RaiseEvent(this.Created, serviceHost, WcfServiceHostState.Created);

                            InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.InitWcfServiceHostProxy", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the URI.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="path">The path.</param>
        /// <returns>Uri string.</returns>
        private string BuildUri(int port, string contractName, string path)
        {
            return new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName + (this.IsNullOrWhiteSpace(path) ? null : "/" + path.Trim('/'))).ToString();
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
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Checks the assembly file.
        /// </summary>
        /// <param name="assemblyFile">The assembly file to check..</param>
        private void CheckAssemblyFile(string assemblyFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified assembly file does not exist.", assemblyFile);
            }
        }

        /// <summary>
        /// Checks the type of the service.
        /// </summary>
        /// <param name="serviceType">Type of the service to check.</param>
        private void CheckServiceType(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceUtilities.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException(string.Format("The parameter serviceType {0} is not a Wcf service.", serviceType.FullName), "serviceType");
            }
        }

        /// <summary>
        /// Checks the service instance.
        /// </summary>
        /// <param name="singletonInstance">The singleton instance to check.</param>
        private void CheckServiceInstance(object singletonInstance)
        {
            if (singletonInstance == null)
            {
                throw new ArgumentNullException("singletonInstance");
            }
        }

        /// <summary>
        /// Checks the configuration file.
        /// </summary>
        /// <param name="configFile">The configuration file to check.</param>
        private void CheckConfigFile(string configFile)
        {
            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("The specified config file does not exist.", configFile);
            }
        }

        /// <summary>
        /// Checks the type of the binding.
        /// </summary>
        /// <param name="bindingType">Type of the binding to check.</param>
        private void CheckBindingType(Type bindingType)
        {
            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException(string.Format("The parameter bindingType {0} is not a System.ServiceModel.Channels.Binding type.", bindingType.FullName), "bindingType");
            }
        }

        /// <summary>
        /// Checks the binding instance.
        /// </summary>
        /// <param name="binding">The binding to check.</param>
        private void CheckBindingInstance(Binding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }
        }

        /// <summary>
        /// Checks the contract types.
        /// </summary>
        /// <param name="contractTypes">The contract types to check.</param>
        private void CheckContractTypes(Type[] contractTypes)
        {
            if (contractTypes == null || contractTypes.Length == 0)
            {
                throw new ArgumentNullException("contractTypes");
            }

            string errorContracts = string.Empty;

            foreach (var item in contractTypes)
            {
                if (!WcfServiceUtilities.HasServiceContractAttribute(item))
                {
                    errorContracts += item.FullName;
                    errorContracts += " ";
                }
            }

            if (!string.IsNullOrEmpty(errorContracts))
            {
                throw new ArgumentException(string.Format("The parameter contractTypes has none Wcf contracts: {0}", errorContracts), "contractTypes");
            }
        }

        /// <summary>
        /// Checks the port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        private void CheckPort(int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }
        }

        /// <summary>
        /// Checks the URI.
        /// </summary>
        /// <param name="baseAddress">The base address to check.</param>
        private void CheckUri(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }
        }

        /// <summary>
        /// Checks whether this host is initialized.
        /// </summary>
        private void CheckInitialized()
        {
            if (!this._isInitialized)
            {
                throw new InvalidOperationException("WcfServiceHost is not initialized.");
            }
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        private bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether this instance is disposed.
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
