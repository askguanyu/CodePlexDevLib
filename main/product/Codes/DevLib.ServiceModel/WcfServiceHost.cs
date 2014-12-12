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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, string configFile, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, configFile, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, string configFile, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, configFile, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, bindingType, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, bindingType, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Binding binding, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, binding, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Binding binding, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, binding, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Type bindingType, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, contractType, bindingType, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="port">Wcf service address port.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Type bindingType, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, contractType, bindingType, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Binding binding, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, contractType, binding, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="port">Wcf service address port.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(string assemblyFile, Type contractType, Binding binding, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(assemblyFile, contractType, binding, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, string configFile, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, configFile, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, string configFile, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, configFile, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, bindingType, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, bindingType, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Binding binding, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, binding, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Binding binding, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, binding, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Type bindingType, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, contractType, bindingType, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="port">Wcf service address port.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Type bindingType, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, contractType, bindingType, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Binding binding, string baseAddress, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, contractType, binding, baseAddress);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

            if (openNow)
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
        /// <param name="port">Wcf service address port.</param>
        /// <param name="openNow">true if immediately open wcf service; otherwise, false.</param>
        /// <param name="setServiceCredentialsAction">A delegate to configure ServiceCredentials.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractResolverAction.</param>
        public WcfServiceHost(Type serviceType, Type contractType, Binding binding, int port, bool openNow = false, Action<ServiceCredentials> setServiceCredentialsAction = null, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction = null)
        {
            this.Initialize(serviceType, contractType, binding, port);

            this.SetServiceCredentialsAction = setServiceCredentialsAction;

            this.SetDataContractResolverAction = setDataContractResolverAction;

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
        public event EventHandler<WcfServiceHostEventArgs> Receiving;

        /// <summary>
        /// Occurs before send reply.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Replying;

        /// <summary>
        /// Gets a value indicating whether service host is opened or not.
        /// </summary>
        public bool IsOpened
        {
            get;
            private set;
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
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

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
        public void Initialize(string assemblyFile, int port)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(WcfServiceType.LoadFile(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

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
        public void Initialize(string assemblyFile, string configFile, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(WcfServiceType.LoadFile(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();
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
        public void Initialize(string assemblyFile, Type bindingType, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(WcfServiceType.LoadFile(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._bindingType = bindingType;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

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
        public void Initialize(string assemblyFile, Binding binding, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(WcfServiceType.LoadFile(assemblyFile)[0])[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified file.", "assemblyFile");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._binding = binding;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

            this._isInitialized = true;
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

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractType = contractType;
            this._bindingType = bindingType;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(string assemblyFile, Type contractType, Type bindingType, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractType = contractType;
            this._bindingType = bindingType;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractType.FullName).ToString();

            this._isInitialized = true;
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

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractType = contractType;
            this._binding = binding;
            this._baseAddress = baseAddress;

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(string assemblyFile, Type contractType, Binding binding, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            this._assemblyFile = Path.GetFullPath(assemblyFile);
            this._contractType = contractType;
            this._binding = binding;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractType.FullName).ToString();

            this._isInitialized = true;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(Type serviceType, string baseAddress)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (string.IsNullOrEmpty(baseAddress) || !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress ?? string.Empty);
            }

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
        public void Initialize(Type serviceType, int port)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._bindingType = typeof(BasicHttpBinding);
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

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
        public void Initialize(Type serviceType, string configFile, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._configFile = Path.GetFullPath(configFile);
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();
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

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(Type serviceType, Type bindingType, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._bindingType = bindingType;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

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

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(Type serviceType, Binding binding, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string contractName = string.Empty;

            try
            {
                contractName = WcfServiceType.GetServiceContract(serviceType)[0].FullName;
            }
            catch
            {
                throw new ArgumentException("Cannot get contract type full name from the specified service type.", "serviceType");
            }

            this._serviceType = serviceType;
            this._binding = binding;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractName).ToString();

            this._isInitialized = true;
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

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(Type serviceType, Type contractType, Type bindingType, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            this._serviceType = serviceType;
            this._contractType = contractType;
            this._bindingType = bindingType;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractType.FullName).ToString();

            this._isInitialized = true;
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

            this._isInitialized = true;
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="contractType">Wcf contract type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="port">Wcf service address port.</param>
        public void Initialize(Type serviceType, Type contractType, Binding binding, int port)
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

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            this._serviceType = serviceType;
            this._contractType = contractType;
            this._binding = binding;
            this._baseAddress = new UriBuilder(Uri.UriSchemeHttp, "localhost", port, contractType.FullName).ToString();

            this._isInitialized = true;
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
                            this.RaiseEvent(this.Opening, serviceHost.Description.Name, WcfServiceHostState.Opening);

                            serviceHost.Open();

                            this.RaiseEvent(this.Opened, serviceHost.Description.Name, WcfServiceHostState.Opened);

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
                    this.RaiseEvent(this.Closing, serviceHost.Description.Name, WcfServiceHostState.Closing);

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

                    this.RaiseEvent(this.Closed, serviceHost.Description.Name, WcfServiceHostState.Closed);
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
                    this.RaiseEvent(this.Aborting, serviceHost.Description.Name, WcfServiceHostState.Aborting);

                    try
                    {
                        serviceHost.Abort();

                        InternalLogger.Log(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    this.RaiseEvent(this.Aborted, serviceHost.Description.Name, WcfServiceHostState.Aborted);
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
                            this.RaiseEvent(this.Restarting, serviceHost.Description.Name, WcfServiceHostState.Restarting);

                            serviceHost.Open();

                            this.RaiseEvent(this.Restarted, serviceHost.Description.Name, WcfServiceHostState.Restarted);

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
        /// <param name="wcfServiceName">String of Wcf Service Name.</param>
        /// <param name="state">Instance of WcfServiceHostState.</param>
        /// <param name="message">The message.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, string wcfServiceName, WcfServiceHostState state, string message = "")
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(wcfServiceName, state, message));
            }
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="wcfServiceName">String of Wcf Service Name.</param>
        /// <param name="state">Instance of WcfServiceHostState.</param>
        /// <param name="message">The message.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, object sender, string wcfServiceName, WcfServiceHostState state, string message = "")
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(sender, new WcfServiceHostEventArgs(wcfServiceName, state, message));
            }
        }

        /// <summary>
        /// Method InitWcfServiceHostProxy.
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
                    serviceTypeList = WcfServiceType.LoadFile(this._assemblyFile, this._configFile);
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

                            WcfServiceHostProxy serviceHost = string.IsNullOrEmpty(this._baseAddress) ? new WcfServiceHostProxy(serviceType) : new WcfServiceHostProxy(serviceType, new Uri(this._baseAddress));

                            if (this.SetServiceCredentialsAction != null)
                            {
                                this.SetServiceCredentialsAction(serviceHost.Credentials);
                            }

                            WcfServiceHostServiceBehavior wcfServiceHostServiceBehavior = serviceHost.Description.Behaviors.Find<WcfServiceHostServiceBehavior>();

                            if (wcfServiceHostServiceBehavior == null)
                            {
                                wcfServiceHostServiceBehavior = new WcfServiceHostServiceBehavior();
                                wcfServiceHostServiceBehavior.Receiving += (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying += (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);

                                serviceHost.Description.Behaviors.Add(wcfServiceHostServiceBehavior);
                            }
                            else
                            {
                                wcfServiceHostServiceBehavior.Receiving -= (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying -= (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);

                                wcfServiceHostServiceBehavior.Receiving += (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying += (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);
                            }

                            if (this.SetDataContractResolverAction != null)
                            {
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

                                        this.SetDataContractResolverAction(serializerBehavior);
                                    }
                                }
                            }

                            this._serviceHostList.Add(serviceHost);

                            this.RaiseEvent(this.Created, this._assemblyFile ?? this._serviceType.Name, WcfServiceHostState.Created);

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
                    serviceTypeList = WcfServiceType.LoadFile(this._assemblyFile);
                }
                else
                {
                    serviceTypeList = new Type[1] { this._serviceType };
                }

                Uri baseAddressUri = new Uri(this._baseAddress);

                Binding binding = this._binding ?? WcfServiceType.GetBinding(this._bindingType);

                IList<Type> contractList = null;

                if (this._contractType != null)
                {
                    contractList = new Type[1] { this._contractType };
                }

                lock (ServiceHostSyncRoot)
                {
                    WcfServiceHostProxy.SetConfigFile(null);

                    try
                    {
                        foreach (var serviceType in serviceTypeList)
                        {
                            if (this._contractType == null)
                            {
                                contractList = WcfServiceType.GetServiceContract(serviceType);
                            }

                            WcfServiceHostProxy serviceHost = new WcfServiceHostProxy(serviceType, baseAddressUri);

                            serviceHost.Description.Endpoints.Clear();

                            foreach (Type serviceContract in contractList)
                            {
                                serviceHost.AddServiceEndpoint(serviceContract, binding, baseAddressUri);
                            }

                            if (this.SetServiceCredentialsAction != null)
                            {
                                this.SetServiceCredentialsAction(serviceHost.Credentials);
                            }

                            WcfServiceHostServiceBehavior wcfServiceHostServiceBehavior = serviceHost.Description.Behaviors.Find<WcfServiceHostServiceBehavior>();

                            if (wcfServiceHostServiceBehavior == null)
                            {
                                wcfServiceHostServiceBehavior = new WcfServiceHostServiceBehavior();
                                wcfServiceHostServiceBehavior.Receiving += (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying += (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);

                                serviceHost.Description.Behaviors.Add(wcfServiceHostServiceBehavior);
                            }
                            else
                            {
                                wcfServiceHostServiceBehavior.Receiving -= (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying -= (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);

                                wcfServiceHostServiceBehavior.Receiving += (s, e) => this.RaiseEvent(this.Receiving, s, serviceHost.Description.Name, e.State, e.Message);
                                wcfServiceHostServiceBehavior.Replying += (s, e) => this.RaiseEvent(this.Replying, s, serviceHost.Description.Name, e.State, e.Message);
                            }

                            ServiceDebugBehavior serviceDebugBehavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

                            if (serviceDebugBehavior == null)
                            {
                                serviceDebugBehavior = new ServiceDebugBehavior();
                                serviceDebugBehavior.IncludeExceptionDetailInFaults = true;

                                serviceHost.Description.Behaviors.Add(serviceDebugBehavior);
                            }
                            else
                            {
                                serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
                            }

                            ServiceThrottlingBehavior serviceThrottlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();

                            if (serviceThrottlingBehavior == null)
                            {
                                serviceThrottlingBehavior = new ServiceThrottlingBehavior();
                                serviceThrottlingBehavior.MaxConcurrentCalls = int.MaxValue;
                                serviceThrottlingBehavior.MaxConcurrentInstances = int.MaxValue;
                                serviceThrottlingBehavior.MaxConcurrentSessions = int.MaxValue;

                                serviceHost.Description.Behaviors.Add(serviceThrottlingBehavior);
                            }
                            else
                            {
                                serviceThrottlingBehavior.MaxConcurrentCalls = int.MaxValue;
                                serviceThrottlingBehavior.MaxConcurrentInstances = int.MaxValue;
                                serviceThrottlingBehavior.MaxConcurrentSessions = int.MaxValue;
                            }

                            if (baseAddressUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                            {
                                ServiceMetadataBehavior serviceMetadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                                if (serviceMetadataBehavior == null)
                                {
                                    serviceMetadataBehavior = new ServiceMetadataBehavior();
                                    serviceMetadataBehavior.HttpGetEnabled = true;

                                    serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                                }
                                else
                                {
                                    serviceMetadataBehavior.HttpGetEnabled = true;
                                }
                            }

                            foreach (var endpoint in serviceHost.Description.Endpoints)
                            {
                                foreach (var operationDescription in endpoint.Contract.Operations)
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

                            this._serviceHostList.Add(serviceHost);

                            this.RaiseEvent(this.Created, this._assemblyFile ?? this._serviceType.Name, WcfServiceHostState.Created);

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
        /// Method CheckInitialized.
        /// </summary>
        private void CheckInitialized()
        {
            if (!this._isInitialized)
            {
                throw new InvalidOperationException("WcfServiceHost is not initialized.");
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
