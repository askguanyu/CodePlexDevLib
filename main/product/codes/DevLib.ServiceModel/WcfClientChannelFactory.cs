//-----------------------------------------------------------------------
// <copyright file="WcfClientChannelFactory.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

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
        /// Creates a channel of a specified type to a specified endpoint address.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, string.Empty, string.Empty);

                ChannelFactory<TChannel> result;

                lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        result = ChannelFactoryDictionary[key];
                    }
                    else
                    {
                        result = new ChannelFactory<TChannel>();

                        ChannelFactoryDictionary.Add(key, result);
                    }
                }

                return result.CreateChannel();
            }
            else
            {
                return new ChannelFactory<TChannel>().CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel of a specified type to a specified endpoint address.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHostAddress, remotePort, typeof(TChannel).FullName).ToString();

            return CreateChannel(typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service whose endpoint is configured in a specified way.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string remoteIPEndPoint, bool fromCaching = true)
        {
            UriBuilder uriBuilder = new UriBuilder(remoteIPEndPoint);

            uriBuilder.Path = typeof(TChannel).FullName;

            return CreateChannel(typeof(BasicHttpBinding), uriBuilder.ToString(), fromCaching);
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service at a specified name for the endpoint configuration and remote address.
        /// </summary>
        /// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                ChannelFactory<TChannel> result;

                lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        result = ChannelFactoryDictionary[key];
                    }
                    else
                    {
                        result = string.IsNullOrEmpty(remoteAddress) ?
                            new ChannelFactory<TChannel>(endpointConfigurationName) :
                            new ChannelFactory<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress));

                        ChannelFactoryDictionary.Add(key, result);
                    }
                }

                return result.CreateChannel();
            }
            else
            {
                return (string.IsNullOrEmpty(remoteAddress) ?
                    new ChannelFactory<TChannel>(endpointConfigurationName) :
                    new ChannelFactory<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress))).CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> used to configure the endpoint.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, binding.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                ChannelFactory<TChannel> result;

                lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        result = ChannelFactoryDictionary[key];
                    }
                    else
                    {
                        result = new ChannelFactory<TChannel>(binding, new EndpointAddress(remoteAddress));

                        ChannelFactoryDictionary.Add(key, result);
                    }
                }

                return result.CreateChannel();
            }
            else
            {
                return new ChannelFactory<TChannel>(binding, new EndpointAddress(remoteAddress)).CreateChannel();
            }
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, bindingType.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                ChannelFactory<TChannel> result;

                lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
                {
                    if (ChannelFactoryDictionary.ContainsKey(key))
                    {
                        result = ChannelFactoryDictionary[key];
                    }
                    else
                    {
                        result = new ChannelFactory<TChannel>(WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));

                        ChannelFactoryDictionary.Add(key, result);
                    }
                }

                return result.CreateChannel();
            }
            else
            {
                return new ChannelFactory<TChannel>(WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress)).CreateChannel();
            }
        }
    }
}
