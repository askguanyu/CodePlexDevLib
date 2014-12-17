//-----------------------------------------------------------------------
// <copyright file="DynamicClientPerSessionThrowableProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    /// DynamicClientPerSessionThrowableProxy class.
    /// </summary>
    public class DynamicClientPerSessionThrowableProxy : DynamicClientProxyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientPerSessionThrowableProxy"/> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientPerSessionThrowableProxy(Type proxyType, Binding binding, string remoteUri)
            : base(proxyType, binding, remoteUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientPerSessionThrowableProxy"/> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="bindingType">Type of the binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientPerSessionThrowableProxy(Type proxyType, Type bindingType, string remoteUri)
            : base(proxyType, bindingType, remoteUri)
        {
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object CallMethod(string methodName, params object[] parameters)
        {
            try
            {
                return base.CallMethod(methodName, parameters);
            }
            catch (Exception e)
            {
                this.CloseProxy();

                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object CallMethod(string methodName, Type[] types, object[] parameters)
        {
            try
            {
                return base.CallMethod(methodName, types, parameters);
            }
            catch (Exception e)
            {
                this.CloseProxy();

                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object CallMethod(System.Reflection.MethodInfo methodInfo, params object[] parameters)
        {
            try
            {
                return base.CallMethod(methodInfo, parameters);
            }
            catch (Exception e)
            {
                this.CloseProxy();

                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
