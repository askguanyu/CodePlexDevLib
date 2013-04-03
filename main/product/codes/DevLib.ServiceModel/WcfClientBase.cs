//-----------------------------------------------------------------------
// <copyright file="WcfClientBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Provides the implementation used to create client objects that can call services.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public static class WcfClientBase<TChannel> where TChannel : class
    {
        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance()
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(string endpointConfigurationName)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(string endpointConfigurationName, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetInstance(Binding binding, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientClientBaseClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted.
        /// </summary>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance()
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(string endpointConfigurationName)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(string endpointConfigurationName, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableInstance(Binding binding, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientReusableProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance()
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(string endpointConfigurationName)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName);

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(string endpointConfigurationName, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, endpointConfigurationName, new EndpointAddress(remoteAddress));

            return instance;
        }

        /// <summary>
        /// Get Wcf client instance. This instance of the proxy can be reused if the channel is faulted and unwrap any FaultException and throw the original Exception.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        /// <returns>Instance of a ClientBase derived class.</returns>
        public static TChannel GetReusableFaultUnwrappingInstance(Binding binding, string remoteAddress)
        {
            Type type = WcfClientProxyTypeBuilder.BuildType<TChannel, WcfClientHandleFaultExceptionProxyClassBuilder<TChannel>>();

            TChannel instance = (TChannel)Activator.CreateInstance(type, binding, new EndpointAddress(remoteAddress));

            return instance;
        }
    }
}
