//-----------------------------------------------------------------------
// <copyright file="ReflectionUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Reflection Utilities
    /// </summary>
    public static class ReflectionUtilities
    {
        /// <summary>
        /// Gets the System.Type object with the specified name from assembly file
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly</param>
        /// <param name="typeName">The full name of the type</param>
        /// <returns>A System.Type object that represents the specified class</returns>
        public static Type GetType(string assemblyFile, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The file does not exist.", assemblyFile);
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetType(typeName);
            }
            catch
            {
                throw;
            }
        }
    }
}
