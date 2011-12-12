//-----------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection Extensions
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Update value, if not cantain key then add value
        /// </summary>
        /// <param name="source">The source dictionary</param>
        /// <param name="sourceKey">The key of interest</param>
        /// <param name="sourceValue">The value to update</param>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey sourceKey, TValue sourceValue)
        {
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
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return source.Any();
        }
    }
}
