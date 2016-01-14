//-----------------------------------------------------------------------
// <copyright file="ServiceLocator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;

    /// <summary>
    /// This delegate type is used to provide a method that will return the current container. Used with the <see cref="T:DevLib.Ioc.ServiceLocator" /> static accessor class.
    /// </summary>
    /// <returns>An <see cref="T:DevLib.Ioc.IServiceLocator" />.</returns>
    public delegate IServiceLocator ServiceLocatorProvider();

    /// <summary>
    /// This class provides the ambient container for this application.
    /// If your framework defines such an ambient container, use ServiceLocator.Current to get it.
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// Static field CurrentProvider.
        /// </summary>
        private static ServiceLocatorProvider CurrentProvider;

        /// <summary>
        /// Gets the current ambient container.
        /// </summary>
        public static IServiceLocator Current
        {
            get
            {
                if (!ServiceLocator.IsServiceLocatorProviderSet)
                {
                    throw new InvalidOperationException("ServiceLocationProvider must be set.");
                }

                return ServiceLocator.CurrentProvider();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ServiceLocatorProvider is set.
        /// </summary>
        public static bool IsServiceLocatorProviderSet
        {
            get
            {
                return ServiceLocator.CurrentProvider != null;
            }
        }

        /// <summary>
        /// Set the delegate that is used to retrieve the current container.
        /// </summary>
        /// <param name="newProvider">Delegate that, when called, will return the current ambient container.</param>
        public static void SetLocatorProvider(ServiceLocatorProvider newProvider)
        {
            ServiceLocator.CurrentProvider = newProvider;
        }
    }
}
