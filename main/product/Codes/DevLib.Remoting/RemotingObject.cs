//-----------------------------------------------------------------------
// <copyright file="RemotingObject.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Remoting
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    /// <summary>
    /// Provides a mechanism for communicating that allows objects to interact with each other across application domains or processes.
    /// </summary>
    /// <remarks>
    /// To use an object as a remoting object, object type must be deriving from MarshalByRefObject,
    /// or making it serializable either by adding the [Serializable] tag or by implementing the ISerializable interface.
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public static class RemotingObject
    {
        /// <summary>
        /// Field IpcUrlStringFormat.
        /// </summary>
        private const string IpcUrlStringFormat = "ipc://{0}/{0}";

        /// <summary>
        /// Field WorldSidSecurityIdentifier.
        /// </summary>
        private static readonly SecurityIdentifier WorldSidSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Field KeyStringFormat.
        /// </summary>
        private static readonly string KeyStringFormat = "[Key1][{0}][Key2][{1}]";

        /// <summary>
        /// Initializes static members of the <see cref="RemotingObject" /> class.
        /// </summary>
        static RemotingObject()
        {
            try
            {
                LifetimeServices.LeaseTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Registers an object on the service end as a well-known type for remoting communication.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="label">A unique label that allows to register multiple instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when register object by label; otherwise, false.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public static void Register(Type objectType, string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(objectType, label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);
                    IpcChannel ipcChannel = channel as IpcChannel;

                    if (ipcChannel == null)
                    {
                        RemotingConfiguration.CustomErrorsEnabled(false);

                        IDictionary properties = new Hashtable();
                        properties["portName"] = objectUri;
                        properties["name"] = objectUri;
                        properties["authorizedGroup"] = WorldSidSecurityIdentifier.Translate(typeof(NTAccount)).Value;

                        BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                        serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                        BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

                        ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);
                        ChannelServices.RegisterChannel(ipcChannel, false);
                        RemotingConfiguration.RegisterWellKnownServiceType(objectType, objectUri, WellKnownObjectMode.Singleton);
                    }
                }
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
        /// Unregisters an object on the service end as a well-known type for remoting communication.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="label">A unique label that allows to register multiple instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when register object by label; otherwise, false.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public static void Unregister(Type objectType, string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(objectType, label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);

                    if (channel != null)
                    {
                        Type remotingConfigHandlerType = Type.GetType("System.Runtime.Remoting.RemotingConfigHandler, mscorlib");
                        FieldInfo remotingConfigHandlerFieldInfoInfo = remotingConfigHandlerType.GetField("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                        Type identityHolderType = Type.GetType("System.Runtime.Remoting.IdentityHolder, mscorlib");
                        MethodInfo identityHolderMethodInfoRemoveIdentity = identityHolderType.GetMethod("RemoveIdentity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

                        object obj = remotingConfigHandlerFieldInfoInfo.GetValue(null);
                        FieldInfo fieldInfo = obj.GetType().GetField("_wellKnownExportInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        obj = fieldInfo.GetValue(obj);
                        Hashtable registeredTypes = (Hashtable)obj;

                        try
                        {
                            identityHolderMethodInfoRemoveIdentity.Invoke(null, new object[] { objectUri });
                            registeredTypes.Remove(objectUri.ToLowerInvariant());
                        }
                        catch
                        {
                        }

                        IpcChannel ipcChannel = channel as IpcChannel;

                        if (ipcChannel != null)
                        {
                            ipcChannel.StopListening(null);
                        }

                        IpcServerChannel ipcServerChannel = channel as IpcServerChannel;

                        if (ipcServerChannel != null)
                        {
                            ipcServerChannel.StopListening(null);
                        }

                        ChannelServices.UnregisterChannel(channel);
                    }
                }
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
        /// <param name="objectType">Type of the object.</param>
        /// <param name="label">A unique label that gets the specific instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when get object by label; otherwise, false.</param>
        /// <returns>The remoting object instance.</returns>
        [SecurityPermission(SecurityAction.Demand)]
        public static object GetObject(Type objectType, string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(objectType, label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);
                    IpcChannel ipcChannel = channel as IpcChannel;

                    if (ipcChannel == null)
                    {
                        RemotingConfiguration.CustomErrorsEnabled(false);

                        IDictionary properties = new Hashtable();
                        properties["portName"] = Guid.NewGuid().ToString();
                        properties["name"] = objectUri;
                        properties["authorizedGroup"] = WorldSidSecurityIdentifier.Translate(typeof(NTAccount)).Value;

                        BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                        serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                        BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

                        ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);

                        ChannelServices.RegisterChannel(ipcChannel, false);
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return Activator.GetObject(objectType, string.Format(IpcUrlStringFormat, objectUri), objectUri);
        }

        /// <summary>
        /// Determines whether the specified remoting object instances are the same singleton instance.
        /// </summary>
        /// <param name="objA">The first remoting object to compare.</param>
        /// <param name="objB">The second remoting object to compare.</param>
        /// <returns>true if objA is the same as objB or if both are null references; otherwise, false.</returns>
        [SecurityPermission(SecurityAction.Demand)]
        public static bool RemotingEquals(MarshalByRefObject objA, MarshalByRefObject objB)
        {
            if (object.ReferenceEquals(objA, objB))
            {
                return true;
            }

            if (objA == null || objB == null)
            {
                return false;
            }

            if (RemotingServices.IsTransparentProxy(objA))
            {
                if (RemotingServices.IsTransparentProxy(objB))
                {
                    return string.Equals(RemotingServices.GetObjectUri(objA), RemotingServices.GetObjectUri(objB), StringComparison.Ordinal);
                }
                else
                {
                    string uriA = RemotingServices.GetObjectUri(objA);
                    string[] uriB = RemotingServices.GetObjectUri(objB).Split('/');

                    return uriA.Contains(uriB[uriB.Length - 1]);
                }
            }
            else
            {
                if (RemotingServices.IsTransparentProxy(objB))
                {
                    string[] uriA = RemotingServices.GetObjectUri(objA).Split('/');
                    string uriB = RemotingServices.GetObjectUri(objB);

                    return uriB.Contains(uriA[uriA.Length - 1]);
                }
                else
                {
                    string[] uriA = RemotingServices.GetObjectUri(objA).Split('/');
                    string[] uriB = RemotingServices.GetObjectUri(objB).Split('/');

                    return string.Equals(uriA[uriA.Length - 1], uriB[uriB.Length - 1], StringComparison.Ordinal);
                }
            }
        }

        /// <summary>
        /// Gets object uri.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="label">A unique label.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>Object uri string.</returns>
        private static string GetObjectUri(Type objectType, string label, bool ignoreCase)
        {
            return GetHashString(string.Format(KeyStringFormat, objectType.FullName, string.IsNullOrEmpty(label) ? string.Empty : label), ignoreCase);
        }

        /// <summary>
        /// Gets hash string.
        /// </summary>
        /// <param name="value">String to calculate.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>A hash string.</returns>
        private static string GetHashString(string value, bool ignoreCase)
        {
            byte[] hash;

            using (MD5 hasher = MD5.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(ignoreCase ? value.ToLowerInvariant() : value));
            }

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }

    /// <summary>
    /// Provides a mechanism for communicating that allows objects to interact with each other across application domains or processes.
    /// </summary>
    /// <typeparam name="T">Type of the remoting object.</typeparam>
    /// <remarks>
    /// To use an object as a remoting object, object type must be deriving from MarshalByRefObject,
    /// or making it serializable either by adding the [Serializable] tag or by implementing the ISerializable interface.
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public static class RemotingObject<T>
    {
        /// <summary>
        /// Field IpcUrlStringFormat.
        /// </summary>
        private const string IpcUrlStringFormat = "ipc://{0}/{0}";

        /// <summary>
        /// Field WorldSidSecurityIdentifier.
        /// </summary>
        private static readonly SecurityIdentifier WorldSidSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Field ObjectType.
        /// </summary>
        private static readonly Type ObjectType = typeof(T);

        /// <summary>
        /// Field KeyStringFormat.
        /// </summary>
        private static readonly string KeyStringFormat = "[Key1][" + ObjectType.FullName + "][Key2][{0}]";

        /// <summary>
        /// Field DefaultKey.
        /// </summary>
        private static readonly string DefaultKey = "[Key1][" + ObjectType.FullName + "][Key2][]";

        /// <summary>
        /// Field ObjectTypeHash.
        /// </summary>
        private static readonly string ObjectTypeHash = GetHashString(DefaultKey, false);

        /// <summary>
        /// Field ObjectTypeHashIgnoreCase.
        /// </summary>
        private static readonly string ObjectTypeHashIgnoreCase = GetHashString(DefaultKey, true);

        /// <summary>
        /// Initializes static members of the <see cref="RemotingObject{T}" /> class.
        /// </summary>
        static RemotingObject()
        {
            try
            {
                LifetimeServices.LeaseTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Registers an object on the service end as a well-known type for remoting communication.
        /// </summary>
        /// <param name="label">A unique label that allows to register multiple instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when register object by label; otherwise, false.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public static void Register(string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);
                    IpcChannel ipcChannel = channel as IpcChannel;

                    if (ipcChannel == null)
                    {
                        RemotingConfiguration.CustomErrorsEnabled(false);

                        IDictionary properties = new Hashtable();
                        properties["portName"] = objectUri;
                        properties["name"] = objectUri;
                        properties["authorizedGroup"] = WorldSidSecurityIdentifier.Translate(typeof(NTAccount)).Value;

                        BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                        serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                        BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

                        ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);
                        ChannelServices.RegisterChannel(ipcChannel, false);
                        RemotingConfiguration.RegisterWellKnownServiceType(ObjectType, objectUri, WellKnownObjectMode.Singleton);
                    }
                }
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
        /// Unregisters an object on the service end as a well-known type for remoting communication.
        /// </summary>
        /// <param name="label">A unique label that allows to register multiple instance of the same type.</param>
        /// <param name="ignoreCase">true to ignore case when register object by label; otherwise, false.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public static void Unregister(string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);

                    if (channel != null)
                    {
                        Type remotingConfigHandlerType = Type.GetType("System.Runtime.Remoting.RemotingConfigHandler, mscorlib");
                        FieldInfo remotingConfigHandlerFieldInfoInfo = remotingConfigHandlerType.GetField("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                        Type identityHolderType = Type.GetType("System.Runtime.Remoting.IdentityHolder, mscorlib");
                        MethodInfo identityHolderMethodInfoRemoveIdentity = identityHolderType.GetMethod("RemoveIdentity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

                        object obj = remotingConfigHandlerFieldInfoInfo.GetValue(null);
                        FieldInfo fieldInfo = obj.GetType().GetField("_wellKnownExportInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        obj = fieldInfo.GetValue(obj);
                        Hashtable registeredTypes = (Hashtable)obj;

                        try
                        {
                            identityHolderMethodInfoRemoveIdentity.Invoke(null, new object[] { objectUri });
                            registeredTypes.Remove(objectUri.ToLowerInvariant());
                        }
                        catch
                        {
                        }

                        IpcChannel ipcChannel = channel as IpcChannel;

                        if (ipcChannel != null)
                        {
                            ipcChannel.StopListening(null);
                        }

                        IpcServerChannel ipcServerChannel = channel as IpcServerChannel;

                        if (ipcServerChannel != null)
                        {
                            ipcServerChannel.StopListening(null);
                        }

                        ChannelServices.UnregisterChannel(channel);
                    }
                }
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
            string objectUri = GetObjectUri(label, ignoreCase);

            try
            {
                lock (SyncRoot)
                {
                    IChannel channel = ChannelServices.GetChannel(objectUri);
                    IpcChannel ipcChannel = channel as IpcChannel;

                    if (ipcChannel == null)
                    {
                        RemotingConfiguration.CustomErrorsEnabled(false);

                        IDictionary properties = new Hashtable();
                        properties["portName"] = Guid.NewGuid().ToString();
                        properties["name"] = objectUri;
                        properties["authorizedGroup"] = WorldSidSecurityIdentifier.Translate(typeof(NTAccount)).Value;

                        BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                        serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                        BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

                        ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);

                        ChannelServices.RegisterChannel(ipcChannel, false);
                    }
                }
            }
            catch
            {
            }

            return (T)Activator.GetObject(ObjectType, string.Format(IpcUrlStringFormat, objectUri));
        }

        /// <summary>
        /// Determines whether the specified remoting object instances are the same singleton instance.
        /// </summary>
        /// <param name="objA">The first remoting object to compare.</param>
        /// <param name="objB">The second remoting object to compare.</param>
        /// <returns>true if objA is the same as objB or if both are null references; otherwise, false.</returns>
        [SecurityPermission(SecurityAction.Demand)]
        public static bool RemotingEquals(T objA, T objB)
        {
            if (object.ReferenceEquals(objA, objB))
            {
                return true;
            }

            if (objA == null || objB == null)
            {
                return false;
            }

            if (RemotingServices.IsTransparentProxy(objA))
            {
                if (RemotingServices.IsTransparentProxy(objB))
                {
                    return string.Equals(RemotingServices.GetObjectUri(objA as MarshalByRefObject), RemotingServices.GetObjectUri(objB as MarshalByRefObject), StringComparison.Ordinal);
                }
                else
                {
                    string uriA = RemotingServices.GetObjectUri(objA as MarshalByRefObject);
                    string[] uriB = RemotingServices.GetObjectUri(objB as MarshalByRefObject).Split('/');

                    return uriA.Contains(uriB[uriB.Length - 1]);
                }
            }
            else
            {
                if (RemotingServices.IsTransparentProxy(objB))
                {
                    string[] uriA = RemotingServices.GetObjectUri(objA as MarshalByRefObject).Split('/');
                    string uriB = RemotingServices.GetObjectUri(objB as MarshalByRefObject);

                    return uriB.Contains(uriA[uriA.Length - 1]);
                }
                else
                {
                    string[] uriA = RemotingServices.GetObjectUri(objA as MarshalByRefObject).Split('/');
                    string[] uriB = RemotingServices.GetObjectUri(objB as MarshalByRefObject).Split('/');

                    return string.Equals(uriA[uriA.Length - 1], uriB[uriB.Length - 1], StringComparison.Ordinal);
                }
            }
        }

        /// <summary>
        /// Gets object uri.
        /// </summary>
        /// <param name="label">A unique label.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>Object uri string.</returns>
        private static string GetObjectUri(string label, bool ignoreCase)
        {
            return string.IsNullOrEmpty(label) ? (ignoreCase ? ObjectTypeHashIgnoreCase : ObjectTypeHash) : GetHashString(string.Format(KeyStringFormat, label), ignoreCase);
        }

        /// <summary>
        /// Gets hash string.
        /// </summary>
        /// <param name="value">String to calculate.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>A hash string.</returns>
        private static string GetHashString(string value, bool ignoreCase)
        {
            byte[] hash;

            using (MD5 hasher = MD5.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(ignoreCase ? value.ToLowerInvariant() : value));
            }

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
