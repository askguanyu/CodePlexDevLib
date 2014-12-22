//-----------------------------------------------------------------------
// <copyright file="Singleton.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Singleton Design Pattern, thread-safe without using locks.
    /// </summary>
    /// <typeparam name="T">Type of instance.</typeparam>
    public sealed class Singleton<T> where T : new()
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Singleton{T}" /> class from being created.
        /// </summary>
        private Singleton()
        {
        }

        /// <summary>
        /// Gets singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                return Inner.InnerInstance;
            }
        }

        /// <summary>
        /// Class Inner.
        /// </summary>
        private class Inner
        {
            /// <summary>
            /// Static Field InnerInstance.
            /// </summary>
            internal static T InnerInstance = new T();

            /// <summary>
            /// Initializes static members of the <see cref="Inner" /> class.
            /// </summary>
            static Inner()
            {
            }
        }
    }
}
