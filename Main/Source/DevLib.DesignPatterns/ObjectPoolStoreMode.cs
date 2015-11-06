//-----------------------------------------------------------------------
// <copyright file="ObjectPoolStoreMode.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// ObjectPool store mode.
    /// </summary>
    public enum ObjectPoolStoreMode
    {
        /// <summary>
        /// Represents a last-in-first-out (LIFO) collection of objects.
        /// </summary>
        Stack,

        /// <summary>
        /// Represents a first-in, first-out (FIFO) collection of objects.
        /// </summary>
        Queue,

        /// <summary>
        /// Represents a circular FIFO queue.
        /// </summary>
        CircularQueue
    }
}
