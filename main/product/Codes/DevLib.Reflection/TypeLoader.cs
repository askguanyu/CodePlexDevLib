//-----------------------------------------------------------------------
// <copyright file="TypeLoader.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Reflection
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Get the System.Type object utilities.
    /// </summary>
    public static class TypeLoader
    {
        /// <summary>
        /// Get the type list from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <returns>Array of System.Type object.</returns>
        public static Type[] LoadFile(string assemblyFile)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The specified assembly file does not exist.", assemblyFile);
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetTypes();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Gets the System.Type object with the specified name from assembly file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <returns>A System.Type object that represents the specified class.</returns>
        public static Type LoadFrom(string assemblyFile, string typeFullName, bool ignoreCase)
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
                throw new ArgumentException("The specified assembly file does not exist.", "assemblyFile");
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetType(typeFullName, true, ignoreCase);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }
    }
}
