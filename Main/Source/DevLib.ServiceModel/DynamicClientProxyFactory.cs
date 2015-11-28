//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyFactory.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.Design;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Web.Services.Discovery;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using WebService = System.Web.Services.Description;

    /// <summary>
    /// Represents factory class to create DynamicClientProxy.
    /// </summary>
    [Serializable]
    public class DynamicClientProxyFactory : MarshalByRefObject
    {
        /// <summary>
        /// Field DefaultNamespace.
        /// </summary>
        internal const string DefaultNamespace = "http://tempuri.org/";

        /// <summary>
        /// Field _setupInfo.
        /// </summary>
        private DynamicClientProxyFactorySetup _setupInfo;

        /// <summary>
        /// Field _codeCompileUnit.
        /// </summary>
        [NonSerialized]
        private CodeCompileUnit _codeCompileUnit;

        /// <summary>
        /// Field _codeDomProvider.
        /// </summary>
        [NonSerialized]
        private CodeDomProvider _codeDomProvider;

        /// <summary>
        /// Field _contractGenerator.
        /// </summary>
        [NonSerialized]
        private ServiceContractGenerator _contractGenerator;

        /// <summary>
        /// Field _endpoints.
        /// </summary>
        [NonSerialized]
        private IList<ServiceEndpoint> _endpoints;

        /// <summary>
        /// Field _metadata.
        /// </summary>
        [NonSerialized]
        private IList<MetadataSection> _metadata;

        /// <summary>
        /// Field _bindings.
        /// </summary>
        [NonSerialized]
        private IList<Binding> _bindings;

        /// <summary>
        /// Field _contracts.
        /// </summary>
        [NonSerialized]
        private IList<ContractDescription> _contracts;

        /// <summary>
        /// Field _metadataImportWarnings.
        /// </summary>
        [NonSerialized]
        private IList<MetadataConversionError> _metadataImportWarnings;

        /// <summary>
        /// Field _codeGenerationWarnings.
        /// </summary>
        [NonSerialized]
        private IList<MetadataConversionError> _codeGenerationWarnings;

        /// <summary>
        /// Field _compilerWarnings.
        /// </summary>
        [NonSerialized]
        private IList<CompilerError> _compilerWarnings;

        /// <summary>
        /// Field _proxyAssembly.
        /// </summary>
        [NonSerialized]
        private Assembly _proxyAssembly;

        /// <summary>
        /// Field _loadFromFile.
        /// </summary>
        private bool _loadFromFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="setupInfo">The DynamicClientProxyFactorySetup to use.</param>
        public DynamicClientProxyFactory(string url, string outputAssembly, bool overwrite, DynamicClientProxyFactorySetup setupInfo)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            if (setupInfo == null)
            {
                throw new ArgumentNullException("setupInfo");
            }

            this.Url = url;
            this._setupInfo = setupInfo;
            this._loadFromFile = false;

            this.DownloadMetadata();
            this.ImportMetadata();
            this.GenerateServiceContract();
            this.WriteCode();
            this.CompileProxy(outputAssembly, overwrite);
            this.DisposeCodeDomProvider();
            this.ImportTypes();
            this.ImportNamespaces();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        public DynamicClientProxyFactory(string url)
            : this(url, null, true, new DynamicClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="setupInfo">The DynamicClientProxyFactorySetup to use.</param>
        public DynamicClientProxyFactory(string url, DynamicClientProxyFactorySetup setupInfo)
            : this(url, null, true, setupInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        public DynamicClientProxyFactory(string url, string outputAssembly)
            : this(url, outputAssembly, true, new DynamicClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        public DynamicClientProxyFactory(string url, string outputAssembly, bool overwrite)
            : this(url, outputAssembly, overwrite, new DynamicClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="assemblyFile">Client proxy assembly file.</param>
        /// <param name="loadFromFile">true to indicate DynamicClientProxyFactory is loaded from assembly file.</param>
        private DynamicClientProxyFactory(string assemblyFile, bool loadFromFile)
        {
            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified assembly file does not exist.", assemblyFile);
            }

            this._loadFromFile = loadFromFile;

            this.ProxyAssembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
            this.ImportTypes();
            this.ImportNamespaces();
        }

        /// <summary>
        /// Gets the URL for the service address or the file containing the WSDL data.
        /// </summary>
        public string Url
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the client proxy assembly.
        /// </summary>
        public Assembly ProxyAssembly
        {
            get
            {
                return this._proxyAssembly;
            }

            private set
            {
                this._proxyAssembly = value;
            }
        }

        /// <summary>
        /// Gets the client proxy code.
        /// </summary>
        public string ProxyCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the types defined in the current client proxy assembly.
        /// </summary>
        public Type[] Types
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Metadata.
        /// </summary>
        public IList<MetadataSection> Metadata
        {
            get
            {
                return this._metadata;
            }

            private set
            {
                this._metadata = value;
            }
        }

        /// <summary>
        /// Gets all the Bindings.
        /// </summary>
        public IList<Binding> Bindings
        {
            get
            {
                return this._bindings;
            }

            private set
            {
                this._bindings = value;
            }
        }

        /// <summary>
        /// Gets all the Contracts.
        /// </summary>
        public IList<ContractDescription> Contracts
        {
            get
            {
                return this._contracts;
            }

            private set
            {
                this._contracts = value;
            }
        }

        /// <summary>
        /// Gets all the Contract types.
        /// </summary>
        public IList<Type> ContractTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the namespaces.
        /// </summary>
        public IList<string> Namespaces
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Endpoints.
        /// </summary>
        public IList<ServiceEndpoint> Endpoints
        {
            get
            {
                return this._endpoints;
            }

            private set
            {
                this._endpoints = value;
            }
        }

        /// <summary>
        /// Gets all the Metadata Import Warnings.
        /// </summary>
        public IList<MetadataConversionError> MetadataImportWarnings
        {
            get
            {
                return this._metadataImportWarnings;
            }

            private set
            {
                this._metadataImportWarnings = value;
            }
        }

        /// <summary>
        /// Gets all the Code Generation Warnings.
        /// </summary>
        public IList<MetadataConversionError> CodeGenerationWarnings
        {
            get
            {
                return this._codeGenerationWarnings;
            }

            private set
            {
                this._codeGenerationWarnings = value;
            }
        }

        /// <summary>
        /// Gets all the Compiler Warnings.
        /// </summary>
        public IList<CompilerError> CompilerWarnings
        {
            get
            {
                return this._compilerWarnings;
            }

            private set
            {
                this._compilerWarnings = value;
            }
        }

        /// <summary>
        /// Gets DynamicClientProxyFactory from client proxy assembly file.
        /// </summary>
        /// <param name="assemblyFile">Client proxy assembly file.</param>
        /// <returns>DynamicClientProxyFactory instance.</returns>
        public static DynamicClientProxyFactory Load(string assemblyFile)
        {
            return new DynamicClientProxyFactory(assemblyFile, true);
        }

        /// <summary>
        /// Creates and returns a string representation of all errors.
        /// </summary>
        /// <param name="errors">Errors to get.</param>
        /// <returns>A string representation of all errors.</returns>
        public static string GetErrorsString(IEnumerable<MetadataConversionError> errors)
        {
            if (errors != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (MetadataConversionError error in errors)
                {
                    stringBuilder.AppendLine(error.IsWarning ? "Warning : " : "Error : " + error.Message);
                }

                return stringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and returns a string representation of all errors.
        /// </summary>
        /// <param name="errors">Errors to get.</param>
        /// <returns>A string representation of all errors.</returns>
        public static string GetErrorsString(IEnumerable<CompilerError> errors)
        {
            if (errors != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (CompilerError error in errors)
                {
                    stringBuilder.AppendLine(error.ToString());
                }

                return stringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets ServiceEndpoint by Contract name.
        /// </summary>
        /// <param name="contractName">Contract name to get.</param>
        /// <returns>ServiceEndpoint instance.</returns>
        public ServiceEndpoint GetEndpoint(string contractName)
        {
            return this.GetEndpoint(contractName, null);
        }

        /// <summary>
        /// Gets ServiceEndpoint by Contract name and Contract namespace.
        /// </summary>
        /// <param name="contractName">Contract name to get.</param>
        /// <param name="contractNamespace">Contract namespace to get.</param>
        /// <returns>ServiceEndpoint instance.</returns>
        public ServiceEndpoint GetEndpoint(string contractName, string contractNamespace)
        {
            ServiceEndpoint result = null;

            foreach (ServiceEndpoint endpoint in this.Endpoints)
            {
                if (this.ContractNameMatch(endpoint.Contract, contractName) && this.ContractNamespaceMatch(endpoint.Contract, contractNamespace))
                {
                    result = endpoint;
                    break;
                }
            }

            if (result == null)
            {
                throw new ArgumentException(string.Format(DynamicClientProxyConstants.EndpointNotFoundStringFormat, contractNamespace, contractName));
            }

            return result;
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy()
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            return new DynamicClientProxy(proxyType, endpoint.Binding, endpoint.Address.ToString());
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteUri);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteHost, remotePort);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Binding binding, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], binding, remoteUri);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Binding binding, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], binding, remoteHost, remotePort);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type bindingType, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], bindingType, remoteUri);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], bindingType, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type bindingType, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetClientBaseProxy(this.ContractTypes[0], bindingType, remoteHost, remotePort);
            }
            else
            {
                return this.GetClientBaseProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, Type bindingType, string remoteUri)
        {
            return this.GetClientBaseProxy(contractName, null, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, Binding binding, string remoteUri)
        {
            return this.GetClientBaseProxy(contractName, null, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, Binding binding, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(contractName, null, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetClientBaseProxy(endpoint, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetClientBaseProxy(endpoint, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetClientBaseProxy(endpoint, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetClientBaseProxy(endpoint, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetClientBaseProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type contractType, Binding binding, string remoteUri)
        {
            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            return new DynamicClientProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetClientBaseProxy(contractType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientProxy(proxyType, bindingType, address);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientProxy(proxyType, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetClientBaseProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientProxy(proxyType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy()
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            return new DynamicClientPerCallThrowableProxy(proxyType, endpoint.Binding, endpoint.Address.ToString());
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteUri);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Binding binding, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], binding, remoteUri);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Binding binding, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], binding, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type bindingType, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], bindingType, remoteUri);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], bindingType, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type bindingType, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallThrowableProxy(this.ContractTypes[0], bindingType, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallThrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, Type bindingType, string remoteUri)
        {
            return this.GetPerCallThrowableProxy(contractName, null, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerCallThrowableProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, Binding binding, string remoteUri)
        {
            return this.GetPerCallThrowableProxy(contractName, null, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort)
        {
            return this.GetPerCallThrowableProxy(contractName, null, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetPerCallThrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerCallThrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type contractType, Binding binding, string remoteUri)
        {
            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            return new DynamicClientPerCallThrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallThrowableProxy(contractType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerCallThrowableProxy(proxyType, bindingType, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerCallThrowableProxy(proxyType, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerCallThrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerCallThrowableProxy(proxyType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy()
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            return new DynamicClientPerCallUnthrowableProxy(proxyType, endpoint.Binding, endpoint.Address.ToString());
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteUri);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Binding binding, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], binding, remoteUri);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Binding binding, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], binding, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type bindingType, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], bindingType, remoteUri);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], bindingType, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type bindingType, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerCallUnthrowableProxy(this.ContractTypes[0], bindingType, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerCallUnthrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, Type bindingType, string remoteUri)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, Binding binding, string remoteUri)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetPerCallUnthrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerCallUnthrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type contractType, Binding binding, string remoteUri)
        {
            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            return new DynamicClientPerCallUnthrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallUnthrowableProxy(contractType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerCallUnthrowableProxy(proxyType, bindingType, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerCallUnthrowableProxy(proxyType, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerCallUnthrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerCallUnthrowableProxy(proxyType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy()
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            return new DynamicClientPerSessionThrowableProxy(proxyType, endpoint.Binding, endpoint.Address.ToString());
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteUri);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Binding binding, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], binding, remoteUri);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Binding binding, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], binding, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type bindingType, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], bindingType, remoteUri);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], bindingType, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type bindingType, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionThrowableProxy(this.ContractTypes[0], bindingType, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionThrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, Type bindingType, string remoteUri)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, Binding binding, string remoteUri)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetPerSessionThrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerSessionThrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type contractType, Binding binding, string remoteUri)
        {
            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            return new DynamicClientPerSessionThrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionThrowableProxy(contractType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerSessionThrowableProxy(proxyType, bindingType, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerSessionThrowableProxy(proxyType, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerSessionThrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerSessionThrowableProxy(proxyType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy()
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, endpoint.Binding, endpoint.Address.ToString());
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteUri);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], typeof(BasicHttpBinding), remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Binding binding, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], binding, remoteUri);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], binding, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Binding binding, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], binding, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type bindingType, string remoteUri)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], bindingType, remoteUri);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], bindingType, remoteUri);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type bindingType, string remoteHost, int remotePort)
        {
            if (this._loadFromFile)
            {
                return this.GetPerSessionUnthrowableProxy(this.ContractTypes[0], bindingType, remoteHost, remotePort);
            }
            else
            {
                return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
            }
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, Type bindingType, string remoteUri)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, Binding binding, string remoteUri)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetPerSessionUnthrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetPerSessionUnthrowableProxy(contractType, WcfServiceUtilities.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type contractType, Binding binding, string remoteUri)
        {
            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionUnthrowableProxy(contractType, binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, bindingType, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, binding, address);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxyBase.</returns>
        public DynamicClientProxyBase GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            Type proxyType = this.GetProxyType(contractType);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return new DynamicClientPerSessionUnthrowableProxy(proxyType, binding, remoteUri);
        }

        /// <summary>
        /// Gets XmlQualifiedName of Contract.
        /// </summary>
        /// <param name="contractType">Contract type to get.</param>
        /// <param name="name">The local name to use as the name of the System.Xml.XmlQualifiedName object.</param>
        /// <param name="nameSpace">The namespace for the System.Xml.XmlQualifiedName object.</param>
        /// <returns>XmlQualifiedName instance.</returns>
        internal static XmlQualifiedName GetContractXmlQualifiedName(Type contractType, string name, string nameSpace)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = contractType.Name;
            }

            if (nameSpace == null)
            {
                nameSpace = DefaultNamespace;
            }
            else
            {
                nameSpace = Uri.EscapeUriString(nameSpace);
            }

            return new XmlQualifiedName(name, nameSpace);
        }

        /// <summary>
        /// Gets error list.
        /// </summary>
        /// <param name="collection">Error collection.</param>
        /// <returns>A list of error.</returns>
        private static IList<CompilerError> GetErrorList(CompilerErrorCollection collection)
        {
            if (collection == null)
            {
                return null;
            }

            List<CompilerError> result = new List<CompilerError>();

            foreach (CompilerError error in collection)
            {
                result.Add(error);
            }

            return result;
        }

        /// <summary>
        /// Import Namespaces.
        /// </summary>
        private void ImportNamespaces()
        {
            this.Namespaces = new List<string>();

            if (this._codeCompileUnit != null)
            {
                foreach (CodeNamespace item in this._codeCompileUnit.Namespaces)
                {
                    if (!string.IsNullOrEmpty(item.Name) && !this.Namespaces.Contains(item.Name))
                    {
                        this.Namespaces.Add(item.Name);
                    }
                }
            }
            else
            {
                foreach (var item in this.Types)
                {
                    if (!string.IsNullOrEmpty(item.Namespace) && !this.Namespaces.Contains(item.Namespace))
                    {
                        this.Namespaces.Add(item.Namespace);
                    }
                }
            }
        }

        /// <summary>
        /// Import Contract types.
        /// </summary>
        private void ImportTypes()
        {
            this.Types = this.ProxyAssembly.GetTypes();

            this.ContractTypes = new List<Type>();

            if (this.Endpoints != null)
            {
                foreach (var endpoint in this.Endpoints)
                {
                    try
                    {
                        Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

                        if (!this.ContractTypes.Contains(contractType))
                        {
                            this.ContractTypes.Add(contractType);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                foreach (var item in this.Types)
                {
                    if (item.IsInterface && item.IsPublic)
                    {
                        this.ContractTypes.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Dispose CodeDomProvider.
        /// </summary>
        private void DisposeCodeDomProvider()
        {
            if (this._codeDomProvider != null)
            {
                this._codeDomProvider.Dispose();
                this._codeDomProvider = null;
            }
        }

        /// <summary>
        /// Download Metadata.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void DownloadMetadata()
        {
            using (DiscoveryClientProtocol disco = new DiscoveryClientProtocol())
            {
                disco.AllowAutoRedirect = true;
                disco.UseDefaultCredentials = true;
                disco.DiscoverAny(this.Url);
                disco.ResolveAll();

                List<MetadataSection> result = new List<MetadataSection>();

                foreach (object document in disco.Documents.Values)
                {
                    this.AddDocumentToResult(document, result);
                }

                this.Metadata = result;
            }
        }

        /// <summary>
        /// Add document to result.
        /// </summary>
        /// <param name="document">Document to check.</param>
        /// <param name="result">A list to add.</param>
        private void AddDocumentToResult(object document, IList<MetadataSection> result)
        {
            WebService.ServiceDescription wsdl = document as WebService.ServiceDescription;

            XmlSchema xmlSchema = document as XmlSchema;
#if !__MonoCS__
            XmlElement xmlElement = document as XmlElement;
#endif
            if (wsdl != null)
            {
                result.Add(MetadataSection.CreateFromServiceDescription(wsdl));
            }
            else if (xmlSchema != null)
            {
                result.Add(MetadataSection.CreateFromSchema(xmlSchema));
            }
#if !__MonoCS__
            else if (xmlElement != null && xmlElement.LocalName == "Policy")
            {
                result.Add(MetadataSection.CreateFromPolicy(xmlElement, null));
            }
#endif
            else
            {
                MetadataSection metadataSection = new MetadataSection();
                metadataSection.Metadata = document;
                result.Add(metadataSection);
            }
        }

        /// <summary>
        /// Import Metadata.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void ImportMetadata()
        {
            this._codeCompileUnit = new CodeCompileUnit();
            this._codeDomProvider = CodeDomProvider.CreateProvider(this._setupInfo.Language.ToString());

            WsdlImporter importer = new WsdlImporter(new MetadataSet(this.Metadata));

            this.AddStateForDataContractSerializerImport(importer);
            this.AddStateForXmlSerializerImport(importer);

            this.Bindings = importer.ImportAllBindings();
            this.Contracts = importer.ImportAllContracts();
            this.Endpoints = importer.ImportAllEndpoints();
            this.MetadataImportWarnings = importer.Errors;
        }

        /// <summary>
        /// Add state for XmlSerializerImport.
        /// </summary>
        /// <param name="importer">Importer instance.</param>
        private void AddStateForXmlSerializerImport(WsdlImporter importer)
        {
#if !__MonoCS__
            XmlSerializerImportOptions importOptions = new XmlSerializerImportOptions(this._codeCompileUnit);

            importOptions.CodeProvider = this._codeDomProvider;
            importOptions.WebReferenceOptions = new WebService.WebReferenceOptions();
            importOptions.WebReferenceOptions.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.EnableDataBinding;

            if (this._setupInfo.GenerateAsync)
            {
                importOptions.WebReferenceOptions.CodeGenerationOptions |= CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateOldAsync;
            }

            importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(typeof(TypedDataSetSchemaImporterExtension).AssemblyQualifiedName);
            importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(typeof(DataSetSchemaImporterExtension).AssemblyQualifiedName);

            importer.State.Add(typeof(XmlSerializerImportOptions), importOptions);
#endif
        }

        /// <summary>
        /// Add state for DataContractSerializerImport.
        /// </summary>
        /// <param name="importer">Importer instance.</param>
        private void AddStateForDataContractSerializerImport(WsdlImporter importer)
        {
            XsdDataContractImporter xsdDataContractImporter = new XsdDataContractImporter(this._codeCompileUnit);

            xsdDataContractImporter.Options = new ImportOptions();
            xsdDataContractImporter.Options.ImportXmlType = this._setupInfo.FormatMode == DynamicClientProxyFactorySetup.FormatModeOptions.DataContractSerializer;
            xsdDataContractImporter.Options.CodeProvider = this._codeDomProvider;

            importer.State.Add(typeof(XsdDataContractImporter), xsdDataContractImporter);

            foreach (IWsdlImportExtension importExtension in importer.WsdlImportExtensions)
            {
                DataContractSerializerMessageContractImporter dataContractMessageImporter = importExtension as DataContractSerializerMessageContractImporter;

                if (dataContractMessageImporter != null)
                {
                    dataContractMessageImporter.Enabled = this._setupInfo.FormatMode != DynamicClientProxyFactorySetup.FormatModeOptions.XmlSerializer;
                }
            }
        }

        /// <summary>
        /// Generate ServiceContract.
        /// </summary>
        private void GenerateServiceContract()
        {
            this._contractGenerator = new ServiceContractGenerator(this._codeCompileUnit);
            this._contractGenerator.Options = ServiceContractGenerationOptions.ChannelInterface | ServiceContractGenerationOptions.ClientClass;

            if (this._setupInfo.GenerateAsync)
            {
                this._contractGenerator.Options |= ServiceContractGenerationOptions.AsynchronousMethods | ServiceContractGenerationOptions.EventBasedAsynchronousMethods;
            }

            foreach (ContractDescription contract in this.Contracts)
            {
                this._contractGenerator.GenerateServiceContractType(contract);
            }

            bool success = true;
            this.CodeGenerationWarnings = this._contractGenerator.Errors;

            if (this.CodeGenerationWarnings != null)
            {
                foreach (MetadataConversionError error in this.CodeGenerationWarnings)
                {
                    if (!error.IsWarning)
                    {
                        success = false;
                        break;
                    }
                }
            }

            if (!success)
            {
                DynamicClientProxyException exception = new DynamicClientProxyException(DynamicClientProxyConstants.CodeGenerationError);
                exception.CodeGenerationErrors = this.CodeGenerationWarnings;
                InternalLogger.Log(exception);
                throw exception;
            }
        }

        /// <summary>
        /// Compile Proxy.
        /// </summary>
        /// <param name="outputAssembly">The name of the output assembly.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void CompileProxy(string outputAssembly = null, bool overwrite = false)
        {
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;

            if (!string.IsNullOrEmpty(outputAssembly))
            {
                string fullPath = Path.GetFullPath(outputAssembly);

                string fullDirectoryPath = Path.GetDirectoryName(Path.GetFullPath(outputAssembly));

                if (overwrite || !File.Exists(fullPath))
                {
                    if (!Directory.Exists(fullDirectoryPath))
                    {
                        Directory.CreateDirectory(fullDirectoryPath);
                    }

                    compilerParams.OutputAssembly = fullPath;
                }
            }

            this.AddAssemblyReference(typeof(System.ServiceModel.ServiceContractAttribute).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Web.Services.Description.ServiceDescription).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Runtime.Serialization.DataContractAttribute).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Xml.XmlElement).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Uri).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Data.DataSet).Assembly, compilerParams.ReferencedAssemblies);

            CompilerResults compilerResults = this._codeDomProvider.CompileAssemblyFromSource(compilerParams, this.ProxyCode);

            if (compilerResults.Errors != null && compilerResults.Errors.HasErrors)
            {
                DynamicClientProxyException exception = new DynamicClientProxyException(DynamicClientProxyConstants.CompilerError);
                exception.CompilerErrors = GetErrorList(compilerResults.Errors);
                InternalLogger.Log(exception);
                throw exception;
            }

            this.CompilerWarnings = GetErrorList(compilerResults.Errors);
            this.ProxyAssembly = compilerResults.CompiledAssembly;
        }

        /// <summary>
        /// Write Code.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void WriteCode()
        {
            using (StringWriter writer = new StringWriter())
            {
                CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
                codeGenOptions.BracingStyle = "C";
                this._codeDomProvider.GenerateCodeFromCompileUnit(this._codeCompileUnit, writer, codeGenOptions);
                writer.Flush();
                this.ProxyCode = writer.ToString();
            }

            if (this._setupInfo.CodeModifier != null)
            {
                this.ProxyCode = this._setupInfo.CodeModifier(this.ProxyCode);
            }
        }

        /// <summary>
        /// Add assembly reference.
        /// </summary>
        /// <param name="referencedAssembly">Assembly reference to add.</param>
        /// <param name="refAssemblies">Referenced assembly collection.</param>
        private void AddAssemblyReference(Assembly referencedAssembly, StringCollection refAssemblies)
        {
            string path = Path.GetFullPath(referencedAssembly.Location);
            string name = Path.GetFileName(path);

            if (!(refAssemblies.Contains(name) || refAssemblies.Contains(path)))
            {
                refAssemblies.Add(path);
            }
        }

        /// <summary>
        /// Check if Contract name is match or not.
        /// </summary>
        /// <param name="contractDescription">ContractDescription instance.</param>
        /// <param name="name">Contract name.</param>
        /// <returns>true if match; otherwise, false.</returns>
        private bool ContractNameMatch(ContractDescription contractDescription, string name)
        {
            return string.Compare(contractDescription.Name, name, true) == 0;
        }

        /// <summary>
        /// Check if Contract namespace is match or not.
        /// </summary>
        /// <param name="contractDescription">ContractDescription instance.</param>
        /// <param name="nameSpace">Contract namespace.</param>
        /// <returns>true if match; otherwise, false.</returns>
        private bool ContractNamespaceMatch(ContractDescription contractDescription, string nameSpace)
        {
            return nameSpace == null || string.Compare(contractDescription.Namespace, nameSpace, true) == 0;
        }

        /// <summary>
        /// Get Contract type.
        /// </summary>
        /// <param name="contractName">Contract name to get.</param>
        /// <param name="contractNamespace">Contract namespace to get.</param>
        /// <returns>Contract type.</returns>
        private Type GetContractType(string contractName, string contractNamespace)
        {
            Type[] types = this.ProxyAssembly.GetTypes();

            Type result = null;
            ServiceContractAttribute serviceContractAttribute = null;
            XmlQualifiedName xmlQualifiedName;

            foreach (Type type in types)
            {
                if (!type.IsInterface)
                {
                    continue;
                }

                object[] attributes = type.GetCustomAttributes(typeof(ServiceContractAttribute), false);

                if ((attributes == null) || (attributes.Length == 0))
                {
                    continue;
                }

                serviceContractAttribute = (ServiceContractAttribute)attributes[0];

                xmlQualifiedName = GetContractXmlQualifiedName(type, serviceContractAttribute.Name, serviceContractAttribute.Namespace);

                if (string.Compare(xmlQualifiedName.Name, contractName, true) != 0)
                {
                    continue;
                }

                if (string.Compare(xmlQualifiedName.Namespace, contractNamespace, true) != 0)
                {
                    continue;
                }

                result = type;
                break;
            }

            if (result == null)
            {
                throw new ArgumentException(DynamicClientProxyConstants.UnknownContract);
            }

            return result;
        }

        /// <summary>
        /// Get Proxy type.
        /// </summary>
        /// <param name="contractType">Contract type to get.</param>
        /// <returns>Proxy type.</returns>
        private Type GetProxyType(Type contractType)
        {
            Type clientBaseType = typeof(ClientBase<>).MakeGenericType(contractType);

            Type result = null;

            foreach (Type type in this.Types)
            {
                if (type.IsClass && contractType.IsAssignableFrom(type) && type.IsSubclassOf(clientBaseType))
                {
                    result = type;
                    break;
                }
            }

            if (result == null)
            {
                throw new DynamicClientProxyException(string.Format(DynamicClientProxyConstants.ProxyTypeNotFoundStringFormat, contractType.FullName));
            }

            return result;
        }
    }
}
