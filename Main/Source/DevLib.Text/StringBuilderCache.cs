//-----------------------------------------------------------------------
// <copyright file="StringBuilderCache.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Text
{
    using System;
    using System.Text;

    /// <summary>
    /// Provide a cached reusable instance of StringBuilder per thread.
    /// It's an optimization that reduces the number of instances constructed and collected.
    /// </summary>
    public static class StringBuilderCache
    {
        /// <summary>
        /// The value 360 was chosen in discussion with performance experts as a compromise between using as little memory (per thread) as possible and still covering a large part of short-lived StringBuilder creations on the startup path of VS designers.
        /// </summary>
        private const int MaxSize = 360;

        /// <summary>
        /// Field DefaultCapacity.
        /// </summary>
        private const int DefaultCapacity = 16;

        /// <summary>
        /// Field CachedInstance.
        /// </summary>
        [ThreadStatic]
        private static StringBuilder CachedInstance;

        /// <summary>
        /// Get a string builder to use of a particular size.
        /// It can be called any number of times, if a StringBuilder is in the cache then it will be returned and the cache emptied.
        /// Subsequent calls will return a new StringBuilder.
        /// </summary>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <returns>StringBuilder instance.</returns>
        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxSize)
            {
                StringBuilder stringBuilder = StringBuilderCache.CachedInstance;

                if (stringBuilder != null)
                {
                    if (capacity <= stringBuilder.Capacity)
                    {
                        StringBuilderCache.CachedInstance = null;
                        stringBuilder.Length = 0;

                        return stringBuilder;
                    }
                }
            }

            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Place the specified builder in the cache if it is not too big.
        /// The StringBuilder should not be used after it has been released.
        /// Unbalanced releases are perfectly acceptable.
        /// It will merely cause the runtime to create a new StringBuilder next time Acquire is called.
        /// </summary>
        /// <param name="stringBuilder">StringBuilder instance to release.</param>
        public static void Release(StringBuilder stringBuilder)
        {
            if (stringBuilder.Capacity <= MaxSize)
            {
                StringBuilderCache.CachedInstance = stringBuilder;
            }
        }

        /// <summary>
        /// Call ToString() of the StringBuilder, release it to the cache and return the resulting string.
        /// </summary>
        /// <param name="stringBuilder">StringBuilder to get and release.</param>
        /// <returns>A string whose value is the same as this instance.</returns>
        public static string GetStringAndRelease(StringBuilder stringBuilder)
        {
            string result = stringBuilder.ToString();
            Release(stringBuilder);

            return result;
        }
    }
}
