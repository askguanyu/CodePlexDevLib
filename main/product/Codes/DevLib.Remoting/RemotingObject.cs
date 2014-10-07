//-----------------------------------------------------------------------
// <copyright file="RemotingObject.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Remoting
{
    using System;
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Security.Permissions;

    /// <summary>
    /// Provides a mechanism for communicating that allows objects to interact with each other across application domains or processes.
    /// </summary>
    /// <typeparam name="T">Type of the remoting object.</typeparam>
    public static class RemotingObject<T>
    {
        /// <summary>
        /// Field IpcUrlStringFormat.
        /// </summary>
        private const string IpcUrlStringFormat = "ipc://{0}/{0}";

        /// <summary>
        /// Registers an object on the service end as a well-known type for remoting communication.
        /// </summary>
        /// <param name="label">A unique label that allows to register multiple instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when host object by label; otherwise, false.</param>
        public static void Register(string label = null, bool ignoreCase = false)
        {
            Type objectType = typeof(T);

            int objectUriHashCode;

            if (ignoreCase)
            {
                objectUriHashCode = string.IsNullOrEmpty(label) ? objectType.AssemblyQualifiedName.ToLowerInvariant().GetHashCode() : label.ToLowerInvariant().GetHashCode();
            }
            else
            {
                objectUriHashCode = string.IsNullOrEmpty(label) ? objectType.AssemblyQualifiedName.GetHashCode() : label.GetHashCode();
            }

            string objectUri = objectUriHashCode.ToString(CultureInfo.InvariantCulture);

            try
            {
                IpcServerChannel ipcChannel = new IpcServerChannel(objectUri, objectUri);
                ChannelServices.RegisterChannel(ipcChannel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(objectType, objectUri, WellKnownObjectMode.Singleton);
            }
            catch (RemotingException)
            {
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Creates a proxy for the well-known object indicated by the specified type from remoting service.
        /// </summary>
        /// <param name="label">A unique label that gets the specific instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when get object by label; otherwise, false.</param>
        /// <returns>The remoting object instance.</returns>
        [SecurityPermission(SecurityAction.Demand)]
        public static T GetObject(string label = null, bool ignoreCase = false)
        {
            Type objectType = typeof(T);

            int objectUriHashCode;

            if (ignoreCase)
            {
                objectUriHashCode = string.IsNullOrEmpty(label) ? objectType.AssemblyQualifiedName.ToLowerInvariant().GetHashCode() : label.ToLowerInvariant().GetHashCode();
            }
            else
            {
                objectUriHashCode = string.IsNullOrEmpty(label) ? objectType.AssemblyQualifiedName.GetHashCode() : label.GetHashCode();
            }

            string objectUri = objectUriHashCode.ToString(CultureInfo.InvariantCulture);

            return (T)Activator.GetObject(objectType, string.Format(IpcUrlStringFormat, objectUriHashCode), objectUriHashCode);
        }
    }
}
