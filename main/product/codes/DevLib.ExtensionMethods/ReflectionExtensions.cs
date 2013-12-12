//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Reflection Extensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type of object to create.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstance(this Type source, params object[] args)
        {
            return Activator.CreateInstance(source, args);
        }

        /// <summary>
        /// Creates an instance of the specified type using the generic constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type of object to create.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        public static object CreateInstanceGeneric(this Type source, Type[] typeArguments, params object[] args)
        {
            return Activator.CreateInstance(source.MakeGenericType(typeArguments), args);
        }

        /// <summary>
        /// Invokes the method by the current instance, using the specified parameters.
        /// </summary>
        /// <param name="source">The instance of the invoked method.</param>
        /// <param name="method">The name of the invoked method.</param>
        /// <param name="parameters">The parameters of the invoked method.</param>
        /// <returns>The return value of the invoked method.</returns>
        public static object InvokeMethod(this object source, string method, params object[] parameters)
        {
            MethodInfo methodInfo = null;

            Type[] parameterTypes = parameters.Select(p => p.GetType()).ToArray();

            try
            {
                methodInfo = source.GetType().GetMethod(method, parameterTypes);
            }
            catch (AmbiguousMatchException)
            {
                var methods = source.GetType().GetMethods().Where(p => p.Name.Equals(method, StringComparison.OrdinalIgnoreCase) && !p.IsGenericMethod);

                foreach (MethodInfo item in methods)
                {
                    var methodParameters = item.GetParameters();

                    if (methodParameters.Length != parameters.Length)
                    {
                        continue;
                    }

                    for (int i = 0; i < methodParameters.Length; i++)
                    {
                        if (methodParameters[i].ParameterType != parameterTypes[i])
                        {
                            break;
                        }
                    }

                    methodInfo = item;

                    break;
                }
            }

            return methodInfo.Invoke(source, parameters);
        }

        /// <summary>
        /// Invokes the generic method by the current instance, using the specified parameters.
        /// </summary>
        /// <param name="source">The instance of the invoked method.</param>
        /// <param name="method">The name of the invoked method.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="parameters">The parameters of the invoked method.</param>
        /// <returns>The return value of the invoked method.</returns>
        public static object InvokeMethodGeneric(this object source, string method, Type[] typeArguments, params object[] parameters)
        {
            MethodInfo methodInfo = null;

            Type[] parameterTypes;

            if (parameters == null)
            {
                parameterTypes = Type.EmptyTypes;
            }
            else
            {
                parameterTypes = parameters.Select(p => p.GetType()).ToArray();
            }

            try
            {
                methodInfo = source.GetType().GetMethod(method, parameterTypes).MakeGenericMethod(typeArguments);
            }
            catch (AmbiguousMatchException)
            {
                var methods = source.GetType().GetMethods().Where(p => p.Name.Equals(method, StringComparison.OrdinalIgnoreCase) && p.IsGenericMethod);

                foreach (MethodInfo item in methods)
                {
                    var typeArgs = item.GetGenericArguments();

                    if (typeArgs.Length != typeArguments.Length)
                    {
                        continue;
                    }

                    var methodParameters = item.GetParameters();

                    if (methodParameters.Length != parameterTypes.Length)
                    {
                        continue;
                    }

                    for (int i = 0; i < methodParameters.Length; i++)
                    {
                        if (methodParameters[i].ParameterType != parameterTypes[i])
                        {
                            break;
                        }
                    }

                    methodInfo = item;

                    break;
                }
            }

            return methodInfo.MakeGenericMethod(typeArguments).Invoke(source, parameters);
        }

        /// <summary>
        /// Returns the value of the property with optional index values for indexed properties.
        /// </summary>
        /// <param name="source">The object whose property value will be returned.</param>
        /// <param name="propertyName">The string containing the name of the public property to get.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The property value for the <paramref name="source" /> parameter.</returns>
        public static object GetPropertyValue(this object source, string propertyName, params object[] index)
        {
            return source.GetType().GetProperty(propertyName).GetValue(source, index);
        }

        /// <summary>
        /// Returns the value of the property with optional index values for indexed properties.
        /// </summary>
        /// <param name="source">The object whose property value will be returned.</param>
        /// <param name="propertyName">The string containing the name of the public property to get.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The property value for the <paramref name="source" /> parameter.</returns>
        public static object GetPropertyValueGeneric(this object source, string propertyName, Type[] typeArguments, params object[] index)
        {
            return source.GetType().MakeGenericType(typeArguments).GetProperty(propertyName).GetValue(source, index);
        }

        /// <summary>
        /// Sets the value of the property with optional index values for index properties.
        /// </summary>
        /// <param name="source">The object whose property value will be set.</param>
        /// <param name="propertyName">The string containing the name of the public property to set.</param>
        /// <param name="value">The new value for this property.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The source object.</returns>
        public static object SetPropertyValue(this object source, string propertyName, object value, params object[] index)
        {
            source.GetType().GetProperty(propertyName).SetValue(source, value, index);
            return source;
        }

        /// <summary>
        /// Sets the value of the property with optional index values for index properties.
        /// </summary>
        /// <param name="source">The object whose property value will be set.</param>
        /// <param name="propertyName">The string containing the name of the public property to set.</param>
        /// <param name="value">The new value for this property.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The source object.</returns>
        public static object SetPropertyValueGeneric(this object source, string propertyName, object value, Type[] typeArguments, params object[] index)
        {
            source.GetType().MakeGenericType(typeArguments).GetProperty(propertyName).SetValue(source, value, index);
            return source;
        }
    }
}
