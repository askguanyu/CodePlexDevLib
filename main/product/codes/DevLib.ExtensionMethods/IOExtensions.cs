//-----------------------------------------------------------------------
// <copyright file="IOExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// IO Extensions
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Creates a new text file from a string, this method will overwrite exists file
        /// </summary>
        /// <param name="text">Text to write to the file</param>
        /// <param name="fileName">Full path of the file</param>
        /// <returns>Full path of the file name if write file successfully, string.Empty if failed</returns>
        public static string CreateTextFile(this string text, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    return string.Empty;
                }
            }

            Stream stream = null;
            bool result = false;

            try
            {
                stream = new FileStream(fullName, overwritten ? FileMode.Create : FileMode.CreateNew);

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.Write(text);
                    writer.Flush();
                }

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return result ? fullName : string.Empty;
        }

        /// <summary>
        /// Read text file to string
        /// </summary>
        /// <param name="fileName">Text file to be read</param>
        /// <returns>Text file string</returns>
        public static string ReadTextFile(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            string fullName = Path.GetFullPath(fileName);

            if (File.Exists(fullName))
            {
                try
                {
                    return File.ReadAllText(fullName);
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Creates a new binary file from an object, this method will overwrite exists file
        /// </summary>
        /// <param name="binary">Binary to write to the file</param>
        /// <param name="fileName">Full path of the file</param>
        /// <returns>Full path of the file name if write file successfully, string.Empty if failed</returns>
        public static string CreateBinaryFile(this object binary, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    return string.Empty;
                }
            }

            Stream stream = null;
            bool result = false;

            try
            {
                stream = new FileStream(fullName, overwritten ? FileMode.Create : FileMode.CreateNew);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, binary);

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return result ? fullName : string.Empty;
        }

        /// <summary>
        /// Read binary file to object
        /// </summary>
        /// <param name="fileName">Binary file to be read</param>
        /// <returns>Object</returns>
        public static T ReadBinaryFile<T>(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return default(T);
            }

            string fullName = Path.GetFullPath(fileName);
            Stream stream = null;
            T result = default(T);

            if (File.Exists(fullName))
            {
                try
                {
                    stream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    IFormatter formatter = new BinaryFormatter();
                    result = (T)formatter.Deserialize(stream);
                }
                catch
                {
                    result = default(T);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
            }
            else
            {
                result = default(T);
            }

            return result;
        }
    }
}
