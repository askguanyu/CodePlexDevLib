//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxyConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    /// <summary>
    /// WebServiceClientProxy Constants.
    /// </summary>
    internal static class WebServiceClientProxyConstants
    {
        /// <summary>
        /// Field CompilerError.
        /// </summary>
        public const string CompilerError = "There was an error in compiling the proxy code.";

        /// <summary>
        /// Field ProxyCtorNotFound.
        /// </summary>
        public const string ProxyCtorNotFound = "The constructor matching the specified parameter types is not found.";

        /// <summary>
        /// Field ParameterValueMismatch.
        /// </summary>
        public const string ParameterValueMismatch = "The type for each parameter values must be specified.";

        /// <summary>
        /// Field MethodNotFoundStringFormat.
        /// </summary>
        public const string MethodNotFoundStringFormat = "The method {0} is not found.";
    }
}
