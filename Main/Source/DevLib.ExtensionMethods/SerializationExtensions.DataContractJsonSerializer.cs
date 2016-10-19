//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.DataContractJsonSerializer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static partial class SerializationExtensions
    {
        /// <summary>
        /// Field JsonTypeInfoRegex.
        /// </summary>
        private static readonly Regex JsonTypeInfoRegex = new Regex("\\s*\"__type\"\\s*:\\s*\"[^\"]*\"\\s*,\\s*", RegexOptions.Compiled);

        /// <summary>
        /// Serializes object to Json string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="omitTypeInfo">Whether to omit type information.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Json string.</returns>
        public static string SerializeDataContractJsonString(this object source, bool omitTypeInfo = false, Encoding encoding = null, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                memoryStream.Position = 0;
                string result = (encoding ?? Encoding.UTF8).GetString(memoryStream.ToArray());

                if (omitTypeInfo)
                {
                    return JsonTypeInfoRegex.Replace(result, string.Empty);
                }
                else
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// Serializes object to Json string, write to file.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>File full path.</returns>
        public static string WriteDataContractJson(this object source, string filename, bool overwrite = false, Type[] knownTypes = null)
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

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (FileStream fileStream = File.OpenWrite(fullPath))
            {
                dataContractJsonSerializer.WriteObject(fileStream, source);
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes Json string to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeDataContractJsonString(this string source, Type type, Encoding encoding = null, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(source)))
            {
                memoryStream.Position = 0;
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Json string to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeDataContractJsonString(this string source, Type[] knownTypes, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (knownTypes == null || knownTypes.Length < 1)
            {
                throw new ArgumentException("knownTypes is null or empty.", "knownTypes");
            }

            Type sourceType = null;

            using (StringReader stringReader = new StringReader(source))
            {
                string rootNodeName = XElement.Load(stringReader).Name.LocalName;
                sourceType = knownTypes.FirstOrDefault(p => p.Name == rootNodeName);

                if (sourceType == null)
                {
                    throw new InvalidOperationException();
                }
            }

            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(sourceType, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(source)))
            {
                memoryStream.Position = 0;
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Json string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeDataContractJsonString<T>(this string source, Encoding encoding = null, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(source)))
            {
                memoryStream.Position = 0;
                return (T)dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadDataContractJson(this string source, Type type, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            string fullPath = Path.GetFullPath(source);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return dataContractJsonSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadDataContractJson(this string source, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (knownTypes == null || knownTypes.Length < 1)
            {
                throw new ArgumentException("knownTypes is null or empty.", "knownTypes");
            }

            string fullPath = Path.GetFullPath(source);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            Type sourceType = null;
            string rootNodeName = XElement.Load(fullPath).Name.LocalName;
            sourceType = knownTypes.FirstOrDefault(p => p.Name == rootNodeName);

            if (sourceType == null)
            {
                throw new InvalidOperationException();
            }

            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(sourceType, knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return dataContractJsonSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T ReadDataContractJson<T>(this string source, Type[] knownTypes = null)
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

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)dataContractJsonSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to Json bytes.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Json bytes.</returns>
        public static byte[] SerializeDataContractJsonBinary(this object source, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes Json bytes to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeDataContractJsonBinary(this byte[] source, Type type, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Json bytes to object.
        /// </summary>
        /// <param name="source">Json string object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        public static object DeserializeDataContractJsonBinary(this byte[] source, Type[] knownTypes)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (knownTypes == null || knownTypes.Length < 1)
            {
                throw new ArgumentException("knownTypes is null or empty.", "knownTypes");
            }

            Type sourceType = null;

            using (MemoryStream memoryStream = new MemoryStream(source))
            using (XmlReader xmlReader = XmlReader.Create(memoryStream))
            {
                string rootNodeName = XElement.Load(xmlReader).Name.LocalName;
                sourceType = knownTypes.FirstOrDefault(p => p.Name == rootNodeName);

                if (sourceType == null)
                {
                    throw new InvalidOperationException();
                }

                memoryStream.Position = 0;
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(sourceType, knownTypes);
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Json bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeDataContractJsonBinary<T>(this byte[] source, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return (T)dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }
    }
}
