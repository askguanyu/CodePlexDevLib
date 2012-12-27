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
    public sealed class Singleton<T> where T : new()
    {
        /// <summary>
        ///
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
                return Inner._instance;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class Inner
        {
            /// <summary>
            ///
            /// </summary>
            internal static T _instance = new T();

            /// <summary>
            ///
            /// </summary>
            static Inner()
            {
            }
        }
    }
}
