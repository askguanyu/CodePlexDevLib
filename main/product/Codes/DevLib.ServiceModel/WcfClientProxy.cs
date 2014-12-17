//-----------------------------------------------------------------------
// <copyright file="WcfClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

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
        /// Field Lock.
        /// </summary>
        private static readonly ReaderWriterLock Lock = new ReaderWriterLock();

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
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(string endpointConfigurationName, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetClientBaseInstance(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientClientBaseClassBuilder<TChannel>>(ClientBaseInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
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
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(string endpointConfigurationName, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallThrowableInstance(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallThrowableClassBuilder<TChannel>>(PerCallThrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
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
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(string endpointConfigurationName, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. A new instance is created for each call then disposed and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerCallUnthrowableInstance(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerCallUnthrowableClassBuilder<TChannel>>(PerCallUnthrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
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
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(string endpointConfigurationName, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will throw exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionThrowableInstance(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionThrowableClassBuilder<TChannel>>(PerSessionThrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
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
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string endpointConfigurationName, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(string endpointConfigurationName, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, endpointConfigurationName, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Binding binding, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Binding binding, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, binding, remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Type bindingType, string remoteUri, bool fromCaching = true)
        {
            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy is reused for each session and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteHost">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetPerSessionUnthrowableInstance(Type bindingType, string remoteHost, int remotePort, bool fromCaching = true)
        {
            if (remotePort < IPEndPoint.MinPort || remotePort > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("remotePort", remotePort, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
            }

            string remoteUri = new UriBuilder(Uri.UriSchemeHttp, remoteHost, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance<WcfClientPerSessionUnthrowableClassBuilder<TChannel>>(PerSessionUnthrowableInstanceDictionary, WcfServiceType.GetBinding(bindingType), remoteUri, fromCaching);
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

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (caching.ContainsKey(key))
                    {
                        return caching[key];
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (caching.ContainsKey(key))
                            {
                                return caching[key];
                            }
                            else
                            {
                                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                                TChannel result = (TChannel)Activator.CreateInstance(type);

                                caching.Add(key, result);

                                return result;
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
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return (TChannel)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, string remoteUri, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            return GetInstance<TTypeBuilder>(caching, WcfServiceType.GetBinding(typeof(BasicHttpBinding)), remoteUri, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, string endpointConfigurationName, string remoteUri, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteUri) ? string.Empty : remoteUri.ToLowerInvariant());

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (caching.ContainsKey(key))
                    {
                        return caching[key];
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (caching.ContainsKey(key))
                            {
                                return caching[key];
                            }
                            else
                            {
                                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                                TChannel result = string.IsNullOrEmpty(remoteUri) ?
                                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteUri));

                                caching.Add(key, result);

                                return result;
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
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return string.IsNullOrEmpty(remoteUri) ?
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteUri));
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <param name="caching">Caching dictionary to use.</param>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        private static TChannel GetInstance<TTypeBuilder>(Dictionary<string, TChannel> caching, Binding binding, string remoteUri, bool fromCaching = true) where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientProxyDictionaryKeyStringFormat, binding.GetHashCode().ToString(), string.IsNullOrEmpty(remoteUri) ? string.Empty : remoteUri.ToLowerInvariant());

                Lock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (caching.ContainsKey(key))
                    {
                        return caching[key];
                    }
                    else
                    {
                        LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                        try
                        {
                            if (caching.ContainsKey(key))
                            {
                                return caching[key];
                            }
                            else
                            {
                                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                                TChannel result = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteUri));

                                caching.Add(key, result);

                                return result;
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
                Type type = WcfClientType.BuildType<TChannel, TTypeBuilder>();

                return (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteUri));
            }
        }
    }
}
