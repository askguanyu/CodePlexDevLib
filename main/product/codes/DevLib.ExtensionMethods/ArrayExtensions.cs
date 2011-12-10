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
        /// Performs the specified action on each element of the specified array<T>
        /// </summary>
        /// <param name="value">Array like int[]</param>
        /// <param name="action">Method for element</param>
        /// <param name="ignoreException">if set to <c>true</c> ignore any exception</param>
        public static void ForEach<T>(this T[] value, Action<T> action, bool ignoreException = true)
        {
            if (ignoreException)
            {
                if ((value == null) || (value.Length == 0) || (action == null))
                {
                    return;
                }

                for (int i = 0; i < value.Length; i++)
                {
                    action(value[i]);
                }

                return;
            }
            else
            {
                Array.ForEach<T>(value, action);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T[] AddRange<T>(this T[] source, T[] value)
        {
            // TODO: implement add range method
            return source;

            if ((value == null) || (value.Length == 0))
            {
                return source;
            }
        }
    }
}
