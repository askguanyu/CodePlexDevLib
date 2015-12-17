//-----------------------------------------------------------------------
// <copyright file="DynamicClientPerSessionUnthrowableProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection;
    using System.ServiceModel.Channels;

    /// <summary>
    /// DynamicClientPerSessionUnthrowableProxy class.
    /// </summary>
    public class DynamicClientPerSessionUnthrowableProxy : DynamicClientProxyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientPerSessionUnthrowableProxy"/> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientPerSessionUnthrowableProxy(Type proxyType, Binding binding, string remoteUri)
            : base(proxyType, binding, remoteUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientPerSessionUnthrowableProxy"/> class.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="bindingType">Type of the binding.</param>
        /// <param name="remoteUri">The remote URI.</param>
        public DynamicClientPerSessionUnthrowableProxy(Type proxyType, Type bindingType, string remoteUri)
            : base(proxyType, bindingType, remoteUri)
        {
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(string methodName, params object[] parameters)
        {
            try
            {
                return this.CallMethod(this.Proxy, methodName, parameters);
            }
            catch
            {
                this.CloseProxy();
                return null;
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodName">The name of the public method to invoke.</param>
        /// <param name="types">Method parameter types.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(string methodName, Type[] types, object[] parameters)
        {
            try
            {
                return this.CallMethod(this.Proxy, methodName, types, parameters);
            }
            catch
            {
                this.CloseProxy();
                return null;
            }
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object Call(MethodInfo methodInfo, params object[] parameters)
        {
            try
            {
                return this.CallMethod(this.Proxy, methodInfo, parameters);
            }
            catch
            {
                this.CloseProxy();
                return null;
            }
        }
    }
}
