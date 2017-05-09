//-----------------------------------------------------------------------
// <copyright file="GenericEqualityComparer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace System
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Generic methods to support the comparison of objects for equality.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    public class GenericEqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        /// <summary>
        /// The key selectors.
        /// </summary>
        private readonly Converter<T, object>[] _keySelectors;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="keySelectors">The key selectors.</param>
        public GenericEqualityComparer(params Converter<T, object>[] keySelectors)
        {
            this._keySelectors = keySelectors;
        }

        /// <summary>
        /// Gets a new instance of the <see cref="GenericEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="keySelectors">The key selectors.</param>
        /// <returns>GenericEqualityComparer instance.</returns>
        public static GenericEqualityComparer<T> By(params Converter<T, object>[] keySelectors)
        {
            return new GenericEqualityComparer<T>(keySelectors);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(object obj)
        {
            int result = 0;

            if (obj != null)
            {
                foreach (var keySelector in this._keySelectors)
                {
                    result ^= keySelector((T)obj).GetHashCode();
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null | y == null)
            {
                return false;
            }
            else if (this.AllKeysEquals(x, y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(T obj)
        {
            int result = 0;

            if (obj != null)
            {
                foreach (var keySelector in this._keySelectors)
                {
                    result ^= keySelector(obj).GetHashCode();
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        bool IEqualityComparer.Equals(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null | y == null)
            {
                return false;
            }
            else if (this.AllKeysEquals((T)x, (T)y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether all the specified keys are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>true if all the specified keys are equal; otherwise, false.</returns>
        private bool AllKeysEquals(T x, T y)
        {
            foreach (var keySelector in this._keySelectors)
            {
                if (!keySelector(x).Equals(keySelector(y)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
