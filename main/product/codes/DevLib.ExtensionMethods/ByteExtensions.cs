//-----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    /// <summary>
    /// Defines the various compression types that are available
    /// </summary>
    public enum CompressionType
    {
        GZip = 0,
        Deflate = 1
    }

    /// <summary>
    /// Byte Extensions
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Convert bytes to Hex string
        /// </summary>
        /// <param name="source">Byte array</param>
        /// <param name="addSpace">Whether add space between Hex</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(this byte[] source, bool addSpace = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder(source.Length * 2);

            if (addSpace)
            {
                foreach (byte hex in source)
                {
                    result.AppendFormat("{0:X2}", hex);
                    result.Append(" ");
                }
            }
            else
            {
                foreach (byte hex in source)
                {
                    result.AppendFormat("{0:X2}", hex);
                }
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// Convert byte to Hex string
        /// </summary>
        /// <param name="source">Byte</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(this byte source)
        {
            return Convert.ToString(source, 16).PadLeft(2, '0');
        }

        /// <summary>
        /// Convert bytes to Encoding string by using Encoding.Default
        /// </summary>
        /// <param name="source">Byte array</param>
        /// <returns>Encoding string by using Encoding.Default</returns>
        public static string ToEncodingString(this byte[] source)
        {
            return source.ToEncodingString(Encoding.Default);
        }

        /// <summary>
        /// Convert bytes to Encoding string
        /// </summary>
        /// <param name="source">Byte array</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Encoding string</returns>
        public static string ToEncodingString(this byte[] source, Encoding encoding)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                return string.Empty;
            }

            return encoding.GetString(source);
        }

        /// <summary>
        /// Convert bytes to Image
        /// </summary>
        /// <param name="source">Byte array</param>
        /// <returns>Image object</returns>
        public static Image ToImage(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(source))
                {
                    memoryStream.Write(source, 0, source.Length);
                    return Image.FromStream(memoryStream);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert bytes to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Byte array</param>
        /// <returns>Object</returns>
        public static T ToObject<T>(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                return default(T);
            }

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Convert bytes to object
        /// </summary>
        /// <param name="source">Byte array</param>
        /// <returns>Object</returns>
        public static object ToObject(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Length == 0)
            {
                return null;
            }

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Compresses byte array using CompressionType
        /// </summary>
        /// <param name="source">Byte array to compress</param>
        /// <param name="compressionType">Compression Type</param>
        /// <returns>A compressed byte array</returns>
        public static byte[] Compress(this byte[] source, CompressionType compressionType = CompressionType.GZip)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            MemoryStream outputStream = null;
            byte[] result = null;

            try
            {
                outputStream = new MemoryStream();
                Stream zipStream = GetZipStream(outputStream, CompressionMode.Compress, compressionType);
                zipStream.Write(source, 0, source.Length);
                zipStream.Close();
                result = outputStream.ToArray();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (outputStream != null)
                {
                    outputStream.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Decompresses byte array using CompressionType
        /// </summary>
        /// <param name="source">Byte array to decompress</param>
        /// <param name="compressionType">Compression Type</param>
        /// <returns>A decompressed byte array</returns>
        public static byte[] Decompress(this byte[] source, CompressionType compressionType = CompressionType.GZip)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            MemoryStream outputStream = null;
            MemoryStream inputStream = null;
            byte[] result = null;

            try
            {
                outputStream = new MemoryStream();
                inputStream = new MemoryStream(source);
                Stream zipStream = GetZipStream(inputStream, CompressionMode.Decompress, compressionType);
                zipStream.CopyTo(outputStream);
                zipStream.Close();
                result = outputStream.ToArray();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                }

                if (outputStream != null)
                {
                    outputStream.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Get Zip Stream by type
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="compressionMode"></param>
        /// <param name="compressionType"></param>
        /// <returns></returns>
        private static Stream GetZipStream(MemoryStream memoryStream, CompressionMode compressionMode, CompressionType compressionType)
        {
            if (compressionType == CompressionType.GZip)
            {
                return new GZipStream(memoryStream, compressionMode);
            }
            else
            {
                return new DeflateStream(memoryStream, compressionMode);
            }
        }
    }
}
