//-----------------------------------------------------------------------
// <copyright file="WcfClientConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// WcfClient Constants.
    /// </summary>
    internal static class WcfClientConstants
    {
        /// <summary>
        /// Field CreateProxyInstanceMethodName.
        /// </summary>
        public const string CreateProxyInstanceMethodName = "CreateProxyInstance";

        /// <summary>
        /// Field SetClientCredentialsMethodName.
        /// </summary>
        public const string SetClientCredentialsMethodName = "SetClientCredentials";

        /// <summary>
        /// Field CloseProxyInstanceMethodName.
        /// </summary>
        public const string CloseProxyInstanceMethodName = "CloseProxyInstance";

        /// <summary>
        /// Field CloseProxyMethodName.
        /// </summary>
        public const string CloseProxyMethodName = "CloseProxy";

        /// <summary>
        /// Field HandleFaultExceptionMethodName.
        /// </summary>
        public const string HandleFaultExceptionMethodName = "HandleFaultException";
    }
}
