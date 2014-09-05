//-----------------------------------------------------------------------
// <copyright file="BinarySerialization.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Serializes and deserializes an object, or an entire graph of connected objects, in binary format.
    /// </summary>
    public static class BinarySerialization
    {
        /// <summary>
        /// Serializes object to bytes.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Byte array.</returns>
        public static byte[] Serialize(object source)
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
        /// <param name="filename">File name.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>File full path.</returns>
        public static string Write(object source, string filename, bool overwrite = false)
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
        public static object Deserialize(byte[] source)
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
        public static object Read(string source)
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
        /// <typeparam name="T">The type of <paramref name="returns"/> object.</typeparam>
        /// <param name="source">Byte array.</param>
        /// <returns>Instance of T.</returns>
        public static T Deserialize<T>(byte[] source)
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
        public static T Read<T>(string source)
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
    }
}
