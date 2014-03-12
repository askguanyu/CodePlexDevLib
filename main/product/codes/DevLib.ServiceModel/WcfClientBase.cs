//-----------------------------------------------------------------------
// <copyright file="WcfClientBase.cs" company="YuGuan Corporation">
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
    public static class WcfClientBase<TChannel> where TChannel : class
    {
        /// <summary>
        /// Field WcfClientBaseDictionaryKeyStringFormat.
        /// </summary>
        private const string WcfClientBaseDictionaryKeyStringFormat = "[Key1][{0}][Key2][{1}]";

        /// <summary>
        /// Field InstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> InstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field ReusableInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> ReusableInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Field ReusableFaultUnwrappingInstanceDictionary.
        /// </summary>
        private static readonly Dictionary<string, TChannel> ReusableFaultUnwrappingInstanceDictionary = new Dictionary<string, TChannel>();

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, string.Empty, string.Empty);

                TChannel result;

                lock (((ICollection)InstanceDictionary).SyncRoot)
                {
                    if (InstanceDictionary.ContainsKey(key))
                    {
                        result = InstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type);

                        InstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            UriBuilder uriBuilder = new UriBuilder(remoteIPEndPoint);

            uriBuilder.Path = typeof(TChannel).FullName;

            return GetInstance(typeof(BasicHttpBinding), uriBuilder.ToString(), fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHostAddress, remotePort, typeof(TChannel).FullName).ToString();

            return GetInstance(typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)InstanceDictionary).SyncRoot)
                {
                    if (InstanceDictionary.ContainsKey(key))
                    {
                        result = InstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                        result = string.IsNullOrEmpty(remoteAddress) ?
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

                        InstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                return string.IsNullOrEmpty(remoteAddress) ?
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, binding.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)InstanceDictionary).SyncRoot)
                {
                    if (InstanceDictionary.ContainsKey(key))
                    {
                        result = InstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

                        InstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, bindingType.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)InstanceDictionary).SyncRoot)
                {
                    if (InstanceDictionary.ContainsKey(key))
                    {
                        result = InstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));

                        InstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, string.Empty, string.Empty);

                TChannel result;

                lock (((ICollection)ReusableInstanceDictionary).SyncRoot)
                {
                    if (ReusableInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type);

                        ReusableInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            UriBuilder uriBuilder = new UriBuilder(remoteIPEndPoint);

            uriBuilder.Path = typeof(TChannel).FullName;

            return GetReusableInstance(typeof(BasicHttpBinding), uriBuilder.ToString(), fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHostAddress, remotePort, typeof(TChannel).FullName).ToString();

            return GetReusableInstance(typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableInstanceDictionary).SyncRoot)
                {
                    if (ReusableInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                        result = string.IsNullOrEmpty(remoteAddress) ?
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

                        ReusableInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                return string.IsNullOrEmpty(remoteAddress) ?
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, binding.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableInstanceDictionary).SyncRoot)
                {
                    if (ReusableInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

                        ReusableInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and will not throw any exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, bindingType.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableInstanceDictionary).SyncRoot)
                {
                    if (ReusableInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));

                        ReusableInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, string.Empty, string.Empty);

                TChannel result;

                lock (((ICollection)ReusableFaultUnwrappingInstanceDictionary).SyncRoot)
                {
                    if (ReusableFaultUnwrappingInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableFaultUnwrappingInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type);

                        ReusableFaultUnwrappingInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type);
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="remoteIPEndPoint">The IP endpoint of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(string remoteIPEndPoint, bool fromCaching = true)
        {
            UriBuilder uriBuilder = new UriBuilder(remoteIPEndPoint);

            uriBuilder.Path = typeof(TChannel).FullName;

            return GetReusableFaultUnwrappingInstance(typeof(BasicHttpBinding), uriBuilder.ToString(), fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="remoteHostAddress">The host address of the service endpoint.</param>
        /// <param name="remotePort">The port number of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(string remoteHostAddress, int remotePort, bool fromCaching = true)
        {
            string remoteAddress = new UriBuilder(Uri.UriSchemeHttp, remoteHostAddress, remotePort, typeof(TChannel).FullName).ToString();

            return GetReusableFaultUnwrappingInstance(typeof(BasicHttpBinding), remoteAddress, fromCaching);
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(string endpointConfigurationName, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, endpointConfigurationName ?? string.Empty, string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableFaultUnwrappingInstanceDictionary).SyncRoot)
                {
                    if (ReusableFaultUnwrappingInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableFaultUnwrappingInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                        result = string.IsNullOrEmpty(remoteAddress) ?
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                            (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

                        ReusableFaultUnwrappingInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                return string.IsNullOrEmpty(remoteAddress) ?
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName) :
                    (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(Binding binding, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, binding.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableFaultUnwrappingInstanceDictionary).SyncRoot)
                {
                    if (ReusableFaultUnwrappingInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableFaultUnwrappingInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

                        ReusableFaultUnwrappingInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));
            }
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <param name="fromCaching">Whether get instance from caching or not.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(Type bindingType, string remoteAddress, bool fromCaching = true)
        {
            if (fromCaching)
            {
                string key = string.Format(WcfClientBaseDictionaryKeyStringFormat, bindingType.GetHashCode(), string.IsNullOrEmpty(remoteAddress) ? string.Empty : remoteAddress.ToLowerInvariant());

                TChannel result;

                lock (((ICollection)ReusableFaultUnwrappingInstanceDictionary).SyncRoot)
                {
                    if (ReusableFaultUnwrappingInstanceDictionary.ContainsKey(key))
                    {
                        result = ReusableFaultUnwrappingInstanceDictionary[key];
                    }
                    else
                    {
                        Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                        result = (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));

                        ReusableFaultUnwrappingInstanceDictionary.Add(key, result);
                    }
                }

                return result;
            }
            else
            {
                Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

                return (TChannel)Activator.CreateInstance(type, WcfServiceType.GetBinding(bindingType), new EndpointAddress(remoteAddress));
            }
        }
    }
}
