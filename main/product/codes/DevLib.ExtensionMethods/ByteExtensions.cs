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
        /// Compresses byte array using gzip compression
        /// </summary>
        /// <param name="source">Byte array to compress</param>
        /// <returns>A compressed byte array</returns>
        public static byte[] Compress(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            MemoryStream memoryStream = null;
            byte[] result = null;

            try
            {
                memoryStream = new MemoryStream();
                GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress);
                zipStream.Write(source, 0, source.Length);
                memoryStream.Position = 0;

                byte[] compressed = new byte[memoryStream.Length];
                memoryStream.Read(compressed, 0, compressed.Length);

                result = new byte[compressed.Length + 4];
                Buffer.BlockCopy(compressed, 0, result, 4, compressed.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(source.Length), 0, result, 0, 4);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Decompresses byte array using gzip compression
        /// </summary>
        /// <param name="source">Byte array to decompress</param>
        /// <returns>A decompressed byte array</returns>
        public static byte[] Decompress(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            MemoryStream memoryStream = null;
            byte[] result = null;

            try
            {
                memoryStream = new MemoryStream();
                int resultLength = BitConverter.ToInt32(source, 0);
                memoryStream.Write(source, 4, source.Length - 4);

                result = new byte[resultLength];

                memoryStream.Position = 0;
                GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                zipStream.Read(result, 0, result.Length);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
            }

            return result;
        }
    }
}
