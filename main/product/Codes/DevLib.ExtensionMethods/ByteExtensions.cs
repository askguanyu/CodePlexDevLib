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
        /// <param name="delimiter">Delimiter character.</param>
        /// <returns>Hex string.</returns>
        public static string ToHexString(this IList<byte> source, char? delimiter = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            char[] result = new char[source.Count * 2];

            byte tempByte;

            for (int bx = 0, cx = 0; bx < source.Count; ++bx, ++cx)
            {
                tempByte = (byte)(source[bx] >> 4);
                result[cx] = (char)(tempByte > 9 ? tempByte + 0x37 + 0x20 : tempByte + 0x30);

                tempByte = (byte)(source[bx] & 0x0F);
                result[++cx] = (char)(tempByte > 9 ? tempByte + 0x37 + 0x20 : tempByte + 0x30);
            }

            if (delimiter != null)
            {
                char[] resultWithDelimiter = new char[(source.Count * 3)];

                for (int i = 0; i < source.Count; i++)
                {
                    resultWithDelimiter[i * 3] = result[i * 2];
                    resultWithDelimiter[(i * 3) + 1] = result[(i * 2) + 1];
                    resultWithDelimiter[(i * 3) + 2] = delimiter.Value;
                }

                return new string(resultWithDelimiter).TrimEnd(delimiter.Value).ToUpperInvariant();
            }
            else
            {
                return new string(result).ToUpperInvariant();
            }
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

            return (encoding ?? Encoding.UTF8).GetString(source);
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
