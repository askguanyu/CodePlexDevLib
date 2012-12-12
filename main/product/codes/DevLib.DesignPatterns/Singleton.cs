//-----------------------------------------------------------------------
// <copyright file="Singleton.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Singleton Design Pattern
    /// </summary>
    public sealed class Singleton<T> where T : new()
    {
        /// <summary>
        ///
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        ///
        /// </summary>
        private static T _instance;

        /// <summary>
        ///
        /// </summary>
        private Singleton() { }

        /// <summary>
        /// Gets singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
