//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection;
    using System.ServiceModel.Channels;

    /// <summary>
    /// DynamicClientProxy class.
    /// </summary>
    [Serializable]
    public class DynamicClientProxy : DynamicClientProxyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        /// <param name="binding">Service client binding.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        public DynamicClientProxy(Type proxyType, Binding binding, string remoteUri)
            : base(proxyType, binding, remoteUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxy" /> class.
        /// </summary>
        /// <param name="proxyType">Service client type.</param>
        /// <param name="bindingType">Service client binding type.</param>
        /// <param name="remoteUri">The URI that identifies the service endpoint.</param>
        public DynamicClientProxy(Type proxyType, Type bindingType, string remoteUri)
            : base(proxyType, bindingType, remoteUri)
        {
        }

        /// <summary>
        /// Invokes the method represented by the current object, using the specified parameters.
        /// </summary>
        /// <param name="methodInfo">A <see cref="T:System.Reflection.MethodInfo" /> object representing the method.</param>
        /// <param name="parameters">An argument list for the invoked method.</param>
        /// <returns>An object containing the return value of the invoked method.</returns>
        public override object CallMethod(MethodInfo methodInfo, params object[] parameters)
        {
            try
            {
                return base.CallMethod(methodInfo, parameters);
            }
            catch (Exception e)
            {
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
