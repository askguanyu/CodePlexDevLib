//-----------------------------------------------------------------------
// <copyright file="WcfClientProxyTypeBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Class WcfClientProxyTypeBuilder.
    /// </summary>
    internal static class WcfClientProxyTypeBuilder
    {
        /// <summary>
        /// Field TypeDictionary.
        /// </summary>
        private static readonly Dictionary<string, Type> TypeDictionary = new Dictionary<string, Type>();

        /// <summary>
        /// Method BuildType.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <returns>Type object.</returns>
        public static Type BuildType<TChannel, TTypeBuilder>()
            where TChannel : class
            where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            string typeName = typeof(TChannel).FullName.Replace(".", string.Empty) + typeof(TTypeBuilder).Name;

            lock (((ICollection)TypeDictionary).SyncRoot)
            {
                if (TypeDictionary.ContainsKey(typeName))
                {
                    return TypeDictionary[typeName];
                }
                else
                {
                    TypeDictionary.Add(typeName, new TTypeBuilder().GenerateType(typeName));
                    return TypeDictionary[typeName];
                }
            }
        }
    }
}
