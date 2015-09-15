//-----------------------------------------------------------------------
// <copyright file="ConfigurationConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    /// <summary>
    /// Configuration Constants.
    /// </summary>
    internal static class ConfigurationConstants
    {
        /// <summary>
        /// Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failed with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";

        /// <summary>
        /// Field KeyNotFoundExceptionStringFormat.
        /// </summary>
        internal const string KeyNotFoundExceptionStringFormat = "Key \"{0}\" not found exception.";
    }
}
