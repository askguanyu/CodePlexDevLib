//-----------------------------------------------------------------------
// <copyright file="AddInConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    /// <summary>
    ///
    /// </summary>
    internal static class AddInConstants
    {
        /// <summary>
        ///
        /// </summary>
        internal const string DefaultFriendlyName = "DefaultAddInDomain";

        /// <summary>
        ///
        /// </summary>
        internal const string KeyIpcPortName = "portName";

        /// <summary>
        ///
        /// </summary>
        internal const string KeyIpcChannelName = "name";

        /// <summary>
        ///
        /// </summary>
        internal const string IpcUrlStringFormat = "ipc://{0}/{1}";

        /// <summary>
        ///
        /// </summary>
        internal const string AddInUnknownPlatformException = "Unknown platform target specified.";

        /// <summary>
        ///
        /// </summary>
        internal const string ExceptionStringFormat = "Exception: {0} failure with exception. Source: {1} | Message: {2} | StackTrace: {3}";

        /// <summary>
        ///
        /// </summary>
        internal const string WarningStringFormat = "Warning: {0} with warning. Source: {1} | Message: {2} | StackTrace: {3}";
    }
}
