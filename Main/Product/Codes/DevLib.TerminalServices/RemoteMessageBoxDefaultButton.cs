//-----------------------------------------------------------------------
// <copyright file="RemoteMessageBoxDefaultButton.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Specifies the buttons that should be selected by default in a remote message box.
    /// </summary>
    public enum RemoteMessageBoxDefaultButton
    {
        /// <summary>
        /// The first button should be selected. This is the default.
        /// </summary>
        Button1 = 0,

        /// <summary>
        /// The second button should be selected.
        /// </summary>
        Button2 = 0x100,

        /// <summary>
        /// The third button should be selected.
        /// </summary>
        Button3 = 0x200,

        /// <summary>
        /// The fourth button should be selected.
        /// </summary>
        Button4 = 0x300,
    }
}
