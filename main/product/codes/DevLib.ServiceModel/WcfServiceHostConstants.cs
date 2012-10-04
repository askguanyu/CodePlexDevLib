//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// WcfServiceHost Constants
    /// </summary>
    internal static class WcfServiceHostConstants
    {
        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostOpenStringFormat = "WcfServiceHost Open successfully. ServiceType: {0} | AbsoluteUri: {1}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostOpenExceptionStringFormat = "WcfServiceHost Open failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfIsolatedServiceHostCreateDomainExceptionStringFormat = "WcfIsolatedServiceHost CreateDomain failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostCloseStringFormat = "WcfServiceHost Close successfully. ServiceType: {0} | AbsoluteUri: {1}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostCloseExceptionStringFormat = "WcfServiceHost Close failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostAbortStringFormat = "WcfServiceHost Abort successfully. ServiceType: {0} | AbsoluteUri: {1}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostAbortExceptionStringFormat = "WcfServiceHost Abort failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostRestartStringFormat = "WcfServiceHost Restart successfully. ServiceType: {0} | AbsoluteUri: {1}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostRestartExceptionStringFormat = "WcfServiceHost Restart failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostInitStringFormat = "WcfServiceHost Init successfully. ServiceType: {0} | AbsoluteUri: {1}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostInitExceptionStringFormat = "WcfServiceHost Init failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostTypeLoadFileExceptionStringFormat = "WcfServiceHostType LoadFile failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";

        /// <summary>
        ///
        /// </summary>
        internal const string WcfServiceHostTypeLoadFromExceptionStringFormat = "WcfServiceHostType LoadFrom failure with exception. Source:{0} | Message:{1} | StackTrace:{2}";
    }
}
