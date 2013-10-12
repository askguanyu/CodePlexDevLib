//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static class SerializationExtensions
    {
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
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Serializes object to bytes, write to file.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>File full path.</returns>
        public static string WriteBinary(this object source, string fileName, bool overwrite = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fileName))
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return binaryFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes bytes to object.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="returns"/> object.</typeparam>
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
        /// <typeparam name="T">The type of <paramref name="returns"/> object.</typeparam>
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (T)binaryFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to XML string.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an XML declaration.</param>
        /// <param name="removeDefaultNamespace">Whether to write default namespace.</param>
        /// <returns>An XML encoded string representation of the source object.</returns>
        public static string SerializeXml(this object source, bool indent = false, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string result = null;

            StringBuilder stringBuilder = new StringBuilder();
            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent /*, Encoding = new System.Text.UTF8Encoding(false)*/ }))
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

                result = stringBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Serializes object to XML string, write to file.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an XML declaration.</param>
        /// <param name="removeDefaultNamespace">Whether to write default namespace.</param>
        /// <returns>File full path.</returns>
        public static string WriteXml(this object source, string fileName, bool overwrite = false, bool indent = true, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fileName))
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

            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

            using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent /*, Encoding = new System.Text.UTF8Encoding(false)*/ }))
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

                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes XML string to object.
        /// </summary>
        /// <param name="source">The XML string to deserialize.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeXml(this string source, Type type)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            XmlSerializer xmlSerializer = new XmlSerializer(type);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return xmlSerializer.Deserialize(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes XML string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadXml(this string source, Type type)
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

            XmlSerializer xmlSerializer = new XmlSerializer(type);

            using (XmlReader xmlReader = XmlReader.Create(fullPath, new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return xmlSerializer.Deserialize(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes XML string to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The XML string to deserialize.</param>
        /// <returns>Instance of T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static T DeserializeXml<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return (T)xmlSerializer.Deserialize(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes XML string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">File name.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadXml<T>(this string source)
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

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (XmlReader xmlReader = XmlReader.Create(fullPath, new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return (T)xmlSerializer.Deserialize(xmlReader);
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
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>File full path.</returns>
        public static string WriteSoap(this object source, string fileName, bool overwrite = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fileName))
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return soapFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Deserializes Soap string to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
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
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
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

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (T)soapFormatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to JSON string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>JSON string.</returns>
        public static string SerializeJsonString(this object source, Encoding encoding = null, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                return (encoding ?? Encoding.Default).GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Serializes object to JSON string, write to file.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>File full path.</returns>
        public static string WriteJson(this object source, string fileName, bool overwrite = false, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fileName))
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

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (Stream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                dataContractJsonSerializer.WriteObject(fileStream, source);
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes JSON string to object.
        /// </summary>
        /// <param name="source">JSON string object.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeJsonString(this string source, Type type, Encoding encoding = null, IEnumerable<Type> knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.Default).GetBytes(source)))
            {
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes JSON string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static object ReadJson(this string source, Type type, IEnumerable<Type> knownTypes = null)
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

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return dataContractJsonSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Deserializes JSON string to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> objet.</typeparam>
        /// <param name="source">JSON string object.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonString<T>(this string source, Encoding encoding = null, IEnumerable<Type> knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.Default).GetBytes(source)))
            {
                return (T)dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes JSON string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> objet.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static T ReadJson<T>(this string source, IEnumerable<Type> knownTypes = null)
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

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (T)dataContractJsonSerializer.ReadObject(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to JSON bytes.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>JSON bytes.</returns>
        public static byte[] SerializeJsonBinary(this object source, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes JSON bytes to object.
        /// </summary>
        /// <param name="source">JSON string object.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static object DeserializeJsonBinary(this byte[] source, Type type, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes JSON bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> objet.</typeparam>
        /// <param name="source">JSON string object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of object.</returns>
        public static T DeserializeJsonBinary<T>(this byte[] source, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return (T)dataContractJsonSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Serializes DataContract object to XML string.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an XML declaration.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>An XML encoded string representation of the source DataContract object.</returns>
        public static string SerializeDataContractXml(this object source, bool indent = false, bool omitXmlDeclaration = true, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string result = null;

            StringBuilder stringBuilder = new StringBuilder();
            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent }))
            {
                dataContractSerializer.WriteObject(xmlWriter, source);
                xmlWriter.Flush();
                result = stringBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Serializes DataContract object to XML string, write to file.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an XML declaration.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>File full path.</returns>
        public static string WriteDataContract(this object source, string fileName, bool overwrite = false, bool indent = true, bool omitXmlDeclaration = true, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fileName))
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

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent }))
            {
                dataContractSerializer.WriteObject(xmlWriter, source);
                return fullPath;
            }
        }

        /// <summary>
        /// Deserializes DataContract XML string to object.
        /// </summary>
        /// <param name="source">The DataContract XML string to deserialize.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of DataContract object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object DeserializeDataContractXml(this string source, Type type, IEnumerable<Type> knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract XML string to object, read from file.
        /// </summary>
        /// <param name="source">File name.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static object ReadDataContract(this string source, Type type, IEnumerable<Type> knownTypes = null)
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

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(fullPath, new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract XML string to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The DataContract XML string to deserialize.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static T DeserializeDataContractXml<T>(this string source, IEnumerable<Type> knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return (T)dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes DataContract XML string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadDataContract<T>(this string source, IEnumerable<Type> knownTypes = null)
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

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (XmlReader xmlReader = XmlReader.Create(fullPath, new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
            {
                return (T)dataContractSerializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Serializes DataContract object to bytes.
        /// </summary>
        /// <param name="source">The DataContract object to serialize.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Bytes representation of the source DataContract object.</returns>
        public static byte[] SerializeDataContractBinary(this object source, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(memoryStream, source);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes DataContract bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static object DeserializeDataContractBinary(this byte[] source, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return dataContractSerializer.ReadObject(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes DataContract bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of DataContract object.</returns>
        public static T DeserializeDataContractBinary<T>(this byte[] source, IEnumerable<Type> knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return (T)dataContractSerializer.ReadObject(memoryStream);
            }
        }
    }
}
