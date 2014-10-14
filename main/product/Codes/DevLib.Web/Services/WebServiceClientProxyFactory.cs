//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxyFactory.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents factory class to create WebServiceClientProxy.
    /// </summary>
    [Serializable]
    public class WebServiceClientProxyFactory : MarshalByRefObject
    {
        /// <summary>
        /// Field _options.
        /// </summary>
        private WebServiceClientProxyFactoryOptions _options;

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
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="options">The WebServiceClientProxyFactoryOptions to use.</param>
        public WebServiceClientProxyFactory(string url, WebServiceClientProxyFactoryOptions options)
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
            this.WriteCode();
            this.CompileProxy();
            this.DisposeCodeDomProvider();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        public WebServiceClientProxyFactory(string url)
            : this(url, new WebServiceClientProxyFactoryOptions())
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
        public IList<ServiceDescription> Metadata
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
        /// Gets Import Warnings.
        /// </summary>
        public ServiceDescriptionImportWarnings ImportWarnings
        {
            get;
            private set;
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
        /// Gets client proxy.
        /// </summary>
        /// <returns>Instance of WebServiceClientProxy.</returns>
        public WebServiceClientProxy GetProxy()
        {
            return new WebServiceClientProxy(this.Types[0]);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="url">The URL for the service address.</param>
        /// <returns>Instance of WebServiceClientProxy.</returns>
        public WebServiceClientProxy GetProxy(string url)
        {
            return new WebServiceClientProxy(this.Types[0], url);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="proxyType">Service client proxy type.</param>
        /// <returns>Instance of WebServiceClientProxy.</returns>
        public WebServiceClientProxy GetProxy(Type proxyType)
        {
            return new WebServiceClientProxy(proxyType);
        }

        /// <summary>
        /// Gets client proxy.
        /// </summary>
        /// <param name="proxyType">Service client proxy type.</param>
        /// <param name="url">The URL for the service address.</param>
        /// <returns>Instance of WebServiceClientProxy.</returns>
        public WebServiceClientProxy GetProxy(Type proxyType, string url)
        {
            return new WebServiceClientProxy(proxyType, url);
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

                List<ServiceDescription> result = new List<ServiceDescription>();

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
        private void AddDocumentToResult(object document, IList<ServiceDescription> result)
        {
            ServiceDescription wsdl = document as ServiceDescription;

            if (wsdl != null)
            {
                result.Add(wsdl);
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

            CodeNamespace codeNamespace = new CodeNamespace();
            this._codeCompileUnit.Namespaces.Add(codeNamespace);

            ServiceDescriptionImporter serviceDescriptionImporter = new ServiceDescriptionImporter();
            serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.EnableDataBinding;

            if (this._options.GenerateAsync)
            {
                serviceDescriptionImporter.CodeGenerationOptions |= CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateOldAsync;
            }

            foreach (var serviceDescription in this.Metadata)
            {
                serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
            }

            this.ImportWarnings = serviceDescriptionImporter.Import(codeNamespace, this._codeCompileUnit);
        }

        /// <summary>
        /// Compile Proxy.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void CompileProxy()
        {
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;

            this.AddAssemblyReference(typeof(System.Web.Services.Description.ServiceDescription).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Xml.XmlElement).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Uri).Assembly, compilerParams.ReferencedAssemblies);
            this.AddAssemblyReference(typeof(System.Data.DataSet).Assembly, compilerParams.ReferencedAssemblies);

            CompilerResults compilerResults = this._codeDomProvider.CompileAssemblyFromSource(compilerParams, this.ProxyCode);

            if (compilerResults.Errors != null && compilerResults.Errors.HasErrors)
            {
                WebServiceClientProxyException exception = new WebServiceClientProxyException(WebServiceClientProxyConstants.CompilerError);
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
    }
}
