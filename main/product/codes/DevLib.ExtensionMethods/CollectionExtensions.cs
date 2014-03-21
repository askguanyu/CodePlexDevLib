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
        /// Determines whether a sequence is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">Source IEnumerable.</param>
        /// <returns>true if the source sequence is empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
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
        /// <param name="source">Source IEnumerable.</param>
        /// <returns>true if the source sequence is NOT empty; otherwise, false.</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return false;
            }

            return source.Any();
        }

        /// <summary>
        /// Searches for all elements that matches the conditions defined by the specified predicate, and returns the zero-based index of the all occurrence within the entire System.Collections.Generic.List{T}.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of the zero-based index of the all occurrence of elements that matches the conditions defined by match, if found; otherwise, empty list.</returns>
        public static List<int> FindAllIndex<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<int> result = new List<int>();

            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count(); i++)
            {
                if (match(source.ElementAt(i)))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable{T} that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are started with the specified predicate element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable{T} that contains the sub collection of source</returns>
        public static List<IEnumerable<T>> SplitByStartsWith<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<IEnumerable<T>> result = new List<IEnumerable<T>>();

            List<int> indexes = source.FindAllIndex(match);

            if (indexes.Count < 1)
            {
                result.Add(source);
                return result;
            }

            int skipCount = 0;
            int takeCount = 0;

            foreach (int index in indexes)
            {
                if (index == 0)
                {
                    continue;
                }

                takeCount = index - skipCount;
                result.Add(source.Skip(skipCount).Take(takeCount));
                skipCount = index;
            }

            result.Add(source.Skip(skipCount));

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable{T} that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are ended with the specified predicate element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable{T} that contains the sub collection of source</returns>
        public static List<IEnumerable<T>> SplitByEndsWith<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<IEnumerable<T>> result = new List<IEnumerable<T>>();

            List<int> indexes = source.FindAllIndex(match);

            if (indexes.Count < 1)
            {
                result.Add(source);
                return result;
            }

            int skipCount = 0;
            int takeCount = 0;

            foreach (int index in indexes)
            {
                takeCount = index + 1 - skipCount;
                result.Add(source.Skip(skipCount).Take(takeCount));
                skipCount = index + 1;
            }

            if (skipCount < source.Count())
            {
                result.Add(source.Skip(skipCount));
            }

            return result;
        }
    }
}
