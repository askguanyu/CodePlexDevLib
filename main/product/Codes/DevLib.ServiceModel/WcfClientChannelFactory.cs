//-----------------------------------------------------------------------
// <copyright file="WcfClientChannelFactory.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    /// <summary>
    /// A factory that creates channels of different types that are used by clients to send messages to variously configured service endpoints.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public static class WcfClientChannelFactory<TChannel> where TChannel : class
    {
        /// <summary>
        /// Field ChannelFactoryDictionaryKeyStringFormat.
        /// </summary>
        private const string ChannelFactoryDictionaryKeyStringFormat = "[Key1][{0}][Key2][{1}]";

        /// <summary>
        /// Field ChannelFactoryDictionary.
        /// </summary>
        private static readonly Dictionary<string, ChannelFactory<TChannel>> ChannelFactoryDictionary = new Dictionary<string, ChannelFactory<TChannel>>();

        /// <summary>
        /// Field Lock.
        /// </summary>
        private static readonly ReaderWriterLock Lock = new ReaderWriterLock();

        /// <summary>
        /// Creates a channel of a specified type to a specified endpoint address.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, string.Empty, string.Empty);

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        return ChannelFactoryDictionary[key].CreateChannel();
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (ChannelFactoryDictionary.ContainsKey(key))
                            {
                                return ChannelFactoryDictionary[key].CreateChannel();
                            }
                            else
                            {
                                ChannelFactory<TChannel> result = new ChannelFactory<TChannel>();

                                ChannelFactoryDictionary.Add(key, result);

                                return result.CreateChannel();
                            }
                        }
                        finally
                        {
                            Lock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }
                }
                finally
                {
                    Lock.ReleaseReaderLock();
                }
            }
            else
            {
                return new ChannelFactory<TChannel>().CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service whose endpoint is configured in a specified way.
        /// </summary>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string remoteUri, bool fromCaching = true)
        {
            return CreateChannel(typeof(BasicHttpBinding), remoteUri, fromCaching);
        }

        /// <summary>
        /// Creates a channel of a specified type to a specified endpoint address.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string remoteHost, int remotePort, bool fromCaching = true)
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return CreateChannel(typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service at a specified name for the endpoint configuration and remote address.
        /// </summary>
        /// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteUri) ? string.Empty : remoteUri.ToLowerInvariant());

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        return ChannelFactoryDictionary[key].CreateChannel();
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (ChannelFactoryDictionary.ContainsKey(key))
                            {
                                return ChannelFactoryDictionary[key].CreateChannel();
                            }
                            else
                            {
                                ChannelFactory<TChannel> result = string.IsNullOrEmpty(remoteUri) ?
                                    new ChannelFactory<TChannel>(endpointConfigurationName) :
                                    new ChannelFactory<TChannel>(endpointConfigurationName, new EndpointAddress(remoteUri));

                                ChannelFactoryDictionary.Add(key, result);

                                return result.CreateChannel();
                            }
                        }
                        finally
                        {
                            Lock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }
                }
                finally
                {
                    Lock.ReleaseReaderLock();
                }
            }
            else
            {
                return (string.IsNullOrEmpty(remoteUri) ?
                    new ChannelFactory<TChannel>(endpointConfigurationName) :
                    new ChannelFactory<TChannel>(endpointConfigurationName, new EndpointAddress(remoteUri))).CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> used to configure the endpoint.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(Binding binding, string remoteUri, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, binding.GetHashCode().ToString(), string.IsNullOrEmpty(remoteUri) ? string.Empty : remoteUri.ToLowerInvariant());

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        return ChannelFactoryDictionary[key].CreateChannel();
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (ChannelFactoryDictionary.ContainsKey(key))
                            {
                                return ChannelFactoryDictionary[key].CreateChannel();
                            }
                            else
                            {
                                ChannelFactory<TChannel> result = new ChannelFactory<TChannel>(binding, new EndpointAddress(remoteUri));

                                ChannelFactoryDictionary.Add(key, result);

                                return result.CreateChannel();
                            }
                        }
                        finally
                        {
                            Lock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }
                }
                finally
                {
                    Lock.ReleaseReaderLock();
                }
            }
            else
            {
                return new ChannelFactory<TChannel>(binding, new EndpointAddress(remoteUri)).CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <typeparamref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, bindingType.GetHashCode().ToString(), string.IsNullOrEmpty(remoteUri) ? string.Empty : remoteUri.ToLowerInvariant());

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        return ChannelFactoryDictionary[key].CreateChannel();
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (ChannelFactoryDictionary.ContainsKey(key))
                            {
                                return ChannelFactoryDictionary[key].CreateChannel();
                            }
                            else
                            {
                                ChannelFactory<TChannel> result = new ChannelFactory<TChannel>(WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteUri));

                                ChannelFactoryDictionary.Add(key, result);

                                return result.CreateChannel();
                            }
                        }
                        finally
                        {
                            Lock.DowngradeFromWriterLock(ref lockCookie);
                        }
                    }
                }
                finally
                {
                    Lock.ReleaseReaderLock();
                }
            }
            else
            {
                return new ChannelFactory<TChannel>(WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteUri)).CreateChannel();
            }
        }
    }
}
