//-----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

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
        public static void ForEach<T>(this T[] source, Action<T> action, bool ignoreException = true)
        {
            if (ignoreException)
            {
                if ((source == null) || (source.Length == 0) || (action == null))
                {
                    return;
                }

                for (int i = 0; i < source.Length; i++)
                {
                    action(source[i]);
                }
            }
            else
            {
                Array.ForEach<T>(source, action);
            }
        }

        /// <summary>
        /// Adds all elements of the suffixArray to the end of the sourceArray<T>
        /// </summary>
        /// <param name="suffixArray">The array whose elements should be added to the end of the sourceArray</param>
        /// <param name="sourceArray">The target array</param>
        /// <param name="useDeepClone">Whether use deep clone of the element in suffixArray</param>
        public static void AddRangeTo<T>(this T[] suffixArray, ref T[] sourceArray, bool useDeepClone = false)
        {
            if ((suffixArray == null) || (suffixArray.Length == 0))
            {
                return;
            }

            if (sourceArray == null)
            {
                int suffixArrayLength = suffixArray.Length;
                sourceArray = new T[suffixArrayLength];
                if (useDeepClone)
                {
                    for (int i = 0; i < suffixArrayLength; i++)
                    {
                        sourceArray[i] = suffixArray[i].CloneDeep();
                    }
                }
                else
                {
                    Array.Copy(suffixArray, sourceArray, suffixArrayLength);
                }

                return;
            }
            else
            {
                int sourceArrayLength = sourceArray.Length;
                int suffixArrayLength = suffixArray.Length;
                T[] result = new T[sourceArrayLength + suffixArrayLength];

                if (useDeepClone)
                {
                    for (int i = 0; i < sourceArrayLength; i++)
                    {
                        result[i] = sourceArray[i].CloneDeep();
                    }

                    for (int i = 0; i < suffixArrayLength; i++)
                    {
                        result[i + sourceArrayLength] = suffixArray[i].CloneDeep();
                    }
                    sourceArray = result;
                    return;
                }
                else
                {
                    sourceArray.CopyTo(result, 0);
                    suffixArray.CopyTo(result, sourceArrayLength);
                    sourceArray = result;
                    return;
                }
            }
        }
    }
}
