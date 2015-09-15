//-----------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;

    /// <summary>
    /// Array Extensions.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the specified array.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <param name="source">Source array.</param>
        /// <param name="action">Method for element.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        public static void ForEach<T>(this T[] source, Action<T> action, bool throwOnError = false)
        {
            if (source == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("source");
                }

                return;
            }

            if (action == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("action");
                }

                return;
            }

            for (int i = 0; i < source.Length; i++)
            {
                try
                {
                    action(source[i]);
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Adds all elements of the suffixArray to the end of the sourceArray.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <param name="suffixArray">The array whose elements should be added to the end of the sourceArray.</param>
        /// <param name="sourceArray">The target array.</param>
        /// <param name="useDeepClone">Whether use deep clone of the element in suffixArray.</param>
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
                Array.Resize(ref sourceArray, sourceArrayLength + suffixArrayLength);

                if (useDeepClone)
                {
                    for (int i = 0; i < suffixArrayLength; i++)
                    {
                        sourceArray[i + sourceArrayLength] = suffixArray[i].CloneDeep();
                    }

                    return;
                }
                else
                {
                    suffixArray.CopyTo(sourceArray, sourceArrayLength);
                    return;
                }
            }
        }

        /// <summary>
        /// Find the first occurrence of value type array in another value type array.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the array.</typeparam>
        /// <param name="source">The array to search in.</param>
        /// <param name="pattern">The array to find.</param>
        /// <returns>The first position of the found array or -1 if not found.</returns>
        public static int FindArray<T>(this T[] source, T[] pattern) where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            if (pattern.Length == 0)
            {
                return 0;
            }

            int j = -1;
            int end = source.Length - pattern.Length;

            while (((j = Array.IndexOf(source, pattern[0], j + 1)) <= end) && (j != -1))
            {
                int i = 1;

                while (source[j + i].Equals(pattern[i]))
                {
                    if (++i == pattern.Length)
                    {
                        return j;
                    }
                }
            }

            return -1;
        }
    }
}
