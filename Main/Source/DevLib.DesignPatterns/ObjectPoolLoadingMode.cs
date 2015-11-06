//-----------------------------------------------------------------------
// <copyright file="ObjectPoolLoadingMode.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// ObjectPool loading mode.
    /// </summary>
    public enum ObjectPoolLoadingMode
    {
        /// <summary>
        /// Represents eager loading.
        /// </summary>
        Eager,

        /// <summary>
        /// Represents lazy loading.
        /// </summary>
        Lazy,

        /// <summary>
        /// Represents lazy loading with expanding.
        /// </summary>
        LazyExpanding
    }
}
