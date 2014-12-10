//-----------------------------------------------------------------------
// <copyright file="GenericDataContractResolverExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel.Extensions
{
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// GenericDataContractResolver extension methods.
    /// </summary>
    public static class GenericDataContractResolverExtensions
    {
        /// <summary>
        /// Adds Generic DataContractResolver.
        /// </summary>
        /// <param name="source">The source ServiceHost.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver(this ServiceHost source, params Type[] knownTypes)
        {
            foreach (ServiceEndpoint endpoint in source.Description.Endpoints)
            {
                endpoint.AddGenericDataContractResolver(knownTypes);
            }
        }

        /// <summary>
        /// Adds Generic DataContractResolver.
        /// </summary>
        /// <typeparam name="T">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source ServiceHost.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver<T>(this ClientBase<T> source, params Type[] knownTypes) where T : class
        {
            source.Endpoint.AddGenericDataContractResolver(knownTypes);
        }

        /// <summary>
        /// Adds Generic DataContractResolver.
        /// </summary>
        /// <typeparam name="T">The channel to be used to connect to the service.</typeparam>
        /// <param name="source">The source ServiceHost.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver<T>(this ChannelFactory<T> source, params Type[] knownTypes) where T : class
        {
            source.Endpoint.AddGenericDataContractResolver(knownTypes);
        }

        /// <summary>
        /// Adds GenericDataContractResolver.
        /// </summary>
        /// <param name="source">The source ServiceEndpoint.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver(this ServiceEndpoint source, params Type[] knownTypes)
        {
            foreach (OperationDescription operationDescription in source.Contract.Operations)
            {
                operationDescription.AddGenericDataContractResolver(knownTypes);
            }
        }

        /// <summary>
        /// Adds GenericDataContractResolver.
        /// </summary>
        /// <param name="source">The source ServiceEndpoint.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver(this OperationDescription source, params Type[] knownTypes)
        {
            DataContractSerializerOperationBehavior serializerBehavior = source.Behaviors.Find<DataContractSerializerOperationBehavior>();

            if (serializerBehavior == null)
            {
                serializerBehavior = new DataContractSerializerOperationBehavior(source);
                source.Behaviors.Add(serializerBehavior);
            }

            serializerBehavior.AddGenericDataContractResolver(knownTypes);
        }

        /// <summary>
        /// Adds GenericDataContractResolver.
        /// </summary>
        /// <param name="source">The source ServiceEndpoint.</param>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static void AddGenericDataContractResolver(this DataContractSerializerOperationBehavior source, params Type[] knownTypes)
        {
            GenericDataContractResolver resolver = source.DataContractResolver as GenericDataContractResolver;

            if (resolver != null)
            {
                resolver.AddKnownTypeRange(knownTypes);
                resolver.AddKnownTypeRange(GenericDataContractResolver.GetReflectTypes(Assembly.GetCallingAssembly()));
            }
            else
            {
                resolver = new GenericDataContractResolver(knownTypes);
            }

            source.DataContractResolver = resolver;
        }
    }
}
