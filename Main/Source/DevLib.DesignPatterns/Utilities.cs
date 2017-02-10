//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Cryptography;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Class Utilities.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Field DateTimeFormat.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffUzzz";

        /// <summary>
        /// The ticks factor.
        /// </summary>
        private const long TicksFactor = 10000;

        /// <summary>
        /// Provides cryptographically strong random data for GUID creation.
        /// </summary>
        private static readonly RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// Serializes object to Xml bytes.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>Bytes representation of the source object.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        public static byte[] SerializeXmlBinary(object source)
        {
            if (source == null)
            {
                return null;
            }

            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader streamReader = new StreamReader(memoryStream))
            using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings() { OmitXmlDeclaration = true, CloseOutput = true }))
            {
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(xmlWriter, source, xmlns);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes Xml bytes to object.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="source">The bytes to deserialize.</param>
        /// <returns>Instance of Xml object.</returns>
        public static T DeserializeXmlBinary<T>(byte[] source)
        {
            if (source == null)
            {
                return default(T);
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes the XML binary to string.
        /// </summary>
        /// <param name="source">The bytes to deserialize.</param>
        /// <returns>Xml string.</returns>
        public static string DeserializeXmlBinaryString(byte[] source)
        {
            if (source == null)
            {
                return string.Empty;
            }

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(memoryStream);
                return xmlDocument.InnerXml;
            }
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the synchronize root.
        /// </summary>
        /// <param name="obj">The ICollection object.</param>
        /// <returns>The SyncRoot object.</returns>
        public static object GetSyncRoot(ICollection obj)
        {
            return obj.SyncRoot;
        }

        /// <summary>
        /// Generates a new GUID value which is sequentially ordered by the least significant six bytes of the Data4 block.
        /// Optimize for: SQL Server - uniqueidentifier;
        /// </summary>
        /// <returns>New sequential at end Guid instance.</returns>
        public static Guid NewSequentialGuid()
        {
            // slower but more random
            byte[] randomBytes = new byte[10];
            RandomGenerator.GetBytes(randomBytes);

            ////// faster but less random
            ////byte[] randomBytes = Guid.NewGuid().ToByteArray();

            long timestamp = DateTime.UtcNow.Ticks / TicksFactor;
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
            Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);

            return new Guid(guidBytes);
        }
    }
}
