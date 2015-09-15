//-----------------------------------------------------------------------
// <copyright file="WcfClientProxyExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    /// <summary>
    /// WcfClientBase extension methods.
    /// </summary>
    public static class WcfClientProxyExtensions
    {
        /// <summary>
        /// Configure the client binding.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source TChannel.</param>
        /// <param name="setBindingAction">A delegate to configure Binding.</param>
        public static void SetBinding<TChannel>(this TChannel source, Action<Binding> setBindingAction) where TChannel : class
        {
            WcfClientBase<TChannel> wcfClient = source as WcfClientBase<TChannel>;

            if (wcfClient != null)
            {
                wcfClient.SetBindingAction = setBindingAction;
            }
        }

        /// <summary>
        /// Configure the client credentials.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source TChannel.</param>
        /// <param name="setClientCredentialsAction">A delegate to configure ClientCredentials.</param>
        public static void SetClientCredentials<TChannel>(this TChannel source, Action<ClientCredentials> setClientCredentialsAction) where TChannel : class
        {
            WcfClientBase<TChannel> wcfClient = source as WcfClientBase<TChannel>;

            if (wcfClient != null)
            {
                wcfClient.SetClientCredentialsAction = setClientCredentialsAction;
            }
        }

        /// <summary>
        /// Configure DataContractResolver.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source TChannel.</param>
        /// <param name="setDataContractResolverAction">A delegate to configure DataContractSerializerOperationBehavior.</param>
        public static void SetDataContractResolver<TChannel>(this TChannel source, Action<DataContractSerializerOperationBehavior> setDataContractResolverAction) where TChannel : class
        {
            WcfClientBase<TChannel> wcfClient = source as WcfClientBase<TChannel>;

            if (wcfClient != null)
            {
                wcfClient.SetDataContractResolverAction = setDataContractResolverAction;
            }
        }

        /// <summary>
        /// Gets the WCF client base.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source TChannel.</param>
        /// <returns>WcfClientBase{TChannel} instance.</returns>
        public static WcfClientBase<TChannel> GetWcfClientBase<TChannel>(this TChannel source) where TChannel : class
        {
            return source as WcfClientBase<TChannel>;
        }

        /// <summary>
        /// Gets the WCF client base interface.
        /// </summary>
        /// <param name="source">The source TChannel.</param>
        /// <returns>IWcfClientBase instance.</returns>
        public static IWcfClientBase GetIWcfClientBase(this object source)
        {
            return source as IWcfClientBase;
        }
    }
}
