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
    using System.ServiceModel.Configuration;

    /// <summary>
    /// Wcf ServiceHost Type
    /// </summary>
    public static class WcfServiceHostType
    {
        /// <summary>
        /// Get the type list of hosted services from assembly file
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly</param>
        /// <returns>The type list of hosted services</returns>
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
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get the type list of hosted services from assembly file
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly</param>
        /// <param name="typeFullName">The path of the configuration file</param>
        /// <returns>The type list of hosted services</returns>
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHostType.LoadFile", e.Source, e.Message, e.StackTrace));
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets the System.Type object with the specified name from assembly file
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly</param>
        /// <param name="typeFullName">The full name of the type</param>
        /// <returns>A System.Type object that represents the specified class</returns>
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
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHostType.LoadFrom", e.Source, e.Message, e.StackTrace));
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
