//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.XmlSerializer.cs" company="YuGuan Corporation">
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
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static partial class SerializationExtensions
    {
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
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        public static string SerializeXmlString(this object source, bool indent = false, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true, Type[] extraTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader streamReader = new StreamReader(memoryStream))
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
        public static object DeserializeXmlString(this string source, Type type, Type[] extraTypes = null)
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
        public static object DeserializeXmlString(this string source, Type[] knownTypes)
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
        /// Deserializes Xml string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of T.</returns>
        public static T DeserializeXmlString<T>(this string source, Type[] extraTypes = null)
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
        /// Serializes object to Xml bytes.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Bytes representation of the source object.</returns>
        public static byte[] SerializeXmlBinary(this object source, Type[] extraTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes Xml bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="type">Type of Xml object.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of Xml object.</returns>
        public static object DeserializeXmlBinary(this byte[] source, Type type, Type[] extraTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(type) : new XmlSerializer(type, extraTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return xmlSerializer.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Xml bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array of object types to serialize.</param>
        /// <returns>Instance of Xml object.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        public static object DeserializeXmlBinary(this byte[] source, Type[] knownTypes = null)
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
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(memoryStream);
                string rootNodeName = xmlDocument.LocalName;

                sourceType = knownTypes.FirstOrDefault(p => p.Name == rootNodeName);

                if (sourceType == null)
                {
                    throw new InvalidOperationException();
                }

                memoryStream.Position = 0;

                XmlSerializer xmlSerializer = new XmlSerializer(sourceType, knownTypes);

                return xmlSerializer.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Xml bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The bytes to deserialize.</param>
        /// <param name="knownTypes">A <see cref="T:System.Type" /> array that may be present in the object graph.</param>
        /// <returns>Instance of Xml object.</returns>
        public static T DeserializeXmlBinary<T>(this byte[] source, Type[] knownTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            XmlSerializer xmlSerializer = (knownTypes == null || knownTypes.Length == 0) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), knownTypes);

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }
    }
}
