//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// WcfServiceHost Constants.
    /// </summary>
    internal static class WcfServiceHostConstants
    {
        /// <summary>
        /// Field WcfServiceHostSucceededStringFormat.
        /// </summary>
        internal const string WcfServiceHostSucceededStringFormat = "Succeeded: {0}. ServiceType: {1} | AbsoluteUri: {2}";

        /// <summary>
        /// Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failed with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
