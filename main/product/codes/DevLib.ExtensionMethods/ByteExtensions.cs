//-----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Defines the various compression types that are available.
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// Represents GZipStream.
        /// </summary>
        GZip = 0,

        /// <summary>
        /// Represents DeflateStream.
        /// </summary>
        Deflate = 1
    }

    /// <summary>
    /// Byte Extensions.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Convert bytes to Hex string.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <param name="addSpace">Whether add space between Hex.</param>
        /// <returns>Hex string.</returns>
        public static string ToHexString(this IEnumerable<byte> source, bool addSpace = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            IEnumerator<byte> enumerator = source.GetEnumerator();

            List<string> result = new List<string>();

            while (enumerator.MoveNext())
            {
                result.Add(string.Format("{0:X2}", enumerator.Current));
            }

            return string.Join(addSpace ? " " : string.Empty, result.ToArray());
        }

        /// <summary>
        /// Convert byte to Hex string.
        /// </summary>
        /// <param name="source">Source byte.</param>
        /// <returns>Hex string.</returns>
        public static string ToHexString(this byte source)
        {
            return Convert.ToString(source, 16).PadLeft(2, '0');
        }

        /// <summary>
        /// Convert bytes to Encoding string.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <param name="encoding">Instance of Encoding.</param>
        /// <returns>Encoding string.</returns>
        public static string ToEncodingString(this byte[] source, Encoding encoding = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (encoding ?? Encoding.Unicode).GetString(source);
        }

        /// <summary>
        /// Convert bytes to Image.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <returns>Image object.</returns>
        public static Image ToImage(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Write(source, 0, source.Length);
                return Image.FromStream(memoryStream);
            }
        }

        /// <summary>
        /// Compresses byte array using CompressionType.
        /// </summary>
        /// <param name="source">Byte array to compress.</param>
        /// <param name="compressionType">Compression Type.</param>
        /// <returns>A compressed byte array.</returns>
        public static byte[] Compress(this byte[] source, CompressionType compressionType = CompressionType.GZip)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (Stream zipStream = GetZipStream(outputStream, CompressionMode.Compress, compressionType))
                {
                    zipStream.Write(source, 0, source.Length);
                }

                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses byte array using CompressionType.
        /// </summary>
        /// <param name="source">Byte array to decompress.</param>
        /// <param name="compressionType">Compression Type.</param>
        /// <returns>A decompressed byte array.</returns>
        public static byte[] Decompress(this byte[] source, CompressionType compressionType = CompressionType.GZip)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (MemoryStream inputStream = new MemoryStream(source))
                {
                    using (Stream zipStream = GetZipStream(inputStream, CompressionMode.Decompress, compressionType))
                    {
                        byte[] array = new byte[81920];
                        int count;
                        while ((count = zipStream.Read(array, 0, array.Length)) != 0)
                        {
                            outputStream.Write(array, 0, count);
                        }
                    }
                }

                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Get Zip Stream by type.
        /// </summary>
        /// <param name="memoryStream">Instance of MemoryStream.</param>
        /// <param name="compressionMode">Compression mode.</param>
        /// <param name="compressionType">Compression type.</param>
        /// <returns>Instance of Stream.</returns>
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
