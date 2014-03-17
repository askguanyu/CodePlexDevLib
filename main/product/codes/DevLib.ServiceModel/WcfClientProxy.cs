//-----------------------------------------------------------------------
// <copyright file="WcfClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Provides the implementation used to create client objects that can call services.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public static class WcfClientProxy<TChannel> where TChannel : class
    {
        /// <summary>
        /// Field WcfClientProxyDictionaryKeyStringFormat.
        /// </summary>
        private const string WcfClientProxyDictionaryKeyStringFormat = "[Key1][{0}][Key2][{1}]";

        /// <summary>
        /// Field ClientBaseInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> ClientBaseInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field PerSessionThrowableInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> PerSessionThrowableInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field PerSessionUnthrowableInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> PerSessionUnthrowableInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field PerCallThrowableInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> PerCallThrowableInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field PerCallUnthrowableInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> PerCallUnthrowableInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, remoteIPEndPoint, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, remoteHostAddress, remotePort, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, endpointConfigurationName, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, binding, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, bindingType, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, remoteIPEndPoint, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, remoteHostAddress, remotePort, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, endpointConfigurationName, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, binding, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, bindingType, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, remoteIPEndPoint, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, remoteHostAddress, remotePort, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, endpointConfigurationName, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, binding, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, bindingType, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, remoteIPEndPoint, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, remoteHostAddress, remotePort, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, endpointConfigurationName, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, binding, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, bindingType, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, remoteIPEndPoint, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, remoteHostAddress, remotePort, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, endpointConfigurationName, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, binding, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, bindingType, remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, string.Empty, string.Empty);

                TChannel result;

                lock (((ICollection)caching).SyncRoot)
                {
                    if (caching.ContainsKey(key))
                    {
                        result = caching[key];
                    }
                    else
                    {
                        Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                        result = (TChannel)Activator.CreateInstance(type);

                        caching.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return (TChannel)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, string remoteIPEndPoint, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            UriBuilder uriBuilder = new UriBuilder(remoteIPEndPoint);

            uriBuilder.Path = typeof(TChannel).FullName;

            return GetInstance<TTypeBuilder>(caching, typeof(BasicHttpBinding), uriBuilder.ToString(), fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, string remoteHostAddress, int remotePort, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHostAddress, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<TTypeBuilder>(caching, typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, string endpointConfigurationName, string remoteAddress, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)caching).SyncRoot)
                {
                    if (caching.ContainsKey(key))
                    {
                        result = caching[key];
                    }
                    else
                    {
                        Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                        result = string.IsNullOrEmpty(remoteAddress) ?
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

                        caching.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return string.IsNullOrEmpty(remoteAddress) ?
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, Binding binding, string remoteAddress, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, binding.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)caching).SyncRoot)
                {
                    if (caching.ContainsKey(key))
                    {
                        result = caching[key];
                    }
                    else
                    {
                        Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                        result = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

                        caching.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, Type bindingType, string remoteAddress, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, bindingType.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)caching).SyncRoot)
                {
                    if (caching.ContainsKey(key))
                    {
                        result = caching[key];
                    }
                    else
                    {
                        Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                        result = (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));

                        caching.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));
            }
        }
    }
}
