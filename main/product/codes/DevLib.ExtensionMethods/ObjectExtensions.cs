//-----------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Object Extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Invokes the method by the current instance, using the specified parameters.
        /// </summary>
        /// <param name="source">The instance of the invoked method.</param>
        /// <param name="method">The name of the invoked method.</param>
        /// <param name="parameters">The parameters of the invoked method.</param>
        /// <returns>The return value of the invoked method.</returns>
        public static object InvokeMethod(this object source, string method, object[] parameters)
        {
            MethodInfo methodInfo = source.GetType().GetMethod(method);
            return methodInfo.Invoke(source, parameters);
        }

        /// <summary>
        /// If object is not null, invoke method.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">Object to check.</param>
        /// <param name="action">Delegate method.
        /// <example>E.g. <code>source => DoSomething(source);</code></example>
        /// </param>
        /// <returns>Source object.</returns>
        public static T IfNotNull<T>(this T source, Action<T> action)
        {
            if (source != null)
            {
                action(source);
            }

            return source;
        }

        /// <summary>
        /// If object is null, invoke method.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">Object to check.</param>
        /// <param name="action">Delegate method.
        /// <example>E.g. <code>source => DoSomething(source);</code></example>
        /// </param>
        /// <returns>Source object.</returns>
        public static T IfNull<T>(this T source, Action<T> action)
        {
            if (source == null)
            {
                action(source);
            }

            return source;
        }

        /// <summary>
        /// Invoke System.Console.WriteLine() or System.Console.Write().
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">Source object.</param>
        /// <param name="appendObj">Append object to display.</param>
        /// <param name="withNewLine">Whether followed by the current line terminator.</param>
        /// <returns>The input object.</returns>
        public static T ConsoleOutput<T>(this T source, object appendObj = null, bool withNewLine = true)
        {
            if (appendObj == null)
            {
                if (withNewLine)
                {
                    Console.WriteLine(source);
                }
                else
                {
                    Console.Write(source);
                }

                return source;
            }

            if ((appendObj is string) && (appendObj as string).Contains("{0}"))
            {
                if (withNewLine)
                {
                    Console.WriteLine(appendObj as string, source);
                }
                else
                {
                    Console.Write(appendObj as string, source);
                }

                return source;
            }

            if (withNewLine)
            {
                Console.WriteLine("{0}{1}", source, appendObj);
            }
            else
            {
                Console.Write("{0}{1}", source, appendObj);
            }

            return source;
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneDeep<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Converts an object to the specified target type or returns the default value if those two types are not convertible.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>The target type.</returns>
        public static T ConvertTo<T>(this object source, T defaultValue = default(T), bool throwOnError = false)
        {
            if (source == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("source");
                }

                return default(T);
            }

            try
            {
                var targetType = typeof(T);

                if (source.GetType() == targetType)
                {
                    return (T)source;
                }

                var converter = TypeDescriptor.GetConverter(source);
                if (converter != null && converter.CanConvertTo(targetType))
                {
                    return (T)converter.ConvertTo(source, targetType);
                }

                converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null && converter.CanConvertFrom(source.GetType()))
                {
                    return (T)converter.ConvertFrom(source);
                }

                throw new NotSupportedException();
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Copies the readable and writable public property values from the target object to the source.
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        public static void CopyPropertiesFrom(this object source, object target)
        {
            source.CopyPropertiesFrom(target, string.Empty);
        }

        /// <summary>
        /// Copies the readable and writable public property values from the target object to the source and
        /// optionally allows for the ignoring of any number of properties.
        /// </summary>
        /// <remarks>The source and target objects must be of the same type.</remarks>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ignoreProperties">An array of property names to ignore.</param>
        public static void CopyPropertiesFrom(this object source, object target, params string[] ignoreProperties)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            // Get and check the object types
            Type type = target.GetType();
            if (source.GetType() != type)
            {
                throw new ArgumentException("The target type must be the same as the source");
            }

            // Build a clean list of property names to ignore
            List<string> ignoreList = new List<string>();
            foreach (string item in ignoreProperties)
            {
                if (!string.IsNullOrEmpty(item) && !ignoreList.Contains(item))
                {
                    ignoreList.Add(item);
                }
            }

            // Copy the properties
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite && property.CanRead && !ignoreList.Contains(property.Name))
                {
                    object val = property.GetValue(target, null);
                    property.SetValue(source, val, null);
                }
            }
        }

        /// <summary>
        /// Retrieve object's all properties value.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>Instance of Dictionary{string, object}.</returns>
        public static Dictionary<string, object> RetrieveProperties(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (PropertyInfo property in source.GetType().GetProperties())
            {
                if (property.CanRead)
                {
                    result.Add(property.Name, property.GetValue(source, null));
                }
            }

            return result;
        }
    }
}
