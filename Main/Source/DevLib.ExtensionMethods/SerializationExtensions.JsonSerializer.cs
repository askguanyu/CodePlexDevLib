//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.JsonSerializer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static partial class SerializationExtensions
    {
        /// <summary>
        /// Serializes object to Json string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="omitTypeInfo">Whether to omit type information.</param>
        /// <returns>Json string.</returns>
        public static string SerializeJsonString(this object source, bool omitTypeInfo = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string result = JsonSerializer.Serialize(source);

            if (omitTypeInfo)
            {
                return JsonTypeInfoRegex.Replace(result, string.Empty);
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Serializes object to Json string, write to file.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="omitTypeInfo">Whether to omit type information.</param>
        /// <returns>File full path.</returns>
        public static string WriteJson(this object source, string filename, bool overwrite = false, bool omitTypeInfo = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(filename))
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }
                catch
                {
                    throw;
                }
            }

            string result = JsonSerializer.Serialize(source);

            if (omitTypeInfo)
            {
                result = JsonTypeInfoRegex.Replace(result, string.Empty);
            }

            File.WriteAllText(fullPath, result);

            return fullPath;
        }

        /// <summary>
        /// Deserializes Json string to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeJsonString(this string source, Type type = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            return JsonSerializer.Deserialize(source, type);
        }

        /// <summary>
        /// Deserializes Json string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonString<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            return JsonSerializer.Deserialize<T>(source);
        }

        /// <summary>
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadJson(this string source, Type type = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(source);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            return JsonSerializer.Deserialize(File.ReadAllText(fullPath), type);
        }

        /// <summary>
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <returns>Instance of object.</returns>
        public static T ReadJson<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(source);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            return JsonSerializer.Deserialize<T>(File.ReadAllText(fullPath));
        }

        /// <summary>
        /// Serializes object to Json bytes.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <returns>Json bytes.</returns>
        public static byte[] SerializeJsonBinary(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source));
        }

        /// <summary>
        /// Deserializes Json bytes to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeJsonBinary(this byte[] source, Type type = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return JsonSerializer.Deserialize(Encoding.UTF8.GetString(source), type);
        }

        /// <summary>
        /// Deserializes Json bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonBinary<T>(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(source));
        }
    }
}
