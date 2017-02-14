//-----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Reflection Extensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// The BindingFlags for public only lookup.
        /// </summary>
        public const BindingFlags PublicOnlyLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        /// <summary>
        /// The BindingFlags for non public only lookup.
        /// </summary>
        public const BindingFlags NonPublicOnlyLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        /// <summary>
        /// The BindingFlags for public and non public lookup.
        /// </summary>
        public const BindingFlags AllLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Check whether the source type inherits from baseType.
        /// </summary>
        /// <param name="source">The source type.</param>
        /// <param name="baseType">The base type.</param>
        /// <returns>true if the source type inherits from baseType; otherwise, false.</returns>
        public static bool IsInheritFrom(this Type source, Type baseType)
        {
            if (source.Equals(baseType)
                || ((source.AssemblyQualifiedName != null || baseType.AssemblyQualifiedName != null) && source.AssemblyQualifiedName == baseType.AssemblyQualifiedName))
            {
                return true;
            }

            if (!baseType.IsGenericType || !source.IsGenericType)
            {
                if (baseType.IsAssignableFrom(source))
                {
                    return true;
                }
            }
            else if (source.GetGenericTypeDefinition().Equals(baseType.GetGenericTypeDefinition()))
            {
                return true;
            }

            foreach (Type item in source.GetInterfaces())
            {
                if (item.IsInheritFrom(baseType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check whether the Type is nullable type.
        /// </summary>
        /// <param name="source">The type to check.</param>
        /// <returns>true if the type is Nullable{} type; otherwise, false.</returns>
        public static bool IsNullable(this Type source)
        {
            return source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Returns whether can convert the object to a <see cref="T:System.String" /> and vice versa.
        /// </summary>
        /// <param name="source">A <see cref="T:System.Type" /> that represents the type you want to convert.</param>
        /// <returns>true if can perform the conversion; otherwise, false.</returns>
        public static bool CanConvert(this Type source)
        {
            if (Type.GetTypeCode(source) == TypeCode.Object
                && !source.IsEnum
                && source != typeof(Guid)
                && source != typeof(TimeSpan)
                && source != typeof(DateTimeOffset)
                && !source.IsNullableCanConvert())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates an instance of IList by the specified type which inherit IEnumerable interface.
        /// </summary>
        /// <param name="source">Source Type which inherit IEnumerable interface.</param>
        /// <param name="lengths">An array of 32-bit integers that represent the size of each dimension of the list to create.</param>
        /// <returns>A reference to the newly created IList object.</returns>
        public static IList CreateIList(this Type source, params object[] lengths)
        {
            if (source.IsEnumerable() && !source.IsDictionary())
            {
                if (source.IsArray)
                {
                    return lengths.IsNullOrEmpty()
                        ? Array.CreateInstance(source.GetElementType(), 0)
                        : Array.CreateInstance(source.GetElementType(), lengths.Select(i => (long)i).ToArray());
                }
                else
                {
                    return lengths.IsNullOrEmpty()
                        ? (IList)Activator.CreateInstance(source)
                        : (IList)Activator.CreateInstance(source, lengths[0]);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an instance of List{} by the specified element type.
        /// </summary>
        /// <param name="source">Element Type of the list.</param>
        /// <returns>A reference to the newly created List object.</returns>
        public static IList CreateListByElementType(this Type source)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(source));
        }

        /// <summary>
        /// Creates an instance of List{} by the specified element type.
        /// </summary>
        /// <param name="source">Element Type of the list.</param>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <returns>A reference to the newly created List object.</returns>
        public static IList CreateListByElementType(this Type source, object collection)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(source), collection);
        }

        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type of object to create.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstance(this Type source, params object[] args)
        {
            if (source == typeof(string))
            {
                return string.Empty;
            }

            object result = null;

            try
            {
                result = Activator.CreateInstance(source, args);
            }
            catch
            {
                result = FormatterServices.GetUninitializedObject(source);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using the generic constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type of object to create.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstanceGeneric(this Type source, Type[] typeArguments, params object[] args)
        {
            object result = null;

            Type genericType = source.MakeGenericType(typeArguments);

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
        /// Invokes the method by the current instance, using the specified parameters.
        /// </summary>
        /// <param name="source">The instance of the invoked method.</param>
        /// <param name="method">The name of the invoked method.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="parameters">The parameters of the invoked method.</param>
        /// <returns>The return value of the invoked method.</returns>
        public static object InvokeMethod(this object source, string method, bool isPublicOnly = true, params object[] parameters)
        {
            MethodInfo methodInfo = null;

            Type[] parameterTypes = parameters.Select(p => p.GetType()).ToArray();

            try
            {
                methodInfo = source.GetType().GetMethod(method, isPublicOnly ? PublicOnlyLookup : AllLookup, null, CallingConventions.Any, parameterTypes, null);
            }
            catch (AmbiguousMatchException)
            {
                var methods = source.GetType().GetMethods(isPublicOnly ? PublicOnlyLookup : AllLookup).Where(p => p.Name.Equals(method, StringComparison.OrdinalIgnoreCase) && !p.IsGenericMethod);

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
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="parameters">The parameters of the invoked method.</param>
        /// <returns>The return value of the invoked method.</returns>
        public static object InvokeMethodGeneric(this object source, string method, Type[] typeArguments, bool isPublicOnly = true, params object[] parameters)
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
                methodInfo = source.GetType().GetMethod(method, isPublicOnly ? PublicOnlyLookup : AllLookup, null, CallingConventions.Any, parameterTypes, null).MakeGenericMethod(typeArguments);
            }
            catch (AmbiguousMatchException)
            {
                var methods = source.GetType().GetMethods(isPublicOnly ? PublicOnlyLookup : AllLookup).Where(p => p.Name.Equals(method, StringComparison.OrdinalIgnoreCase) && p.IsGenericMethod);

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
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The property value for the <paramref name="source" /> parameter.</returns>
        public static object GetPropertyValue(this object source, string propertyName, bool isPublicOnly = true, params object[] index)
        {
            return source.GetType().GetProperty(propertyName, isPublicOnly ? PublicOnlyLookup : AllLookup).GetValue(source, index);
        }

        /// <summary>
        /// Returns the value of the property with optional index values for indexed properties.
        /// </summary>
        /// <param name="source">The object whose property value will be returned.</param>
        /// <param name="propertyName">The string containing the name of the public property to get.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The property value for the <paramref name="source" /> parameter.</returns>
        public static object GetPropertyValueGeneric(this object source, string propertyName, Type[] typeArguments, bool isPublicOnly = true, params object[] index)
        {
            return source.GetType().MakeGenericType(typeArguments).GetProperty(propertyName, isPublicOnly ? PublicOnlyLookup : AllLookup).GetValue(source, index);
        }

        /// <summary>
        /// Sets the value of the property with optional index values for index properties.
        /// </summary>
        /// <param name="source">The object whose property value will be set.</param>
        /// <param name="propertyName">The string containing the name of the public property to set.</param>
        /// <param name="value">The new value for this property.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The source object.</returns>
        public static object SetPropertyValue(this object source, string propertyName, object value, bool isPublicOnly = true, params object[] index)
        {
            source.GetType().GetProperty(propertyName, isPublicOnly ? PublicOnlyLookup : AllLookup).SetValue(source, value, index);

            return source;
        }

        /// <summary>
        /// Sets the value of the property with optional index values for index properties.
        /// </summary>
        /// <param name="source">The object whose property value will be set.</param>
        /// <param name="propertyName">The string containing the name of the public property to set.</param>
        /// <param name="value">The new value for this property.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The source object.</returns>
        public static object SetPropertyValueGeneric(this object source, string propertyName, object value, Type[] typeArguments, bool isPublicOnly = true, params object[] index)
        {
            source.GetType().MakeGenericType(typeArguments).GetProperty(propertyName, isPublicOnly ? PublicOnlyLookup : AllLookup).SetValue(source, value, index);

            return source;
        }

        /// <summary>
        /// Retrieves object's all properties value.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="declaredOnly">Specifies that only members declared at the level of the supplied type's hierarchy should be considered. Inherited members are not considered.</param>
        /// <returns>Instance of Dictionary{string, object}.</returns>
        public static Dictionary<string, object> RetrieveProperties(this object source, bool isPublicOnly = true, bool declaredOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Dictionary<string, object> result = new Dictionary<string, object>();

            BindingFlags lookup = isPublicOnly ? PublicOnlyLookup : AllLookup;

            if (declaredOnly)
            {
                lookup |= BindingFlags.DeclaredOnly;
            }

            foreach (PropertyInfo property in source.GetType().GetProperties(lookup))
            {
                if (property.CanRead)
                {
                    result.Add(property.Name, property.GetValue(source, null));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets object's all properties with get value function.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <param name="declaredOnly">Specifies that only members declared at the level of the supplied type's hierarchy should be considered. Inherited members are not considered.</param>
        /// <returns>Instance of Dictionary{string, Func{object}}.</returns>
        public static Dictionary<string, Func<object>> RetrievePropertiesLazy(this object source, bool isPublicOnly = true, bool declaredOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Dictionary<string, Func<object>> result = new Dictionary<string, Func<object>>();

            BindingFlags lookup = isPublicOnly ? PublicOnlyLookup : AllLookup;

            if (declaredOnly)
            {
                lookup |= BindingFlags.DeclaredOnly;
            }

            foreach (PropertyInfo property in source.GetType().GetProperties(lookup))
            {
                result.Add(property.Name, () => property.GetValue(source, null));
            }

            return result;
        }

        /// <summary>
        /// Gets the public get set properties.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="declaredOnly">Specifies that only members declared at the level of the supplied type's hierarchy should be considered. Inherited members are not considered.</param>
        /// <returns>Array of PropertyInfo.</returns>
        public static PropertyInfo[] GetPublicGetSetProperties(this Type source, bool declaredOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source
                .GetProperties(declaredOnly ? (PublicOnlyLookup | BindingFlags.DeclaredOnly) : PublicOnlyLookup)
                .Where(i => i.CanRead && i.CanWrite && i.GetGetMethod(true).IsPublic && i.GetSetMethod(true).IsPublic)
                .ToArray();
        }

        /// <summary>
        /// Check whether the Type is nullable type and can convert.
        /// </summary>
        /// <param name="source">The type to check.</param>
        /// <returns>true if the type is Nullable{} type; otherwise, false.</returns>
        public static bool IsNullableCanConvert(this Type source)
        {
            return source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(source).CanConvert();
        }

        /// <summary>
        /// Gets the System.Type with the specified name, specifying whether to perform a case-sensitive search and whether to throw an exception if the type is not found.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="isFullName">true if type name is full name; otherwise, false.</param>
        /// <param name="throwOnError">true to throw an exception if the type cannot be found; false to return null.</param>
        /// <param name="ignoreCase">true to perform a case-insensitive search for typeName, false to perform a case-sensitive search for typeName.</param>
        /// <returns>The type with the specified name. If the type is not found, the throwOnError parameter specifies whether null is returned or an exception is thrown.</returns>
        public static Type GetType(this string source, bool isFullName, bool throwOnError, bool ignoreCase)
        {
            Type result = null;

            Exception exception = null;

            try
            {
                result = Type.GetType(source, true, ignoreCase);
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (result == null)
            {
                var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                try
                {
                    if (isFullName)
                    {
                        result = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .SelectMany(i => i.GetTypes())
                            .FirstOrDefault(i => i.FullName.Equals(source, stringComparison));
                    }
                    else
                    {
                        result = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .SelectMany(i => i.GetTypes())
                            .FirstOrDefault(i => i.Name.Equals(source, stringComparison));
                    }
                }
                catch (Exception e)
                {
                    exception.Data.Add("InnerException", e);
                }
            }

            if (result == null && throwOnError)
            {
                throw exception ?? new TypeLoadException();
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified type using the constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type full name of object to create.</param>
        /// <param name="isFullName">true if type name is full name; otherwise, false.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstance(this string source, bool isFullName, params object[] args)
        {
            return source.GetType(isFullName, true, true).CreateInstance(args);
        }

        /// <summary>
        /// Creates an instance of the specified type using the generic constructor that best matches the specified parameters.
        /// </summary>
        /// <param name="source">The type full name of object to create.</param>
        /// <param name="isFullName">true if type name is full name; otherwise, false.</param>
        /// <param name="typeArguments">An array of types to be substituted for the type parameters of the current generic method definition.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>A reference to the newly created object.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static object CreateInstanceGeneric(this string source, bool isFullName, Type[] typeArguments, params object[] args)
        {
            return source.GetType(isFullName, true, true).CreateInstance(typeArguments, args);
        }
    }
}
