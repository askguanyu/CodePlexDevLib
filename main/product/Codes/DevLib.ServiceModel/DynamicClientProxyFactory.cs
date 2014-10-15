﻿//-----------------------------------------------------------------------
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
        /// Field _options.
        /// </summary>
        private DynamicClientProxyFactoryOptions _options;

        /// <summary>
        /// Field _codeCompileUnit.
        /// </summary>
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
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="options">The DynamicClientProxyFactoryOptions to use.</param>
        public DynamicClientProxyFactory(string url, DynamicClientProxyFactoryOptions options)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.Url = url;
            this._options = options;

            this.DownloadMetadata();
            this.ImportMetadata();
            this.GenerateServiceContract();
            this.WriteCode();
            this.CompileProxy();
            this.DisposeCodeDomProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        public DynamicClientProxyFactory(string url)
            : this(url, new DynamicClientProxyFactoryOptions())
        {
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
            get;
            private set;
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
            get
            {
                return this.ProxyAssembly.GetTypes();
            }
        }

        /// <summary>
        /// Gets all the Metadata.
        /// </summary>
        public IList<MetadataSection> Metadata
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Bindings.
        /// </summary>
        public IList<Binding> Bindings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Contracts.
        /// </summary>
        public IList<ContractDescription> Contracts
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Endpoints.
        /// </summary>
        public IList<ServiceEndpoint> Endpoints
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Metadata Import Warnings.
        /// </summary>
        public IList<MetadataConversionError> MetadataImportWarnings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Code Generation Warnings.
        /// </summary>
        public IList<MetadataConversionError> CodeGenerationWarnings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all the Compiler Warnings.
        /// </summary>
        public IList<CompilerError> CompilerWarnings
        {
            get;
            private set;
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy()
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string remoteUri)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], this.Endpoints[0].Binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Binding binding, string remoteUri)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], binding, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Binding binding, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], binding, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type bindingType, string remoteUri)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], bindingType, remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(this.Endpoints[0], bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, Type bindingType, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(contractName, null, bindingType, remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, Binding binding, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, Binding binding, string remoteHost, int remotePort)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, string contractNamespace, Binding binding, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type contractType, Type bindingType, string remoteUri)
        {
            return this.GetClientBaseProxy(contractType, WcfServiceType.GetBinding(bindingType), remoteUri);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type contractType, Type bindingType, string remoteHost, int remotePort)
        {
            return this.GetClientBaseProxy(contractType, WcfServiceType.GetBinding(bindingType), remoteHost, remotePort);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type contractType, Binding binding, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(Type contractType, Binding binding, string remoteHost, int remotePort)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri)
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
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetClientBaseProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort)
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
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionThrowableProxy(endpoint, endpoint.Binding, endpoint.Address.ToString(), fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionThrowableProxy(endpoint, endpoint.Binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionThrowableProxy(endpoint, endpoint.Binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(this.Endpoints[0], binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(this.Endpoints[0], bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. his instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionThrowableProxy(contractName, null, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionThrowableProxy(endpoint, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type contractType, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionThrowableProxy(contractType, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type contractType, Binding binding, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionThrowableProxy(contractType, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionUnthrowableProxy(endpoint, endpoint.Binding, endpoint.Address.ToString(), fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionUnthrowableProxy(endpoint, endpoint.Binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerSessionUnthrowableProxy(endpoint, endpoint.Binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerSessionUnthrowableProxy(contractName, null, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerSessionUnthrowableProxy(endpoint, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type contractType, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionUnthrowableProxy(contractType, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type contractType, Binding binding, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerSessionUnthrowableProxy(contractType, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerSessionUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerSessionUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallThrowableProxy(endpoint, endpoint.Binding, endpoint.Address.ToString(), fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallThrowableProxy(endpoint, endpoint.Binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallThrowableProxy(endpoint, endpoint.Binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(this.Endpoints[0], binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(this.Endpoints[0], bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(contractName, null, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(contractName, null, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(contractName, null, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallThrowableProxy(contractName, null, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallThrowableProxy(endpoint, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type contractType, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallThrowableProxy(contractType, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type contractType, Binding binding, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallThrowableProxy(contractType, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallThrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallThrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallUnthrowableProxy(endpoint, endpoint.Binding, endpoint.Address.ToString(), fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallUnthrowableProxy(endpoint, endpoint.Binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.Endpoints[0];

            return this.GetPerCallUnthrowableProxy(endpoint, endpoint.Binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(this.Endpoints[0], binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(this.Endpoints[0], binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(this.Endpoints[0], bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(this.Endpoints[0], bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, Binding binding, string remoteUri, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            return this.GetPerCallUnthrowableProxy(contractName, null, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, bindingType, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteUri, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractName">Service Contract name.</param>
        /// <param name="contractNamespace">Service Contract namespace.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(string contractName, string contractNamespace, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            ServiceEndpoint endpoint = this.GetEndpoint(contractName, contractNamespace);

            return this.GetPerCallUnthrowableProxy(endpoint, binding, remoteHost, remotePort, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type contractType, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type contractType, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallUnthrowableProxy(contractType, bindingType, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type contractType, Binding binding, string remoteUri, bool fromCaching = true)
        {
            string address = string.IsNullOrEmpty(remoteUri) ? this.GetEndpoint(contractType.Name, null).Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="contractType">Service Contract type.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(Type contractType, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            return this.GetPerCallUnthrowableProxy(contractType, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Type), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { bindingType, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteUri, bool fromCaching = true)
        {
            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string address = string.IsNullOrEmpty(remoteUri) ? endpoint.Address.ToString() : remoteUri;

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, address, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
        }

        /// <summary>
        /// Gets client proxy. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpoint">Service endpoint.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of DynamicClientProxy.</returns>
        public DynamicClientProxy GetPerCallUnthrowableProxy(ServiceEndpoint endpoint, Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            Type contractType = this.GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, contractType.FullName).ToString();

            Type clientProxyType = typeof(WcfClientProxy<>).MakeGenericType(contractType);

            MethodInfo methodInfo = clientProxyType.GetMethod("GetPerCallUnthrowableInstance", new Type[] { typeof(Binding), typeof(string), typeof(bool) });

            object clientProxyObject = methodInfo.Invoke(null, new object[] { binding, remoteUri, fromCaching });

            return new DynamicClientProxy(clientProxyObject);
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
            this._codeDomProvider = CodeDomProvider.CreateProvider(this._options.Language.ToString());

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

            if (this._options.GenerateAsync)
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
            xsdDataContractImporter.Options.ImportXmlType = this._options.FormatMode == DynamicClientProxyFactoryOptions.FormatModeOptions.DataContractSerializer;
            xsdDataContractImporter.Options.CodeProvider = this._codeDomProvider;

            importer.State.Add(typeof(XsdDataContractImporter), xsdDataContractImporter);

            foreach (IWsdlImportExtension importExtension in importer.WsdlImportExtensions)
            {
                DataContractSerializerMessageContractImporter dataContractMessageImporter = importExtension as DataContractSerializerMessageContractImporter;

                if (dataContractMessageImporter != null)
                {
                    dataContractMessageImporter.Enabled = this._options.FormatMode != DynamicClientProxyFactoryOptions.FormatModeOptions.XmlSerializer;
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

            if (this._options.GenerateAsync)
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
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void CompileProxy()
        {
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;

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

            if (this._options.CodeModifier != null)
            {
                this.ProxyCode = this._options.CodeModifier(this.ProxyCode);
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

            Type[] types = this.ProxyAssembly.GetTypes();

            Type result = null;

            foreach (Type type in types)
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