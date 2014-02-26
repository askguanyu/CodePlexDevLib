//-----------------------------------------------------------------------
// <copyright file="XmlSerialization.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Serialization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Serializes and deserializes objects into and from XML documents.
    /// </summary>
    public static class XmlSerialization
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
        public static string Serialize(object source, bool indent = false, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true, Type[] extraTypes = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string result = null;

            StringBuilder stringBuilder = new StringBuilder();

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

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
        /// Serializes object to Xml string, write to file.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="source">The object to serialize.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="indent">Whether to write individual elements on new lines and indent.</param>
        /// <param name="omitXmlDeclaration">Whether to write an Xml declaration.</param>
        /// <param name="removeDefaultNamespace">Whether to write default namespace.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>File full path.</returns>
        public static string Write(object source, string fileName, bool overwrite = false, bool indent = true, bool omitXmlDeclaration = true, bool removeDefaultNamespace = true, Type[] extraTypes = null)
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

            XmlSerializer xmlSerializer = (extraTypes == null || extraTypes.Length == 0) ? new XmlSerializer(source.GetType()) : new XmlSerializer(source.GetType(), extraTypes);

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
        /// Deserializes Xml string to object.
        /// </summary>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="type">Type of object.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of object.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object Deserialize(string source, Type type, Type[] extraTypes = null)
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
        public static object Deserialize(string source, Type[] knownTypes)
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
                XmlDocument xmlDocument = new XmlDocument();

                xmlDocument.Load(stringReader);

                string rootNodeName = xmlDocument.LocalName;

                foreach (var item in knownTypes)
                {
                    if (item.Name.Equals(rootNodeName, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceType = item;
                        break;
                    }
                }

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
        public static object Read(string source, Type type, Type[] extraTypes = null)
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
        public static object Read(string source, Type[] knownTypes)
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

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.Load(source);

            string rootNodeName = xmlDocument.LocalName;

            foreach (var item in knownTypes)
            {
                if (item.Name.Equals(rootNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    sourceType = item;
                    break;
                }
            }

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
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The Xml string to deserialize.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of T.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static T Deserialize<T>(string source, Type[] extraTypes = null)
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
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">File name.</param>
        /// <param name="extraTypes">A <see cref="T:System.Type" /> array of additional object types to serialize.</param>
        /// <returns>Instance of T.</returns>
        public static T Read<T>(string source, Type[] extraTypes = null)
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
    }
}
