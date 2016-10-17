//-----------------------------------------------------------------------
// <copyright file="IServiceLocator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The generic Service Locator interface. This interface is used to retrieve services
    /// (instances identified by type and optional name) from a container.
    /// </summary>
    public interface IServiceLocator : IServiceProvider
    {
        /// <summary>
        /// Get an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType, bool createNew = false);

        /// <summary>
        /// Get an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        TService GetInstance<TService>(bool createNew = false);

        /// <summary>
        /// Get an instance of the given named <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="key">Name the object was registered with.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType, string key, bool createNew = false);

        /// <summary>
        /// Get an instance of the given named <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="key">Name the object was registered with.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        TService GetInstance<TService>(string key, bool createNew = false);

        /// <summary>
        /// Get all instances of the given <paramref name="serviceType"/> currently registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>.</returns>
        IEnumerable<object> GetAllInstances(Type serviceType, bool createNew = false);

        /// <summary>
        /// Get all instances of the given <typeparamref name="TService"/> currently
        /// registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>A sequence of instances of the requested <typeparamref name="TService"/>.</returns>
        IEnumerable<TService> GetAllInstances<TService>(bool createNew = false);
    }
}
