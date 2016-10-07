//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;

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
        /// Serializes object to bytes.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <returns>Byte array.</returns>
        public static byte[] Serialize(object source)
        {
            if (source == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes bytes to object.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <returns>Instance object.</returns>
        public static object Deserialize(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes bytes to object.
        /// </summary>
        /// <typeparam name="T">The type of returns object.</typeparam>
        /// <param name="source">Byte array.</param>
        /// <returns>Instance of T.</returns>
        public static T Deserialize<T>(byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                return (T)binaryFormatter.Deserialize(memoryStream);
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
