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
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Initializes static members of the <see cref="RemotingObject" /> class.
        /// </summary>
        static RemotingObject()
        {
            try
            {
                LifetimeServices.LeaseManagerPollTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.LeaseTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.RenewOnCallTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.SponsorshipTimeout = TimeSpan.Zero;
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
        public static void Register(Type objectType, string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(objectType, label, ignoreCase);

            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                RemotingConfiguration.CustomErrorsEnabled(false);

                IDictionary properties = new Hashtable();
                properties["portName"] = objectUri;
                properties["name"] = objectUri;
                properties["authorizedGroup"] = "Everyone";

                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                IpcServerChannel ipcChannel = new IpcServerChannel(properties, serverProvider);

                lock (SyncRoot)
                {
                    try
                    {
                        ChannelServices.RegisterChannel(ipcChannel, false);
                    }
                    catch (RemotingException)
                    {
                    }
                    catch
                    {
                        throw;
                    }

                    RemotingConfiguration.RegisterWellKnownServiceType(objectType, objectUri, WellKnownObjectMode.Singleton);
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
                IChannel channel = ChannelServices.GetChannel(objectUri);
                IpcServerChannel ipcChannel = channel as IpcServerChannel;

                if (ipcChannel != null)
                {
                    Type remotingConfigHandlerType = Type.GetType("System.Runtime.Remoting.RemotingConfigHandler, mscorlib");
                    FieldInfo remotingConfigHandlerFieldInfoInfo = remotingConfigHandlerType.GetField("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    Type identityHolderType = Type.GetType("System.Runtime.Remoting.IdentityHolder, mscorlib");
                    MethodInfo identityHolderMethodInfoRemoveIdentity = identityHolderType.GetMethod("RemoveIdentity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

                    object obj = remotingConfigHandlerFieldInfoInfo.GetValue(null);
                    FieldInfo fieldInfo = obj.GetType().GetField("_wellKnownExportInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    obj = fieldInfo.GetValue(obj);
                    Hashtable registeredTypes = (Hashtable)obj;

                    lock (SyncRoot)
                    {
                        try
                        {
                            identityHolderMethodInfoRemoveIdentity.Invoke(null, new object[] { objectUri });
                            registeredTypes.Remove(objectUri.ToLowerInvariant());
                        }
                        catch
                        {
                        }

                        ipcChannel.StopListening(null);
                        ChannelServices.UnregisterChannel(ipcChannel);
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

            return Activator.GetObject(objectType, string.Format(IpcUrlStringFormat, objectUri), objectUri);
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

        /// <summary>
        /// Gets object uri.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="label">A unique label.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>Object uri string.</returns>
        private static string GetObjectUri(Type objectType, string label, bool ignoreCase)
        {
            return string.IsNullOrEmpty(label) ? GetHashString(objectType.AssemblyQualifiedName, ignoreCase) : GetHashString(label, ignoreCase);
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
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Field ObjectType.
        /// </summary>
        private static Type ObjectType = typeof(T);

        /// <summary>
        /// Field ObjectTypeHash.
        /// </summary>
        private static string ObjectTypeHash = GetHashString(ObjectType.AssemblyQualifiedName, false);

        /// <summary>
        /// Field ObjectTypeHashIgnoreCase.
        /// </summary>
        private static string ObjectTypeHashIgnoreCase = GetHashString(ObjectType.AssemblyQualifiedName, true);

        /// <summary>
        /// Initializes static members of the <see cref="RemotingObject{T}" /> class.
        /// </summary>
        static RemotingObject()
        {
            try
            {
                LifetimeServices.LeaseManagerPollTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.LeaseTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.RenewOnCallTime = TimeSpan.Zero;
            }
            catch
            {
            }

            try
            {
                LifetimeServices.SponsorshipTimeout = TimeSpan.Zero;
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
        public static void Register(string label = null, bool ignoreCase = false)
        {
            string objectUri = GetObjectUri(label, ignoreCase);

            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                RemotingConfiguration.CustomErrorsEnabled(false);

                IDictionary properties = new Hashtable();
                properties["portName"] = objectUri;
                properties["name"] = objectUri;
                properties["authorizedGroup"] = "Everyone";

                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                IpcServerChannel ipcChannel = new IpcServerChannel(properties, serverProvider);

                lock (SyncRoot)
                {
                    try
                    {
                        ChannelServices.RegisterChannel(ipcChannel, false);
                    }
                    catch (RemotingException)
                    {
                    }
                    catch
                    {
                        throw;
                    }

                    RemotingConfiguration.RegisterWellKnownServiceType(ObjectType, objectUri, WellKnownObjectMode.Singleton);
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
                IChannel channel = ChannelServices.GetChannel(objectUri);
                IpcServerChannel ipcChannel = channel as IpcServerChannel;

                if (ipcChannel != null)
                {
                    Type remotingConfigHandlerType = Type.GetType("System.Runtime.Remoting.RemotingConfigHandler, mscorlib");
                    FieldInfo remotingConfigHandlerFieldInfoInfo = remotingConfigHandlerType.GetField("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    Type identityHolderType = Type.GetType("System.Runtime.Remoting.IdentityHolder, mscorlib");
                    MethodInfo identityHolderMethodInfoRemoveIdentity = identityHolderType.GetMethod("RemoveIdentity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

                    object obj = remotingConfigHandlerFieldInfoInfo.GetValue(null);
                    FieldInfo fieldInfo = obj.GetType().GetField("_wellKnownExportInfo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    obj = fieldInfo.GetValue(obj);
                    Hashtable registeredTypes = (Hashtable)obj;

                    lock (SyncRoot)
                    {
                        try
                        {
                            identityHolderMethodInfoRemoveIdentity.Invoke(null, new object[] { objectUri });
                            registeredTypes.Remove(objectUri.ToLowerInvariant());
                        }
                        catch
                        {
                        }

                        ipcChannel.StopListening(null);
                        ChannelServices.UnregisterChannel(ipcChannel);
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

            return (T)Activator.GetObject(ObjectType, string.Format(IpcUrlStringFormat, objectUri), objectUri);
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

        /// <summary>
        /// Gets object uri.
        /// </summary>
        /// <param name="label">A unique label.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>Object uri string.</returns>
        private static string GetObjectUri(string label, bool ignoreCase)
        {
            return string.IsNullOrEmpty(label) ? (ignoreCase ? ObjectTypeHashIgnoreCase : ObjectTypeHash) : GetHashString(label, ignoreCase);
        }
    }
}
