//-----------------------------------------------------------------------
// <copyright file="SerializationExtensions.SoapFormatter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Xml;

    /// <summary>
    /// Serialization Extensions.
    /// </summary>
    public static partial class SerializationExtensions
    {
        /// <summary>
        /// Serializes object to Soap string.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>Soap string.</returns>
        public static string SerializeSoapString(this object source)
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
        public static object DeserializeSoapString(this string source)
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
        /// Deserializes Soap string to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The Soap string to deserialize.</param>
        /// <returns>Instance of T.</returns>
        public static T DeserializeSoapString<T>(this string source)
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
        /// Serializes object to Soap bytes.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>Bytes representation of the source object.</returns>
        public static byte[] SerializeSoapBinary(this object source)
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
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes Soap bytes to object.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <returns>Instance of Soap object.</returns>
        public static object DeserializeSoapBinary(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SoapFormatter soapFormatter = new SoapFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return soapFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes Soap bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The bytes to deserialize.</param>
        /// <returns>Instance of Soap object.</returns>
        public static T DeserializeSoapBinary<T>(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            SoapFormatter soapFormatter = new SoapFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return (T)soapFormatter.Deserialize(memoryStream);
            }
        }
    }
}
