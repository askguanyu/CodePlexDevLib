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
                ExceptionHandler.Log(e);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get the type list of hosted services from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="configFile">Wcf configuration file.</param>
        /// <returns>Instance of List{Type}.</returns>
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
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
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
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Static Method IsWcfServiceClass.
        /// </summary>
        /// <param name="type">Source Type.</param>
        /// <returns>true if type is WcfService Class; otherwise, false.</returns>
        internal static bool IsWcfServiceClass(Type type)
        {
            return type.IsClass && HasServiceContractAttribute(type) && !IsDerivedFrom(type, typeof(ClientBase<>));
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
        /// Static Method HasServiceContractAttribute.
        /// </summary>
        /// <param name="type">Source Type.</param>
        /// <returns>true if has ServiceContractAttribute; otherwise, false.</returns>
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
