//-----------------------------------------------------------------------
// <copyright file="WcfServiceUtilities.cs" company="YuGuan Corporation">
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
    using System.ServiceModel.Configuration;

    /// <summary>
    /// WcfService Utilities.
    /// </summary>
    public static class WcfServiceUtilities
    {
        /// <summary>
        /// Gets the type list of hosted services from assembly file by using Assembly.Load(byte[]).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <returns>The list of hosted services type.</returns>
        public static List<Type> LoadWcfTypes(string assemblyFile)
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
                Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
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
        /// Gets the type list of hosted services from assembly file by using Assembly.Load(byte[]).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="configFile">Wcf configuration file.</param>
        /// <returns>The list of hosted services type.</returns>
        public static List<Type> LoadWcfTypes(string assemblyFile, string configFile)
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
                Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
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
        /// Gets the System.Type object with the specified name from assembly file by using Assembly.Load(byte[]).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <returns>A System.Type object that represents the specified class.</returns>
        public static Type LoadType(string assemblyFile, string typeFullName)
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
                Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
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
