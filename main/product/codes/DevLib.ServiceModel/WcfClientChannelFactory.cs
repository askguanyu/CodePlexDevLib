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
        /// Field _ChannelFactoryDictionary.
        /// </summary>
        private static readonly Dictionary<string, ChannelFactory<TChannel>> ChannelFactoryDictionary = new Dictionary<string, ChannelFactory<TChannel>>();

        /// <summary>
        /// Creates a channel of a specified type to a specified endpoint address.
        /// </summary>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TChannel CreateChannel()
        {
            return new ChannelFactory<TChannel>().CreateChannel();
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service whose endpoint is configured in a specified way.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint configuration used for the service.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string endpointConfigurationName)
        {
            string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.Empty);

            ChannelFactory<TChannel> result;

            lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
            {
                if (ChannelFactoryDictionary.ContainsKey(key))
                {
                    result = ChannelFactoryDictionary[key];
                }
                else
                {
                    ChannelFactoryDictionary.Add(key, new ChannelFactory<TChannel>(endpointConfigurationName));
                    result = ChannelFactoryDictionary[key];
                }
            }

            return result.CreateChannel();
        }

        /// <summary>
        /// Creates a channel that is used to send messages to a service at a specified name for the endpoint configuration and remote address.
        /// </summary>
        /// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(string endpointConfigurationName, string remoteAddress)
        {
            EndpointAddress endpointAddress = new EndpointAddress(remoteAddress);

            string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, endpointAddress.GetHashCode());

            ChannelFactory<TChannel> result;

            lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
            {
                if (ChannelFactoryDictionary.ContainsKey(key))
                {
                    result = ChannelFactoryDictionary[key];
                }
                else
                {
                    ChannelFactoryDictionary.Add(key, new ChannelFactory<TChannel>(endpointConfigurationName, endpointAddress));
                    result = ChannelFactoryDictionary[key];
                }
            }

            return result.CreateChannel();
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> used to configure the endpoint.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(Binding binding, string remoteAddress)
        {
            EndpointAddress endpointAddress = new EndpointAddress(remoteAddress);

            string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, binding.GetHashCode(), endpointAddress.GetHashCode());

            ChannelFactory<TChannel> result;

            lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
            {
                if (ChannelFactoryDictionary.ContainsKey(key))
                {
                    result = ChannelFactoryDictionary[key];
                }
                else
                {
                    ChannelFactoryDictionary.Add(key, new ChannelFactory<TChannel>(binding, endpointAddress));
                    result = ChannelFactoryDictionary[key];
                }
            }

            return result.CreateChannel();
        }

        /// <summary>
        /// Creates a channel of a specified type that is used to send messages to a service endpoint that is configured with a specified binding.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address that provides the location of the service.</param>
        /// <returns>The <paramref name="TChannel" /> of type <see cref="T:System.ServiceModel.Channels.IChannel" /> created by the factory.</returns>
        public static TChannel CreateChannel(Type bindingType, string remoteAddress)
        {
            EndpointAddress endpointAddress = new EndpointAddress(remoteAddress);

            Binding binding = WcfServiceType.GetBinding(bindingType);

            string key = string.Format(ChannelFactoryDictionaryKeyStringFormat, bindingType.GetHashCode(), endpointAddress.GetHashCode());

            ChannelFactory<TChannel> result;

            lock (((ICollection)ChannelFactoryDictionary).SyncRoot)
            {
                if (ChannelFactoryDictionary.ContainsKey(key))
                {
                    result = ChannelFactoryDictionary[key];
                }
                else
                {
                    ChannelFactoryDictionary.Add(key, new ChannelFactory<TChannel>(binding, endpointAddress));
                    result = ChannelFactoryDictionary[key];
                }
            }

            return result.CreateChannel();
        }
    }
}
