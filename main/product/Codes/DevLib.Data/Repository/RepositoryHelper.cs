//-----------------------------------------------------------------------
// <copyright file="RepositoryHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Encapsulates a method that returns the primary key of an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the entity primary key.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity primary key.</returns>
    public delegate TPrimaryKey GetPrimaryKeyFunc<in TEntity, out TPrimaryKey>(TEntity entity);

    /// <summary>
    /// Encapsulates a method that reads a file and returns the repository.
    /// </summary>
    /// <typeparam name="TString">String type.</typeparam>
    /// <typeparam name="T">The type of the repository.</typeparam>
    /// <param name="filename">The filename.</param>
    /// <returns>The repository instance.</returns>
    public delegate T ReadFileFunc<in TString, out T>(TString filename);

    /// <summary>
    /// Encapsulates a method that writes the repository to a file.
    /// </summary>
    /// <typeparam name="TString">String type.</typeparam>
    /// <typeparam name="TObject">The repository type.</typeparam>
    /// <param name="filename">The filename.</param>
    /// <param name="source">The repository source.</param>
    public delegate void WriteFileAction<TString, TObject>(TString filename, TObject source);

    /// <summary>
    /// Repository helper.
    /// </summary>
    internal static class RepositoryHelper
    {
        /// <summary>
        /// Field SharedMutexFileNamePrefix.
        /// </summary>
        private const string SharedMutexFileNamePrefix = @"Global\DevLibDataRepositoryRepositoryHelper_";

        /// <summary>
        /// Create a global shared mutex.
        /// </summary>
        /// <param name="mutexName">The mutex name.</param>
        /// <returns>Instance of Mutex.</returns>
        public static Mutex CreateSharedMutex(string mutexName)
        {
            try
            {
                MutexSecurity mutexSecurity = new MutexSecurity();

                mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));

                bool createdNew;

                return new Mutex(false, mutexName, out createdNew, mutexSecurity);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a global shared mutex name according to file name.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Mutex name.</returns>
        public static string GetSharedFileMutexName(string filename)
        {
            byte[] hash;

            using (MD5 hasher = MD5.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(filename).ToLowerInvariant()));
            }

            return SharedMutexFileNamePrefix + BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        /// <summary>
        /// Determines whether the specified enumerable instance is null or empty.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The System.Collections.Generic.IEnumerable{T} to check for emptiness.</param>
        /// <returns>true if the instance is null or empty; otherwise, false.</returns>
        public static bool IsEnumerableNullOrEmpty<TSource>(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return true;
            }

            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns an System.Int64 that represents the total number of elements in a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable{T} that contains the elements to be counted.</param>
        /// <returns>The number of elements in the source sequence.</returns>
        public static long LongCount<TSource>(IEnumerable<TSource> source)
        {
            long count = 0;

            if (source == null)
            {
                return count;
            }

            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                checked
                {
                    while (e.MoveNext())
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Deserializes Xml string to object, read from file.
        /// </summary>
        /// <typeparam name="T">Type of the returns object.</typeparam>
        /// <param name="filename">File name.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadXml<T>(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(filename);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (FileStream fileStream = File.OpenRead(fullPath))
            {
                return (T)xmlSerializer.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Serializes object to Xml string, write to file.
        /// </summary>
        /// <remarks>
        /// The object to be serialized should be decorated with the <see cref="SerializableAttribute"/>, or implement the <see cref="ISerializable"/> interface.
        /// </remarks>
        /// <param name="filename">File name.</param>
        /// <param name="source">The object to serialize.</param>
        public static void WriteXml(string filename, object source)
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

            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

            using (XmlWriter xmlWriter = XmlWriter.Create(fullPath, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, Encoding = new UTF8Encoding(false), CloseOutput = true }))
            {
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(xmlWriter, source, xmlns);
                xmlWriter.Flush();
            }
        }

        /// <summary>
        /// Deserializes bytes to object, read from file.
        /// </summary>
        /// <typeparam name="T">The type of returns object.</typeparam>
        /// <param name="filename">File name.</param>
        /// <returns>Instance of T.</returns>
        public static T ReadBinary<T>(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(filename);

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

        /// <summary>
        /// Serializes object to bytes, write to file.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="source">Source object.</param>
        public static void WriteBinary(string filename, object source)
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
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneDeep<T>(T source)
        {
            if (source == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
