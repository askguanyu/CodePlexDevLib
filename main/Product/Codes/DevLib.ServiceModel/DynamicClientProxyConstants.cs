//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// DynamicClientProxy Constants.
    /// </summary>
    internal static class DynamicClientProxyConstants
    {
        /// <summary>
        /// Field ImportError.
        /// </summary>
        public const string ImportError = "There was an error in importing the metadata.";

        /// <summary>
        /// Field CodeGenerationError
        /// </summary>
        public const string CodeGenerationError = "There was an error in generating the proxy code.";

        /// <summary>
        /// Field CompilerError
        /// </summary>
        public const string CompilerError = "There was an error in compiling the proxy code.";

        /// <summary>
        /// Field UnknownContract
        /// </summary>
        public const string UnknownContract = "The specified contract is not found in the proxy assembly.";

        /// <summary>
        /// Field EndpointNotFoundStringFormat
        /// </summary>
        public const string EndpointNotFoundStringFormat = "The endpoint associated with contract {0}:{1} is not found.";

        /// <summary>
        /// Field ProxyTypeNotFoundStringFormat
        /// </summary>
        public const string ProxyTypeNotFoundStringFormat = "The proxy that implements the service contract {0} is not found.";

        /// <summary>
        /// Field ProxyCtorNotFound
        /// </summary>
        public const string ProxyCtorNotFound = "The constructor matching the specified parameter types is not found.";

        /// <summary>
        /// Field ParameterValueMismatch
        /// </summary>
        public const string ParameterValueMismatch = "The type for each parameter values must be specified.";

        /// <summary>
        /// Field MethodNotFoundStringFormat
        /// </summary>
        public const string MethodNotFoundStringFormat = "The method {0} is not found.";
    }
}
