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
        /// Check Type inherit IEnumerable interface or not.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IEnumerable interface; otherwise, false.</returns>
        public static bool IsEnumerable(this Type source)
        {
            return source != typeof(string)
                && source.GetInterface("IEnumerable") != null;
        }

        /// <summary>
        /// Check Type inherit IDictionary interface or not.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IDictionary interface; otherwise, false.</returns>
        public static bool IsDictionary(this Type source)
        {
            return source.GetInterface("IDictionary") != null;
        }

        /// <summary>
        /// Gets the element Type of the specified type which inherit IEnumerable interface.
        /// </summary>
        /// <param name="source">Source Type which inherit IEnumerable interface.</param>
        /// <returns>The System.Type of the element in the source list.</returns>
        public static Type GetEnumerableElementType(this Type source)
        {
            if (source.GetInterface("IEnumerable") != null)
            {
                return source.IsArray ? source.GetElementType() : source.GetGenericArguments()[0];
            }

            return null;
        }

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

            if (source.ContainsKey(sourceKey))
            {
                source[sourceKey] = sourceValue;
            }
            else
            {
                source.Add(sourceKey, sourceValue);
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
        /// Gets the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            TValue result;

            source.TryGetValue(key, out result);

            return result;
        }

        /// <summary>
        /// Adds the elements of the specified dictionary to the source.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="dictionary">The dictionary to add.</param>
        /// <param name="forceUpdate">true to update the source dictionary if the key exists; otherwise, ignore the value.</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> dictionary, bool forceUpdate = true)
        {
            foreach (KeyValuePair<TKey, TValue> current in dictionary)
            {
                if (forceUpdate || !source.ContainsKey(current.Key))
                {
                    source[current.Key] = current.Value;
                }
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key, if the key is found; otherwise, create a value, add it to the source and return it.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="builder">The function to create a value.</param>
        /// <returns>The value associated with the specified key, if the key is found; otherwise, the new value created.</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> builder)
        {
            TValue result;

            if (!source.TryGetValue(key, out result))
            {
                result = builder();
                source.Add(key, result);
            }

            return result;
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
        /// Returns an empty enumeration of the same type if source is null; otherwise, return source itself.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">Source IEnumerable.</param>
        /// <returns>An empty enumeration of the same type if source is null; otherwise, source itself.</returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Returns an enumeration of the specified source type.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>An enumeration of the source type </returns>
        public static IEnumerable<T> Enumerate<T>(this T source)
        {
            yield return source;
            yield break;
        }

        /// <summary>
        /// Searches for all elements that matches the conditions defined by the specified predicate, and returns the zero-based index of the all occurrence within the entire System.Collections.Generic.List{T}.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable{T}.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of the zero-based index of the all occurrence of elements that matches the conditions defined by match, if found; otherwise, empty list.</returns>
        public static List<int> FindAllIndex<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<int> result = new List<int>();

            if (source == null)
            {
                return result;
            }

            int i = 0;

            foreach (var item in source)
            {
                if (match(item))
                {
                    result.Add(i);
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Searches for all elements that matches the conditions defined by the specified predicate, and returns the zero-based index of the all occurrence within the entire System.Collections.Generic.List{T}.
        /// </summary>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of the zero-based index of the all occurrence of elements that matches the conditions defined by match, if found; otherwise, empty list.</returns>
        public static List<int> FindAllIndex(this IEnumerable source, Func<object, bool> predicate)
        {
            List<int> result = new List<int>();

            if (source == null)
            {
                return result;
            }

            int i = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result.Add(i);
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable{T} that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are started with the specified predicate element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable{T}.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable{T} that contains the sub collection of source.</returns>
        public static List<List<T>> SplitByStartsWith<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<List<T>> result = new List<List<T>>();

            List<int> indexes = source.FindAllIndex(match);

            if (indexes.Count < 1)
            {
                result.Add(source.ToList());
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
                result.Add(source.Skip(skipCount).Take(takeCount).ToList());
                skipCount = index;
            }

            result.Add(source.Skip(skipCount).ToList());

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are started with the specified predicate element.
        /// </summary>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable that contains the sub collection of source.</returns>
        public static List<List<object>> SplitByStartsWith(this IEnumerable source, Func<object, bool> predicate)
        {
            List<List<object>> result = new List<List<object>>();

            List<int> indexes = source.FindAllIndex(predicate);

            var sourceItems = source.Cast<object>();

            if (indexes.Count < 1)
            {
                result.Add(sourceItems.ToList());
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
                result.Add(sourceItems.Skip(skipCount).Take(takeCount).ToList());
                skipCount = index;
            }

            result.Add(sourceItems.Skip(skipCount).ToList());

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable{T} that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are ended with the specified predicate element.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">Source IEnumerable{T}.</param>
        /// <param name="match">The System.Predicate{T} delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable{T} that contains the sub collection of source.</returns>
        public static List<List<T>> SplitByEndsWith<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            List<List<T>> result = new List<List<T>>();

            List<int> indexes = source.FindAllIndex(match);

            if (indexes.Count < 1)
            {
                result.Add(source.ToList());
                return result;
            }

            int skipCount = 0;
            int takeCount = 0;

            foreach (int index in indexes)
            {
                takeCount = index + 1 - skipCount;
                result.Add(source.Skip(skipCount).Take(takeCount).ToList());
                skipCount = index + 1;
            }

            if (skipCount < source.Count())
            {
                result.Add(source.Skip(skipCount).ToList());
            }

            return result;
        }

        /// <summary>
        /// Returns a list of IEnumerable that contains the sub collection of source that are delimited by elements of a specified predicate.
        /// The elements of each sub collection are ended with the specified predicate element.
        /// </summary>
        /// <param name="source">Source IEnumerable.</param>
        /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>A list of IEnumerable that contains the sub collection of source.</returns>
        public static List<List<object>> SplitByEndsWith(this IEnumerable source, Func<object, bool> predicate)
        {
            List<List<object>> result = new List<List<object>>();

            List<int> indexes = source.FindAllIndex(predicate);

            var sourceItems = source.Cast<object>();

            if (indexes.Count < 1)
            {
                result.Add(sourceItems.ToList());
                return result;
            }

            int skipCount = 0;
            int takeCount = 0;

            foreach (int index in indexes)
            {
                takeCount = index + 1 - skipCount;
                result.Add(sourceItems.Skip(skipCount).Take(takeCount).ToList());
                skipCount = index + 1;
            }

            if (skipCount < sourceItems.Count())
            {
                result.Add(sourceItems.Skip(skipCount).ToList());
            }

            return result;
        }
    }
}
