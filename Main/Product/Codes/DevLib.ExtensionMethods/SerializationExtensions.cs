//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Field JsonTypeInfoRegex.
        /// </summary>
        private static readonly Regex JsonTypeInfoRegex = new Regex("\\s*\"__type\"\\s*:\\s*\"[^\"]*\"\\s*,\\s*", RegexOptions.Compiled);

        /// <summary>
        /// Serializes object to bytes.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Byte array.</returns>
        public static byte[] SerializeBinary(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Serializes object to bytes, write to file.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>File full path.</returns>
        public static string WriteBinary(this object source, string filename, bool overwrite = false)
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

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.OpenWrite(fullPath))
            {
                binaryFormatter.Serialize(fileStream, source);
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes bytes to object.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <returns>Instance object.</returns>
        public static object DeserializeBinary(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                throw new ArgumentOutOfRangeException("source");
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes bytes to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <returns>Instance object.</returns>
        public static object ReadBinary(this string source)
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

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return binaryFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes bytes to object.
        /// </summary>
        /// <typeparam name="T">The type of returns object.</typeparam>
        /// <param name="source">Byte array.</param>
        /// <returns>Instance of T.</returns>
        public static T DeserializeBinary<T>(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                throw new ArgumentOutOfRangeException("source");
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes bytes to object, read from file.
        /// </summary>
        /// <typeparam name="T">The type of returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadBinary<T>(this string source)
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

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)binaryFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to Xml string.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="removeDefaultNamespace">Whether to write default namespace.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>An Xml encoded string representation of the source object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static string SerializeXml(this object source, bool indent = false, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true, Type[] extraTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            MemoryStream memoryStream = new MemoryStream();
            StreamReader streamReader = new StreamReader(memoryStream);

            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent, Encoding = new UTF8Encoding(false), CloseOutput = true }))
            {
                if (removeDefaultNamespace)
                {
                    XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                    xmlns.Add(string.Empty, string.Empty);
                    xmlSerializer.Serialize(xmlWriter, source, xmlns);
                }
                else
                {
                    xmlSerializer.Serialize(xmlWriter, source);
                }

                xmlWriter.Flush();
                memoryStream.Position = 0;

                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Serializes object to Xml string, write to file.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="removeDefaultNamespace">Whether to write default namespace.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>File full path.</returns>
        public static string WriteXml(this object source, string filename, bool overwrite = false, bool indent = true, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true, Type[] extraTypes = null)
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

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent, Encoding = new UTF8Encoding(false), CloseOutput = true }))
            {
                if (removeDefaultNamespace)
                {
                    XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                    xmlns.Add(string.Empty, string.Empty);
                    xmlSerializer.Serialize(xmlWriter, source, xmlns);
                }
                else
                {
                    xmlSerializer.Serialize(xmlWriter, source);
                }

                xmlWriter.Flush();

                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes Xml string to object.
        /// </summary>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeXml(this string source, Type type, Type[] extraTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(type) : new XmlSerializer(type, extraTypes);

            using (StringReader stringReader = new StringReader(source))
            {
                return xmlSerializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Deserializes Xml string to object.
        /// </summary>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeXml(this string source, Type[] knownTypes)
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

                sourceType = knownTypes.FirstOrDefault(p => p.Name.Equals(rootNodeName, StringComparison.OrdinalIgnoreCase));

                if (sourceType == null)
                {
                    throw new InvalidOperationException();
                }
            }

            XmlSerializer xmlSerializer = new XmlSerializer(sourceType, knownTypes);

            using (StringReader stringReader = new StringReader(source))
            {
                return xmlSerializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Deserializes Xml string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadXml(this string source, Type type, Type[] extraTypes = null)
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

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(type) : new XmlSerializer(type, extraTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return xmlSerializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Xml string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadXml(this string source, Type[] knownTypes)
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

            sourceType = knownTypes.FirstOrDefault(p => p.Name.Equals(rootNodeName, StringComparison.OrdinalIgnoreCase));

            if (sourceType == null)
            {
                throw new InvalidOperationException();
            }

            XmlSerializer xmlSerializer = new XmlSerializer(sourceType, knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return xmlSerializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Xml string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static T DeserializeXml<T>(this string source, Type[] extraTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

            using (StringReader stringReader = new StringReader(source))
            {
                return (T)xmlSerializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Deserializes Xml string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadXml<T>(this string source, Type[] extraTypes = null)
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

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)xmlSerializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to Soap string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>Soap string.</returns>
        public static string SerializeSoap(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SoapFormatter soapFormatter = new SoapFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                soapFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(memoryStream);

                return xmlDocument.InnerXml;
            }
        }

        /// <summary>
        /// Serializes object to Soap string, write to file.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>File full path.</returns>
        public static string WriteSoap(this object source, string filename, bool overwrite = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
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

            SoapFormatter soapFormatter = new SoapFormatter();

            using (FileStream fileStream = File.OpenWrite(fullPath))
            {
                soapFormatter.Serialize(fileStream, source);
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes Soap string to object.
        /// </summary>
        /// <param name="source">The Soap string to deserialize.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeSoap(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(source);

            SoapFormatter soapFormatter = new SoapFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlDocument.Save(memoryStream);
                memoryStream.Position = 0;

                return soapFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Soap string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadSoap(this string source)
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

            SoapFormatter soapFormatter = new SoapFormatter();

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return soapFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Soap string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The Soap string to deserialize.</param>
        /// <returns>Instance of T.</returns>
        public static T DeserializeSoap<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(source);

            SoapFormatter soapFormatter = new SoapFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlDocument.Save(memoryStream);
                memoryStream.Position = 0;

                return (T)soapFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Soap string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadSoap<T>(this string source)
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

            SoapFormatter soapFormatter = new SoapFormatter();

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)soapFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to Json string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="omitTypeInfo">Whether to omit type information.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Json string.</returns>
        public static string SerializeJsonString(this object source, bool omitTypeInfo = false, Encoding encoding = null, Type[] knownTypes = null)
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
        public static string WriteJson(this object source, string filename, bool overwrite = false, Type[] knownTypes = null)
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
        public static object DeserializeJsonString(this string source, Type type, Encoding encoding = null, Type[] knownTypes = null)
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
        public static object DeserializeJsonString(this string source, Type[] knownTypes, Encoding encoding = null)
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
        /// Deserializes Json string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadJson(this string source, Type type, Type[] knownTypes = null)
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
        public static object ReadJson(this string source, Type[] knownTypes = null)
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
        /// Deserializes Json string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns objet.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonString<T>(this string source, Encoding encoding = null, Type[] knownTypes = null)
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
        /// <typeparam name="T">Type of the returns objet.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T ReadJson<T>(this string source, Type[] knownTypes = null)
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
        public static byte[] SerializeJsonBinary(this object source, Type[] knownTypes = null)
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
        public static object DeserializeJsonBinary(this byte[] source, Type type, Type[] knownTypes = null)
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeJsonBinary(this byte[] source, Type[] knownTypes)
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

            MemoryStream memoryStream = new MemoryStream(source);
            memoryStream.Position = 0;

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
        /// <typeparam name="T">Type of the returns objet.</typeparam>
        /// <param name="source">Json string object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonBinary<T>(this byte[] source, Type[] knownTypes = null)
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

        /// <summary>
        /// Serializes DataContract object to Xml string.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>An Xml encoded string representation of the source DataContract object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static string SerializeDataContractXml(this object source, bool indent = false, bool omitXmlDeclaration = true, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            MemoryStream memoryStream = new MemoryStream();
            StreamReader streamReader = new StreamReader(memoryStream);

            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent, Encoding = new UTF8Encoding(false), CloseOutput = true }))
            {
                dataContractSerializer.WriteObject(xmlWriter, source);
                xmlWriter.Flush();
                memoryStream.Position = 0;

                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Serializes DataContract object to Xml string, write to file.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>File full path.</returns>
        public static string WriteDataContract(this object source, string filename, bool overwrite = false, bool indent = true, bool omitXmlDeclaration = true, Type[] knownTypes = null)
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

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent, Encoding = new UTF8Encoding(false), CloseOutput = true }))
            {
                dataContractSerializer.WriteObject(xmlWriter, source);
                xmlWriter.Flush();
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object.
        /// </summary>
        /// <param name="source">The DataContract Xml string to deserialize.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of DataContract object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeDataContractXml(this string source, Type type, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true, CloseInput = true }))
            {
                return dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object.
        /// </summary>
        /// <param name="source">The DataContract Xml string to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of DataContract object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeDataContractXml(this string source, Type[] knownTypes)
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

            DataContractSerializer dataContractSerializer = new DataContractSerializer(sourceType, knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true, CloseInput = true }))
            {
                return dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static object ReadDataContract(this string source, Type type, Type[] knownTypes = null)
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

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return dataContractSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static object ReadDataContract(this string source, Type[] knownTypes)
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

            DataContractSerializer dataContractSerializer = new DataContractSerializer(sourceType, knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return dataContractSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The DataContract Xml string to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static T DeserializeDataContractXml<T>(this string source, Type[] knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true, CloseInput = true }))
            {
                return (T)dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract Xml string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadDataContract<T>(this string source, Type[] knownTypes = null)
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

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)dataContractSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Serializes DataContract object to bytes.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Bytes representation of the source DataContract object.</returns>
        public static byte[] SerializeDataContractBinary(this object source, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(memoryStream, source);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes DataContract bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static object DeserializeDataContractBinary(this byte[] source, Type type, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return dataContractSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes DataContract bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of DataContract object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeDataContractBinary(this byte[] source, Type[] knownTypes = null)
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

            MemoryStream memoryStream = new MemoryStream(source);
            memoryStream.Position = 0;

            using (XmlReader xmlReader = XmlReader.Create(memoryStream))
            {
                string rootNodeName = XElement.Load(xmlReader).Name.LocalName;

                sourceType = knownTypes.FirstOrDefault(p => p.Name == rootNodeName);

                if (sourceType == null)
                {
                    throw new InvalidOperationException();
                }

                memoryStream.Position = 0;
                DataContractSerializer dataContractSerializer = new DataContractSerializer(sourceType, knownTypes);
                return dataContractSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes DataContract bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static T DeserializeDataContractBinary<T>(this byte[] source, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = (knownTypes == null || knownTypes.Length == 0) ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return (T)dataContractSerializer.ReadObject(memoryStream);
            }
        }
    }
}
