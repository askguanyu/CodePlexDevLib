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
        /// Field _setupInfo.
        /// </summary>
        private WebServiceClientProxyFactorySetup _setupInfo;

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
        /// Field _metadata.
        /// </summary>
        [NonSerialized]
        private IList<ServiceDescription> _metadata;

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
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="setupInfo">The WebServiceClientProxyFactorySetup to use.</param>
        public WebServiceClientProxyFactory(string url, string outputAssembly, bool overwrite, WebServiceClientProxyFactorySetup setupInfo)
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

            this.DownloadMetadata();
            this.ImportMetadata();
            this.WriteCode();
            this.CompileProxy(outputAssembly, overwrite);
            this.DisposeCodeDomProvider();
            this.Types = this.ProxyAssembly.GetTypes();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        public WebServiceClientProxyFactory(string url)
            : this(url, null, true, new WebServiceClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="setupInfo">The WebServiceClientProxyFactorySetup to use.</param>
        public WebServiceClientProxyFactory(string url, WebServiceClientProxyFactorySetup setupInfo)
            : this(url, null, true, setupInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        public WebServiceClientProxyFactory(string url, string outputAssembly)
            : this(url, outputAssembly, true, new WebServiceClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="url">The URL for the service address or the file containing the WSDL data.</param>
        /// <param name="outputAssembly">The name of the output assembly of client proxy.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        public WebServiceClientProxyFactory(string url, string outputAssembly, bool overwrite)
            : this(url, outputAssembly, overwrite, new WebServiceClientProxyFactorySetup())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactory" /> class.
        /// </summary>
        /// <param name="assemblyFile">Client proxy assembly file.</param>
        /// <param name="isLoadFile">true to indicate WebServiceClientProxyFactory is loaded from assembly file.</param>
        private WebServiceClientProxyFactory(string assemblyFile, bool isLoadFile)
        {
            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified assembly file does not exist.", assemblyFile);
            }

            this.ProxyAssembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
            this.Types = this.ProxyAssembly.GetTypes();
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
        public IList<ServiceDescription> Metadata
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
        /// Gets Import Warnings.
        /// </summary>
        public ServiceDescriptionImportWarnings ImportWarnings
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets WebServiceClientProxyFactory from client proxy assembly file.
        /// </summary>
        /// <param name="assemblyFile">Client proxy assembly file.</param>
        /// <returns>WebServiceClientProxyFactory instance.</returns>
        public static WebServiceClientProxyFactory Load(string assemblyFile)
        {
            return new WebServiceClientProxyFactory(assemblyFile, true);
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
            this._codeDomProvider = CodeDomProvider.CreateProvider(this._setupInfo.Language.ToString());

            CodeNamespace codeNamespace = new CodeNamespace();
            this._codeCompileUnit.Namespaces.Add(codeNamespace);

            ServiceDescriptionImporter serviceDescriptionImporter = new ServiceDescriptionImporter();
            serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOrder | CodeGenerationOptions.EnableDataBinding;

            if (this._setupInfo.GenerateAsync)
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
    }
}
