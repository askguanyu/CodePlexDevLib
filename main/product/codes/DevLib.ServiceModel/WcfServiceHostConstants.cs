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
        /// Field WcfServiceHostSucceedStringFormat.
        /// </summary>
        internal const string WcfServiceHostSucceedStringFormat = "Succeed: {0} successfully. ServiceType: {1} | AbsoluteUri: {2}";

        /// <summary>
        /// Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";
    }
}
