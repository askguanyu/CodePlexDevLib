//-----------------------------------------------------------------------
// <copyright file="SendMouseInputFlags.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Input
{
    using System;

    /// <summary>
    /// Mouse input flags used by the Native Input struct. Indicate whether movement took place, or whether buttons were pressed or released.
    /// </summary>
    [Flags]
    internal enum SendMouseInputFlags
    {
        /// <summary>
        /// Specifies that the pointer moved.
        /// </summary>
        Move = 0x0001,

        /// <summary>
        /// Specifies that the left button was pressed.
        /// </summary>
        LeftDown = 0x0002,

        /// <summary>
        /// Specifies that the left button was released.
        /// </summary>
        LeftUp = 0x0004,

        /// <summary>
        /// Specifies that the right button was pressed.
        /// </summary>
        RightDown = 0x0008,

        /// <summary>
        /// Specifies that the right button was released.
        /// </summary>
        RightUp = 0x0010,

        /// <summary>
        /// Specifies that the middle button was pressed.
        /// </summary>
        MiddleDown = 0x0020,

        /// <summary>
        /// Specifies that the middle button was released.
        /// </summary>
        MiddleUp = 0x0040,

        /// <summary>
        /// Specifies that the x button was pressed.
        /// </summary>
        XDown = 0x0080,

        /// <summary>
        /// Specifies that the x button was released.
        /// </summary>
        XUp = 0x0100,

        /// <summary>
        /// Specifies that the wheel was moved.
        /// </summary>
        Wheel = 0x0800,

        /// <summary>
        /// Specifies that x, y are absolute, not relative.
        /// </summary>
        Absolute = 0x8000,
    }
}
