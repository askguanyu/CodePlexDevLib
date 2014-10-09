//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.CSharp;

    /// <summary>
    /// Dynamic generating client proxy for Web services.
    /// </summary>
    public class WebServiceClientProxy
    {
        /// <summary>
        /// Field _client.
        /// </summary>
        private readonly object _client;

        /// <summary>
        /// Field _clientType.
        /// </summary>
        private readonly Type _clientType;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxy" /> class.
        /// </summary>
        /// <param name="url">Web service url.</param>
        [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        public WebServiceClientProxy(string url)
        {
            XmlTextReader xmlTextReader = null;
            ServiceDescriptionImporter serviceDescriptionImporter = null;

            CodeNamespace codeNamespace = new CodeNamespace();
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(codeNamespace);

            try
            {
                try
                {
                    xmlTextReader = new XmlTextReader(url.EndsWith("wsdl", StringComparison.OrdinalIgnoreCase) ? url : url.TrimEnd(' ', '\\', '/', '?') + "?wsdl");
                    ServiceDescription serviceDescription = ServiceDescription.Read(xmlTextReader);
                    serviceDescriptionImporter = new ServiceDescriptionImporter();
                    serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.None | CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.EnableDataBinding;
                    serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
                    serviceDescriptionImporter.Import(codeNamespace, codeCompileUnit);
                }
                catch (InvalidOperationException)
                {
                    if (xmlTextReader != null)
                    {
                        xmlTextReader.Close();
                        xmlTextReader = null;
                    }

                    serviceDescriptionImporter = null;

                    xmlTextReader = new XmlTextReader(url.EndsWith("wsdl", StringComparison.OrdinalIgnoreCase) ? url : url.TrimEnd(' ', '\\', '/', '?') + "?singleWsdl");
                    ServiceDescription serviceDescription = ServiceDescription.Read(xmlTextReader);
                    serviceDescriptionImporter = new ServiceDescriptionImporter();
                    serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.None | CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.EnableDataBinding;
                    serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
                    serviceDescriptionImporter.Import(codeNamespace, codeCompileUnit);
                }

                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.GenerateInMemory = true;

                using (CSharpCodeProvider provider = new CSharpCodeProvider())
                {
                    CompilerResults compilerResults = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);
                    this._clientType = compilerResults.CompiledAssembly.GetTypes()[0];
                    this._client = Activator.CreateInstance(this._clientType);
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                    xmlTextReader = null;
                }
            }
        }

        /// <summary>
        /// Gets all the public methods of the current client proxy.
        /// </summary>
        public MethodInfo[] Methods
        {
            get
            {
                return this._clientType.GetMethods();
            }
        }

        /// <summary>
        /// Compiles a client proxy assembly based on Web services url.
        /// </summary>
        /// <param name="url">Web service url.</param>
        /// <param name="outputAssembly">The name of the output assembly.</param>
        [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void CompileAssembly(string url, string outputAssembly)
        {
            XmlTextReader xmlTextReader = null;
            ServiceDescriptionImporter serviceDescriptionImporter = null;

            CodeNamespace codeNamespace = new CodeNamespace();
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(codeNamespace);

            try
            {
                try
                {
                    xmlTextReader = new XmlTextReader(url.EndsWith("wsdl", StringComparison.OrdinalIgnoreCase) ? url : url.TrimEnd(' ', '\\', '/', '?') + "?wsdl");
                    ServiceDescription serviceDescription = ServiceDescription.Read(xmlTextReader);
                    serviceDescriptionImporter = new ServiceDescriptionImporter();
                    serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.None | CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.EnableDataBinding;
                    serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
                    serviceDescriptionImporter.Import(codeNamespace, codeCompileUnit);
                }
                catch (InvalidOperationException)
                {
                    if (xmlTextReader != null)
                    {
                        xmlTextReader.Close();
                        xmlTextReader = null;
                    }

                    serviceDescriptionImporter = null;

                    xmlTextReader = new XmlTextReader(url.EndsWith("wsdl", StringComparison.OrdinalIgnoreCase) ? url : url.TrimEnd(' ', '\\', '/', '?') + "?singleWsdl");
                    ServiceDescription serviceDescription = ServiceDescription.Read(xmlTextReader);
                    serviceDescriptionImporter = new ServiceDescriptionImporter();
                    serviceDescriptionImporter.CodeGenerationOptions = CodeGenerationOptions.None | CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOldAsync | CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.EnableDataBinding;
                    serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
                    serviceDescriptionImporter.Import(codeNamespace, codeCompileUnit);
                }

                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.GenerateInMemory = false;
                compilerParameters.GenerateExecutable = false;
                compilerParameters.OutputAssembly = outputAssembly;

                using (CSharpCodeProvider provider = new CSharpCodeProvider())
                {
                    CompilerResults compilerResults = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                    xmlTextReader = null;
                }
            }
        }

        /// <summary>
        /// Invokes the method represented by the current client, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="args">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public object Invoke(string methodName, params object[] args)
        {
            return this._clientType.GetMethod(methodName).Invoke(this._client, args);
        }

        /// <summary>
        /// Invokes the method represented by the current client, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="args">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public object Invoke(MethodInfo methodInfo, params object[] args)
        {
            return methodInfo.Invoke(this._client, args);
        }
    }
}
