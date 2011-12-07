//-----------------------------------------------------------------------
// <copyright file="DictionaryExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Dictionary Extensions
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Update value, if not cantain key then add value
        /// </summary>
        /// <param name="dict">The source dictionary</param>
        /// <param name="key">The key of interest</param>
        /// <param name="value">The value to update</param>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            lock (((ICollection)dict).SyncRoot)
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
                else
                {
                    dict.Add(key, value);
                }
            }
        }
    }
}
