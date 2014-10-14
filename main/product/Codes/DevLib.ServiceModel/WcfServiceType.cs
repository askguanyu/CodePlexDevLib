//-----------------------------------------------------------------------
// <copyright file="WcfServiceType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;

    /// <summary>
    /// Class WcfServiceType.
    /// </summary>
    public static class WcfServiceType
    {
        /// <summary>
        /// Gets the type list of hosted services from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <returns>The list of hosted services type.</returns>
        public static List<Type> LoadFile(string assemblyFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            List<Type> result = new List<Type>();

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Type[] assemblyTypeList = assembly.GetTypes();

                foreach (Type type in assemblyTypeList)
                {
                    if (IsWcfServiceClass(type))
                    {
                        result.Add(type);
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets the type list of hosted services from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="configFile">Wcf configuration file.</param>
        /// <returns>The list of hosted services type.</returns>
        public static List<Type> LoadFile(string assemblyFile, string configFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", configFile);
            }

            List<Type> result = new List<Type>();

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = configFile }, ConfigurationUserLevel.None);
                ServiceModelSectionGroup serviceModelSectionGroup = configuration.GetSectionGroup("system.serviceModel") as ServiceModelSectionGroup;
                foreach (ServiceElement serviceElement in serviceModelSectionGroup.Services.Services)
                {
                    try
                    {
                        Type serviceType = assembly.GetType(serviceElement.Name);

                        if (serviceType != null && IsWcfServiceClass(serviceType))
                        {
                            result.Add(serviceType);
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets the System.Type object with the specified name from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <returns>A System.Type object that represents the specified class.</returns>
        public static Type LoadFrom(string assemblyFile, string typeFullName)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (string.IsNullOrEmpty(typeFullName))
            {
                throw new ArgumentNullException("typeFullName");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetType(typeFullName);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Gets the ServiceContract list of hosted services from service type.
        /// </summary>
        /// <param name="type">Service type.</param>
        /// <returns>The list of hosted services contract.</returns>
        public static List<Type> GetServiceContract(Type type)
        {
            List<Type> result = new List<Type>();

            if (type == null)
            {
                return result;
            }

            try
            {
                foreach (Type typeInterface in type.GetInterfaces())
                {
                    if (HasServiceContractAttribute(typeInterface))
                    {
                        result.Add(typeInterface);
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Check whether a Type is a Wcf service.
        /// </summary>
        /// <param name="type">Source Type to check.</param>
        /// <returns>true if type is WcfService Class; otherwise, false.</returns>
        public static bool IsWcfServiceClass(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type.IsClass && HasServiceContractAttribute(type) && !IsDerivedFrom(type, typeof(ClientBase<>));
        }

        /// <summary>
        /// Check whether a Type has ServiceContract Attribute.
        /// </summary>
        /// <param name="type">Source Type to check.</param>
        /// <returns>true if has ServiceContractAttribute; otherwise, false.</returns>
        public static bool HasServiceContractAttribute(Type type)
        {
            if (type.IsDefined(typeof(ServiceContractAttribute), false))
            {
                return true;
            }

            Type[] interfaces = type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                Type interfaceType = interfaces[i];

                if (interfaceType.IsDefined(typeof(ServiceContractAttribute), false))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the Binding instance according to a Binding type.
        /// </summary>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <returns>Instance of Binding.</returns>
        public static Binding GetBinding(Type bindingType)
        {
            return GetBinding(bindingType.Name);
        }

        /// <summary>
        /// Gets the Binding instance according to a Binding type name.
        /// </summary>
        /// <param name="bindingTypeName">The name of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <returns>Instance of Binding.</returns>
        public static Binding GetBinding(string bindingTypeName)
        {
            if (bindingTypeName.Equals("BasicHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new BasicHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("WSHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new WSHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("NetTcpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new NetTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ListenBacklog = int.MaxValue, MaxConnections = ushort.MaxValue, PortSharingEnabled = true, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("NetNamedPipeBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new NetNamedPipeBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, MaxConnections = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("WS2007HttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new WS2007HttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("WS2007FederationHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new WS2007FederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("WSDualHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new WSDualHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("WSFederationHttpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new WSFederationHttpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("NetMsmqBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new NetMsmqBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("NetPeerTcpBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new NetPeerTcpBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10), MaxBufferPoolSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, ReaderQuotas = XmlDictionaryReaderQuotas.Max };
            }

            if (bindingTypeName.Equals("CustomBinding", StringComparison.OrdinalIgnoreCase))
            {
                return new CustomBinding() { OpenTimeout = TimeSpan.FromMinutes(10), CloseTimeout = TimeSpan.FromMinutes(10), SendTimeout = TimeSpan.FromMinutes(10), ReceiveTimeout = TimeSpan.FromMinutes(10) };
            }

            if (bindingTypeName.Equals("MetadataExchangeBindings", StringComparison.OrdinalIgnoreCase))
            {
                return MetadataExchangeBindings.CreateMexNamedPipeBinding();
            }

            if (bindingTypeName.Equals("MetadataExchangeBindings", StringComparison.OrdinalIgnoreCase))
            {
                return MetadataExchangeBindings.CreateMexTcpBinding();
            }

            if (bindingTypeName.Equals("MetadataExchangeBindings", StringComparison.OrdinalIgnoreCase))
            {
                return MetadataExchangeBindings.CreateMexHttpBinding();
            }

            if (bindingTypeName.Equals("MetadataExchangeBindings", StringComparison.OrdinalIgnoreCase))
            {
                return MetadataExchangeBindings.CreateMexHttpsBinding();
            }

            return null;
        }

        /// <summary>
        /// Static Method GetBinding.
        /// </summary>
        /// <param name="address">Binding address.</param>
        /// <param name="isMexBinding">Whether the binding is mex.</param>
        /// <returns>Instance of Binding.</returns>
        internal static Binding GetBinding(string address, bool isMexBinding)
        {
            if (isMexBinding)
            {
                if (address.StartsWith("mexNamedPipeBinding"))
                {
                    return MetadataExchangeBindings.CreateMexNamedPipeBinding();
                }

                if (address.StartsWith("mexTcpBinding"))
                {
                    return MetadataExchangeBindings.CreateMexTcpBinding();
                }

                if (address.StartsWith("mexHttpBinding"))
                {
                    return MetadataExchangeBindings.CreateMexHttpBinding();
                }

                if (address.StartsWith("mexHttpsBinding"))
                {
                    return MetadataExchangeBindings.CreateMexHttpsBinding();
                }
            }
            else
            {
                if (address.StartsWith("netNamedPipeBinding"))
                {
                    return new NetNamedPipeBinding();
                }

                if (address.StartsWith("netTcpBinding"))
                {
                    return new NetTcpBinding();
                }

                if (address.StartsWith("customBinding"))
                {
                    return new CustomBinding();
                }

                if (address.StartsWith("basicHttpBinding"))
                {
                    return new BasicHttpBinding();
                }

                if (address.StartsWith("wsHttpBinding"))
                {
                    return new WSHttpBinding();
                }

                if (address.StartsWith("ws2007HttpBinding"))
                {
                    return new WS2007HttpBinding();
                }

                if (address.StartsWith("ws2007FederationHttpBinding"))
                {
                    return new WS2007FederationHttpBinding();
                }

                if (address.StartsWith("wsDualHttpBinding"))
                {
                    return new WSDualHttpBinding();
                }

                if (address.StartsWith("wsFederationHttpBinding"))
                {
                    return new WSFederationHttpBinding();
                }
            }

            return null;
        }

        /// <summary>
        /// Static Method IsDerivedFrom.
        /// </summary>
        /// <param name="type">Target Type.</param>
        /// <param name="baseType">Base Type.</param>
        /// <returns>true if derived from baseType; otherwise, false.</returns>
        private static bool IsDerivedFrom(Type type, Type baseType)
        {
            return !(type.BaseType == null) && (type.BaseType.GUID == baseType.GUID || IsDerivedFrom(type.BaseType, baseType));
        }
    }
}
