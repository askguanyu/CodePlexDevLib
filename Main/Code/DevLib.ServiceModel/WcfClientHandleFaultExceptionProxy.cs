//-----------------------------------------------------------------------
// <copyright file="WcfClientHandleFaultExceptionProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Class WcfClientHandleFaultExceptionProxy.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    public abstract class WcfClientHandleFaultExceptionProxy<TChannel> : WcfClientReusableProxy<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxy{TChannel}" /> class.
        /// </summary>
        protected WcfClientHandleFaultExceptionProxy()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxy{TChannel}" /> class.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        protected WcfClientHandleFaultExceptionProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxy{TChannel}" /> class.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the endpoint in the application configuration file.</param>
        /// <param name="remoteAddress">The address of the service.</param>
        protected WcfClientHandleFaultExceptionProxy(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxy{TChannel}" /> class.
        /// </summary>
        /// <param name="binding">The binding with which to make calls to the service.</param>
        /// <param name="remoteAddress">The address of the service endpoint.</param>
        protected WcfClientHandleFaultExceptionProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Method HandleFaultException.
        /// </summary>
        /// <typeparam name="TDetail">The serializable error detail type.</typeparam>
        /// <param name="faultException">Instance of FaultException{TDetail}.</param>
        protected virtual void HandleFaultException<TDetail>(FaultException<TDetail> faultException)
        {
            Exception exception = faultException.Detail as Exception;

            if (exception != null)
            {
                throw exception;
            }
        }
    }
}
