//-----------------------------------------------------------------------
// <copyright file="KeyStates.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Input
{
    /// <summary>
    /// Key states.
    /// </summary>
    internal enum KeyStates
    {
        /// <summary>
        /// No state (same as up).
        /// </summary>
        None = 0,

        /// <summary>
        /// The key is down.
        /// </summary>
        Down = 1,

        /// <summary>
        /// The key is toggled on.
        /// </summary>
        Toggled = 2
    }
}
