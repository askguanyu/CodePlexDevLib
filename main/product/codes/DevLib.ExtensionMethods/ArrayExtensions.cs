//-----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;

    /// <summary>
    /// Array Extensions
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the input array<T>
        /// </summary>
        /// <param name="value">Array like int[]</param>
        /// <param name="action">Method for element</param>
        public static void ForEach<T>(this T[] value, Action<T> action)
        {
            if ((value == null) || (value.Length == 0) || (action == null))
            {
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                action(value[i]);
            }
        }
    }
}
