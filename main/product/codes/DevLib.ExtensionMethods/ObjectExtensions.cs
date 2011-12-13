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
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Object Extensions
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Invoke System.Console.WriteLine() or System.Console.Write()
        /// </summary>
        /// <typeparam name="T">The type of input object</typeparam>
        /// <param name="source">The input object</param>
        /// <param name="obj">Append object to display</param>
        /// <param name="withNewLine">Whether followed by the current line terminator</param>
        /// <returns>The input object</returns>
        public static T ConsoleOutput<T>(this T source, object obj, bool withNewLine = true)
        {
            if (withNewLine)
            {
                Console.WriteLine("{0}{1}", source, obj);
            }
            else
            {
                Console.Write("{0}{1}", source, obj);
            }

            return source;
        }

        /// <summary>
        /// Invoke System.Console.WriteLine() or System.Console.Write()
        /// </summary>
        /// <typeparam name="T">The type of input object</typeparam>
        /// <param name="source">The input object</param>
        /// <param name="format">A composite format string</param>
        /// <param name="withNewLine">Whether followed by the current line terminator</param>
        /// <returns>The input object</returns>
        public static T ConsoleOutput<T>(this T source, string format = "", bool withNewLine = true)
        {
            if (format.Contains("{0}"))
            {
                if (withNewLine)
                {
                    Console.WriteLine(format, source);
                }
                else
                {
                    Console.Write(format, source);
                }
            }
            else
            {
                if (withNewLine)
                {
                    Console.WriteLine("{0}{1}", source, format);
                }
                else
                {
                    Console.Write("{0}{1}", source, format);
                }
            }

            return source;
        }

        /// <summary>
        /// Perform a deep Copy of the object
        /// </summary>
        /// <typeparam name="T">The type of object being copied</typeparam>
        /// <param name="source">The object instance to copy</param>
        /// <returns>The copied object</returns>
        public static T CloneDeep<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Convert object to bytes
        /// </summary>
        /// <param name="source">Object</param>
        /// <returns>Byte array</returns>
        public static byte[] ToByteArray<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return null;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, source);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return string.Empty;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(source.GetType());
                serializer.WriteObject(memoryStream, source);
                return Encoding.Default.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="knownTypes">An IEnumerable of known types.  Useful for complex objects</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this object source, IEnumerable<Type> knownTypes)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return string.Empty;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(source.GetType(), knownTypes);
                serializer.WriteObject(memoryStream, source);
                return Encoding.Default.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Serializes a JSON object to an object
        /// </summary>
        /// <param name="source">JSON string object</param>
        /// <returns>The result object</returns>
        public static T FromJson<T>(this object source)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(source.ToString())))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Serializes a JSON object to an object
        /// </summary>
        /// <param name="source">JSON string object</param>
        /// <param name="knownTypes">An IEnumerable of known types.  Useful for complex objects.</param>
        /// <returns>The result object</returns>
        public static T FromJson<T>(this object source, IEnumerable<Type> knownTypes)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(source.ToString())))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
                return (T)serializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Serializes the object into an XML string
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the
        /// <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize</param>
        /// <param name="encoding">The Encoding scheme to use when serializing the data to XML</param>
        /// <returns>An XML encoded string representation of the source object</returns>
        public static string ToXml(this object source, Encoding encoding)
        {
            if (source == null)
            {
                throw new ArgumentException("The source object cannot be null.");
            }

            if (encoding == null)
            {
                throw new Exception("You must specify an encoder to use for serialization.");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());
                xmlSerializer.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return encoding.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Converts an object to the specified target type or returns the default value if
        /// those 2 types are not convertible.
        /// <para>Any exceptions are optionally ignored (<paramref name="ignoreException"/>).</para>
        /// <para>
        /// If the exceptions are not ignored and the <paramref name="source"/> can't be convert even if
        /// the types are convertible with each other, an exception is thrown.</para>
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "source">The value.</param>
        /// <param name = "defaultValue">The default value.</param>
        /// <param name = "ignoreException">if set to <c>true</c> ignore any exception.</param>
        /// <returns>The target type</returns>
        public static T ConvertTo<T>(this object source, T defaultValue = default(T), bool ignoreException = true)
        {
            if (ignoreException)
            {
                try
                {
                    return source.ConvertTo<T>(defaultValue);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return source.ConvertTo<T>(defaultValue);
        }

        /// <summary>
        /// Copies the readable and writable public property values from the target object to the source
        /// </summary>
        /// <remarks>The source and target objects must be of the same type</remarks>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        public static void CopyPropertiesFrom(this object source, object target)
        {
            source.CopyPropertiesFrom(target, string.Empty);
        }

        /// <summary>
        /// Copies the readable and writable public property values from the target object to the source and
        /// optionally allows for the ignoring of any number of properties
        /// </summary>
        /// <remarks>The source and target objects must be of the same type</remarks>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="ignoreProperty">A single property name to ignore</param>
        public static void CopyPropertiesFrom(this object source, object target, string ignoreProperty)
        {
            source.CopyPropertiesFrom(target, new string[] { ignoreProperty });
        }

        /// <summary>
        /// Copies the readable and writable public property values from the target object to the source and
        /// optionally allows for the ignoring of any number of properties
        /// </summary>
        /// <remarks>The source and target objects must be of the same type</remarks>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="ignoreProperties">An array of property names to ignore</param>
        public static void CopyPropertiesFrom(this object source, object target, string[] ignoreProperties)
        {
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
        /// Converts an object to the specified target type or returns the default value if
        /// those 2 types are not convertible.
        /// <para>Any exceptions are optionally ignored (<paramref name="ignoreException"/>).</para>
        /// <para>
        /// If the exceptions are not ignored and the <paramref name="source"/> can't be convert even if
        /// the types are convertible with each other, an exception is thrown.</para>
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "source">The value.</param>
        /// <param name = "defaultValue">The default value.</param>
        /// <returns>The target type</returns>
        private static T ConvertTo<T>(this object source, T defaultValue)
        {
            if (source != null)
            {
                var targetType = typeof(T);

                if (source.GetType() == targetType)
                {
                    return (T)source;
                }

                var converter = TypeDescriptor.GetConverter(source);
                if (converter != null)
                {
                    if (converter.CanConvertTo(targetType))
                    {
                        return (T)converter.ConvertTo(source, targetType);
                    }
                }

                converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null)
                {
                    if (converter.CanConvertFrom(source.GetType()))
                    {
                        return (T)converter.ConvertFrom(source);
                    }
                }
            }

            return defaultValue;
        }
    }
}
