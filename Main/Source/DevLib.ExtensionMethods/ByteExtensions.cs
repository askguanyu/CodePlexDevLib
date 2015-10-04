//-----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections;
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
        /// Convert BitArray to byte array.
        /// </summary>
        /// <param name="source">The source BitArray.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ToByteArray(this BitArray source)
        {
            byte[] result = new byte[((source.Length - 1) / 8) + 1];

            source.CopyTo(result, 0);

            return result;
        }

        /// <summary>
        /// Convert BitArray to bool array.
        /// </summary>
        /// <param name="source">The source BitArray.</param>
        /// <returns>bool array.</returns>
        public static bool[] ToBoolArray(this BitArray source)
        {
            bool[] result = new bool[source.Length];

            source.CopyTo(result, 0);

            return result;
        }

        /// <summary>
        /// Convert BitArray to binary digit Int32 array.
        /// </summary>
        /// <param name="source">The source BitArray.</param>
        /// <returns>Binary digit Int32 array.</returns>
        public static int[] ToBitIntArray(this BitArray source)
        {
            int[] result = new int[source.Length];

            source.CopyTo(result, 0);

            return result;
        }

        /// <summary>
        /// Convert BitArray to binary digit string.
        /// </summary>
        /// <param name="source">The source BitArray.</param>
        /// <returns>Binary digit string.</returns>
        public static string ToBitString(this BitArray source)
        {
            StringBuilder stringBuilder = new StringBuilder(source.Length);

            foreach (bool item in source)
            {
                stringBuilder.Append(item ? "1" : "0");
            }

            string result = stringBuilder.ToString();

            stringBuilder.Length = 0;

            return result;
        }

        /// <summary>
        /// Convert byte array to BitArray.
        /// </summary>
        /// <param name="source">The source byte array.</param>
        /// <returns>BitArray instance.</returns>
        public static BitArray ToBitArray(this byte[] source)
        {
            BitArray result = new BitArray(source);

            return result;
        }

        /// <summary>
        /// Convert byte to BitArray.
        /// </summary>
        /// <param name="source">The source byte.</param>
        /// <returns>BitArray instance.</returns>
        public static BitArray ToBitArray(this byte source)
        {
            BitArray result = new BitArray(new byte[] { source });

            return result;
        }

        /// <summary>
        /// Convert Int32 to BitArray.
        /// </summary>
        /// <param name="source">The source Int32.</param>
        /// <returns>BitArray instance.</returns>
        public static BitArray ToBitArray(this int source)
        {
            BitArray result = new BitArray(new int[] { source });

            return result;
        }

        /// <summary>
        /// Convert bit string to BitArray.
        /// </summary>
        /// <param name="source">The source bit string.</param>
        /// <returns>BitArray instance.</returns>
        public static BitArray BitStringToBitArray(this string source)
        {
            bool[] values = new bool[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                values[i] = source[i] != '0';
            }

            BitArray result = new BitArray(values);

            return result;
        }

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
        /// Convert Hex string to byte array.
        /// </summary>
        /// <param name="source">Hex string.</param>
        /// <returns>Byte array.</returns>
        public static byte[] HexToByteArray(this string source)
        {
            string temp = source.RemoveAny(false, ' ', '-', '\n', '\r');

            if (temp.Length % 2 == 1)
            {
                temp = "0" + temp;
            }

            byte[] result = new byte[temp.Length / 2];

            char tempChar;

            for (int bx = 0, sx = 0; bx < result.Length; ++bx, ++sx)
            {
                tempChar = temp[sx];
                result[bx] = (byte)((tempChar > '9' ? (tempChar > 'Z' ? (tempChar - 'a' + 10) : (tempChar - 'A' + 10)) : (tempChar - '0')) << 4);

                tempChar = temp[++sx];
                result[bx] |= (byte)(tempChar > '9' ? (tempChar > 'Z' ? (tempChar - 'a' + 10) : (tempChar - 'A' + 10)) : (tempChar - '0'));
            }

            return result;
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
        /// Convert Image to bytes.
        /// </summary>
        /// <param name="source">Image to convert.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ImageToByteArray(this Image source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            ImageConverter imageConverter = new ImageConverter();

            return (byte[])imageConverter.ConvertTo(source, typeof(byte[]));
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
