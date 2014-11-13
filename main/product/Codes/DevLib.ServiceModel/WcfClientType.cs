//-----------------------------------------------------------------------
// <copyright file="WcfClientType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Class WcfClientType.
    /// </summary>
    public static class WcfClientType
    {
        /// <summary>
        /// Field TypeDictionary.
        /// </summary>
        private static readonly Dictionary<string, Type> TypeDictionary = new Dictionary<string, Type>();

        /// <summary>
        /// Field Lock.
        /// </summary>
        private static ReaderWriterLock Lock = new ReaderWriterLock();

        /// <summary>
        /// Gets or sets a value indicating whether save generated assembly file or not.
        /// </summary>
        public static bool SaveGeneratedAssemblyFile
        {
            get;
            set;
        }

        /// <summary>
        /// Method BuildType.
        /// </summary>
        /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
        /// <typeparam name="TTypeBuilder">The proxy class builder.</typeparam>
        /// <returns>Type object.</returns>
        internal static Type BuildType<TChannel, TTypeBuilder>()
            where TChannel : class
            where TTypeBuilder : IWcfClientTypeBuilder, new()
        {
            string typeName = typeof(TChannel).FullName.Replace(".", string.Empty) + typeof(TTypeBuilder).Name;

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (TypeDictionary.ContainsKey(typeName))
                {
                    return TypeDictionary[typeName];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (TypeDictionary.ContainsKey(typeName))
                        {
                            return TypeDictionary[typeName];
                        }
                        else
                        {
                            Type result = new TTypeBuilder().GenerateType(typeName);

                            TypeDictionary.Add(typeName, result);

                            return TypeDictionary[typeName];
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
    }
}
