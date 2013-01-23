//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
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
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, source);
                return memoryStream.ToArray();
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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(memoryStream);
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

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent /*, Encoding = new System.Text.UTF8Encoding(false)*/ }))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

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
        /// Deserializes XML string to object.
        /// </summary>
        /// <param name="source">The XML string to deserialize.</param>
        /// <param name="type">Type of object.</param>
        /// <returns>Instance of object.</returns>
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

            using (TextReader textReader = new StringReader(source))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                return xmlSerializer.Deserialize(textReader);
            }
        }

        /// <summary>
        /// Deserializes XML string to object.
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="returns"/> object.</typeparam>
        /// <param name="source">The XML string to deserialize.</param>
        /// <returns>Instance of T.</returns>
        public static T DeserializeXml<T>(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            using (TextReader textReader = new StringReader(source))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(textReader);
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
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);
                dataContractJsonSerializer.WriteObject(memoryStream, source);
                return (encoding ?? Encoding.Default).GetString(memoryStream.ToArray());
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

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.Default).GetBytes(source)))
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);
                return dataContractJsonSerializer.ReadObject(memoryStream);
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

            using (MemoryStream memoryStream = new MemoryStream((encoding ?? Encoding.Default).GetBytes(source)))
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);
                return (T)dataContractJsonSerializer.ReadObject(memoryStream);
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
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(source.GetType()) : new DataContractJsonSerializer(source.GetType(), knownTypes);
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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(type) : new DataContractJsonSerializer(type, knownTypes);
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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                DataContractJsonSerializer dataContractJsonSerializer = knownTypes == null ? new DataContractJsonSerializer(typeof(T)) : new DataContractJsonSerializer(typeof(T), knownTypes);
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

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration, Indent = indent }))
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

                dataContractSerializer.WriteObject(xmlWriter, source);

                xmlWriter.Flush();

                result = stringBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Deserializes DataContract XML string to object.
        /// </summary>
        /// <param name="source">The DataContract XML string to deserialize.</param>
        /// <param name="type">Type of DataContract object.</param>
        /// <param name="knownTypes">An IEnumerable of known types. Useful for complex objects.</param>
        /// <returns>Instance of DataContract object.</returns>
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

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source)))
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(type) : new DataContractSerializer(type, knownTypes);
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
        public static T DeserializeDataContractXml<T>(this string source, IEnumerable<Type> knownTypes = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source");
            }

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(source)))
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);
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

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(source.GetType()) : new DataContractSerializer(source.GetType(), knownTypes);

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

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                DataContractSerializer dataContractSerializer = knownTypes == null ? new DataContractSerializer(typeof(T)) : new DataContractSerializer(typeof(T), knownTypes);

                return (T)dataContractSerializer.ReadObject(memoryStream);
            }
        }
    }
}
