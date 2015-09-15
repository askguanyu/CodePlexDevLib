//-----------------------------------------------------------------------
// <copyright file="RemoteMessageBoxIcon.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Specifies the icon that should be displayed in a remote message box.
    /// </summary>
    public enum RemoteMessageBoxIcon
    {
        /// <summary>
        /// Show no icon. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Show a hand icon.
        /// </summary>
        Hand = 0x10,

        /// <summary>
        /// Show a question mark icon.
        /// </summary>
        Question = 0x20,

        /// <summary>
        /// Show an exclamation point icon.
        /// </summary>
        Exclamation = 0x30,

        /// <summary>
        /// Show an informational icon.
        /// </summary>
        Asterisk = 0x40,

        /// <summary>
        /// Show a warning icon.
        /// </summary>
        Warning = Exclamation,

        /// <summary>
        /// Show an error icon.
        /// </summary>
        Error = Hand,

        /// <summary>
        /// Show an informational icon.
        /// </summary>
        Information = Asterisk,

        /// <summary>
        /// Show a stopping icon.
        /// </summary>
        Stop = Hand,
    }
}
