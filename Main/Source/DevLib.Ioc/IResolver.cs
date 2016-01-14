//-----------------------------------------------------------------------
// <copyright file="IResolver.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;

    /// <summary>
    /// Interface for a resolving context.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The registered instance.</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The registered instance.</returns>
        T Resolve<T>();

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>The registered instance.</returns>
        object Resolve(Type type, string name);

        /// <summary>
        /// Resolves the specified type with the specified name.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="name">The name of registration to resolve.</param>
        /// <returns>The registered instance.</returns>
        T Resolve<T>(string name);
    }
}
