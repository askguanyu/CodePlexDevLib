//-----------------------------------------------------------------------
// <copyright file="ServiceLocatorImplBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class is a helper that provides a default implementation for most of the methods of <see cref="IServiceLocator"/>.
    /// </summary>
    public abstract class ServiceLocatorImplBase : IServiceLocator
    {
        /// <summary>
        /// Get an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        public virtual object GetInstance(Type serviceType, bool createNew = false)
        {
            try
            {
                return this.DoGetInstance(serviceType, createNew);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Get an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        public virtual TService GetInstance<TService>(bool createNew = false)
        {
            return (TService)this.GetInstance(typeof(TService), createNew);
        }

        /// <summary>
        /// Get an instance of the given named <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="key">Name the object was registered with.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        public virtual object GetInstance(Type serviceType, string key, bool createNew = false)
        {
            try
            {
                return this.DoGetInstance(serviceType, key, createNew);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Get an instance of the given named <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="key">Name the object was registered with.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        public virtual TService GetInstance<TService>(string key, bool createNew = false)
        {
            return (TService)this.GetInstance(typeof(TService), key, createNew);
        }

        /// <summary>
        /// Get all instances of the given <paramref name="serviceType"/> currently registered in the container.
        /// </summary>
        /// <param name="serviceType">Type of object requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>.</returns>
        public virtual IEnumerable<object> GetAllInstances(Type serviceType, bool createNew = false)
        {
            try
            {
                return this.DoGetAllInstances(serviceType, createNew);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Get all instances of the given <typeparamref name="TService"/> currently
        /// registered in the container.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>A sequence of instances of the requested <typeparamref name="TService"/>.</returns>
        public virtual IEnumerable<TService> GetAllInstances<TService>(bool createNew = false)
        {
            try
            {
                return this.DoGetAllInstances<TService>(createNew);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Implementation of <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="serviceType">The requested service.</param>
        /// <returns>The requested object.</returns>
        public virtual object GetService(Type serviceType)
        {
            return this.GetInstance(serviceType, null, false);
        }

        /// <summary>
        /// Implementation of <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="serviceType">The requested service.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested object.</returns>
        public virtual object GetService(Type serviceType, bool createNew = false)
        {
            return this.GetInstance(serviceType, null, createNew);
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        protected abstract object DoGetInstance(Type serviceType, bool createNew = false);

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>The requested service instance.</returns>
        protected abstract object DoGetInstance(Type serviceType, string key, bool createNew = false);

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType, bool createNew = false);

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving all the requested service instances.
        /// </summary>
        /// <typeparam name="TService">Type of object requested.</typeparam>
        /// <param name="createNew">true to create new instance if possible every time; otherwise, get the same instance every time.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected virtual IEnumerable<TService> DoGetAllInstances<TService>(bool createNew = false)
        {
            foreach (object item in this.GetAllInstances(typeof(TService), createNew))
            {
                yield return (TService)item;
            }
        }
    }
}
