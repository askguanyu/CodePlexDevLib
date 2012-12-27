//-----------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection Extensions.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Update value, if not contain key then add value.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="sourceKey">The key of the element to update.</param>
        /// <param name="sourceValue">The value of element to update.</param>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey sourceKey, TValue sourceValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (sourceKey == null)
            {
                throw new ArgumentNullException("sourceKey");
            }

            if (sourceValue == null)
            {
                throw new ArgumentNullException("sourceValue");
            }

            lock (((ICollection)source).SyncRoot)
            {
                if (source.ContainsKey(sourceKey))
                {
                    source[sourceKey] = sourceValue;
                }
                else
                {
                    source.Add(sourceKey, sourceValue);
                }
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the specified IDictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
        /// <typeparam name="TValue">The type of values in the collection.</typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="action">Method for element.</param>
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> source, Action<TKey, TValue> action)
        {
            if ((source == null) || (source.Count == 0) || (action == null))
            {
                return;
            }

            foreach (var item in source)
            {
                action(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Determines whether a sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source"></param>
        /// <returns>true if the source sequence is empty; otherwise, false.</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return true;
            }

            return !source.Any();
        }

        /// <summary>
        /// Determines whether a sequence is NOT empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source"></param>
        /// <returns>true if the source sequence is NOT empty; otherwise, false.</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return false;
            }

            return source.Any();
        }
    }
}
