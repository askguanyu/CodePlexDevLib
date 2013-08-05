//-----------------------------------------------------------------------
// <copyright file="RemoteMessageBoxButtons.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Specifies the combination of buttons that should be displayed in a remote message box.
    /// </summary>
    public enum RemoteMessageBoxButtons
    {
        /// <summary>
        /// Show only an "OK" button. This is the default.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Show "OK" and "Cancel" buttons.
        /// </summary>
        OkCancel = 1,

        /// <summary>
        /// Show "Abort", "Retry", and "Ignore" buttons.
        /// </summary>
        AbortRetryIgnore = 2,

        /// <summary>
        /// Show "Yes", "No", and "Cancel" buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        /// Show "Yes" and "No" buttons.
        /// </summary>
        YesNo = 4,

        /// <summary>
        /// Show "Retry" and "Cancel" buttons.
        /// </summary>
        RetryCancel = 5,
    }
}
