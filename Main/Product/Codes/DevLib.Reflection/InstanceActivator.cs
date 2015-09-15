//-----------------------------------------------------------------------
// <copyright file="InstanceActivator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Reflection
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Contains methods to create instances of specified types.
    /// </summary>
    public static class InstanceActivator
    {
        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstance(Type type, params object[] args)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            object result = null;

            try
            {
                result = Activator.CreateInstance(type, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(type);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.Load(byte[]).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstance(string assemblyFile, string typeFullName, bool ignoreCase, params object[] args)
        {
            Type type = TypeLoader.Load(assemblyFile, typeFullName, ignoreCase);

            if (type == typeof(string))
            {
                return string.Empty;
            }

            object result = null;

            try
            {
                result = Activator.CreateInstance(type, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(type);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.LoadFrom(string).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstanceFrom(string assemblyFile, string typeFullName, bool ignoreCase, params object[] args)
        {
            Type type = TypeLoader.LoadFrom(assemblyFile, typeFullName, ignoreCase);

            if (type == typeof(string))
            {
                return string.Empty;
            }

            object result = null;

            try
            {
                result = Activator.CreateInstance(type, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(type);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.Load(byte[]).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstance(string assemblyFile, string typeFullName, bool ignoreCase, Type[] typeArguments, params object[] args)
        {
            object result = null;

            Type genericType = TypeLoader.Load(assemblyFile, typeFullName, ignoreCase).MakeGenericType(typeArguments);

            try
            {
                result = Activator.CreateInstance(genericType, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(genericType);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.LoadFrom(string).
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstanceFrom(string assemblyFile, string typeFullName, bool ignoreCase, Type[] typeArguments, params object[] args)
        {
            object result = null;

            Type genericType = TypeLoader.LoadFrom(assemblyFile, typeFullName, ignoreCase).MakeGenericType(typeArguments);

            try
            {
                result = Activator.CreateInstance(genericType, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(genericType);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.Load(byte[]).
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static T CreateInstance<T>(string assemblyFile, string typeFullName, bool ignoreCase, params object[] args)
        {
            return (T)CreateInstance(assemblyFile, typeFullName, ignoreCase, args);
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor using Assembly.LoadFrom(string).
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static T CreateInstanceFrom<T>(string assemblyFile, string typeFullName, bool ignoreCase, params object[] args)
        {
            return (T)CreateInstanceFrom(assemblyFile, typeFullName, ignoreCase, args);
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor by using Assembly.Load(byte[]).
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static T CreateInstance<T>(string assemblyFile, string typeFullName, bool ignoreCase, Type[] typeArguments, params object[] args)
        {
            return (T)CreateInstance(assemblyFile, typeFullName, ignoreCase, typeArguments, args);
        }

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor using Assembly.LoadFrom(string).
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static T CreateInstanceFrom<T>(string assemblyFile, string typeFullName, bool ignoreCase, Type[] typeArguments, params object[] args)
        {
            return (T)CreateInstanceFrom(assemblyFile, typeFullName, ignoreCase, typeArguments, args);
        }
    }
}
