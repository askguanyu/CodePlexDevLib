//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostType.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    /// <summary>
    /// Wcf ServiceHost Type.
    /// </summary>
    public static class WcfServiceHostType
    {
        /// <summary>
        /// Get the type list of hosted services from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <returns>The type list of hosted services.</returns>
        public static List<Type> LoadFile(string assemblyFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The file does not exist.", assemblyFile);
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
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get the type list of hosted services from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The path of the configuration file.</param>
        /// <returns>The type list of hosted services.</returns>
        public static List<Type> LoadFile(string assemblyFile, string configFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The file does not exist.", assemblyFile);
            }

            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new ArgumentException("The file does not exist.", configFile);
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                throw;
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="configFile"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static List<ServiceHost> LoadServiceHost(string assemblyFile, string configFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The file does not exist.", assemblyFile);
            }

            if (string.IsNullOrEmpty(configFile))
            {
                throw new ArgumentNullException("configFile");
            }

            if (!File.Exists(configFile))
            {
                throw new ArgumentException("The file does not exist.", configFile);
            }

            List<ServiceHost> result = new List<ServiceHost>();

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
                            Uri baseAddress = null;

                            foreach (BaseAddressElement baseAddressElement in serviceElement.Host.BaseAddresses)
                            {
                                baseAddress = new Uri(baseAddressElement.BaseAddress);
                                break;
                            }

                            ServiceHost serviceHost = new ServiceHost(serviceType, baseAddress);

                            foreach (ServiceEndpointElement serviceEndpointElement in serviceElement.Endpoints)
                            {
                                if (!serviceEndpointElement.Binding.StartsWith("mex"))
                                {
                                    var contract = assembly.GetType(serviceEndpointElement.Contract);
                                    var address = new Uri(baseAddress, serviceEndpointElement.Address);
                                    var binding = GetBinding(serviceEndpointElement.Binding, false);
                                    serviceHost.AddServiceEndpoint(serviceEndpointElement.Contract, binding, address);
                                    result.Add(serviceHost);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadServiceHost", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadServiceHost", e.Source, e.Message, e.StackTrace));
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
                throw new ArgumentException("The file does not exist.", assemblyFile);
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetType(typeFullName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHostType.LoadFrom", e.Source, e.Message, e.StackTrace));
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsWcfServiceClass(Type type)
        {
            return type.IsClass && HasServiceContractAttribute(type) && !IsDerivedFrom(type, typeof(ClientBase<>));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isMexBinding"></param>
        /// <returns></returns>
        private static Binding GetBinding(string address, bool isMexBinding)
        {
            if (isMexBinding)
            {
                if (address.StartsWith("mexNamedPipeBinding"))
                    return MetadataExchangeBindings.CreateMexNamedPipeBinding();

                if (address.StartsWith("mexTcpBinding"))
                    return MetadataExchangeBindings.CreateMexTcpBinding();

                if (address.StartsWith("mexHttpBinding"))
                    return MetadataExchangeBindings.CreateMexHttpBinding();

                if (address.StartsWith("mexHttpsBinding"))
                    return MetadataExchangeBindings.CreateMexHttpsBinding();
            }
            else
            {
                if (address.StartsWith("netNamedPipeBinding"))
                    return new NetNamedPipeBinding();

                if (address.StartsWith("netTcpBinding"))
                    return new NetTcpBinding();

                if (address.StartsWith("customBinding"))
                    return new CustomBinding();

                if (address.StartsWith("basicHttpBinding"))
                    return new BasicHttpBinding();

                if (address.StartsWith("wsHttpBinding"))
                    return new WSHttpBinding();

                if (address.StartsWith("ws2007HttpBinding"))
                    return new WS2007HttpBinding();

                if (address.StartsWith("ws2007FederationHttpBinding"))
                    return new WS2007FederationHttpBinding();

                if (address.StartsWith("wsDualHttpBinding"))
                    return new WSDualHttpBinding();

                if (address.StartsWith("wsFederationHttpBinding"))
                    return new WSFederationHttpBinding();
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool HasServiceContractAttribute(Type type)
        {
            if (type.IsDefined(typeof(ServiceContractAttribute), false))
            {
                return true;
            }

            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type type2 = interfaces[i];
                if (type2.IsDefined(typeof(ServiceContractAttribute), false))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        private static bool IsDerivedFrom(Type type, Type baseType)
        {
            return !(type.BaseType == null) && (type.BaseType.GUID == baseType.GUID || IsDerivedFrom(type.BaseType, baseType));
        }
    }
}
