//-----------------------------------------------------------------------
// <copyright file="SettingsConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Settings
{
    /// <summary>
    /// Settings Constants.
    /// </summary>
    internal static class SettingsConstants
    {
        /// <summary>
        /// Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failure with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";

        /// <summary>
        /// Field KeyNotFoundExceptionStringFormat.
        /// </summary>
        internal const string KeyNotFoundExceptionStringFormat = "Key \"{0}\" not found exception.";
    }
}
