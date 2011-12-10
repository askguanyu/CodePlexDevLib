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
    using System.Security.Permissions;

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

        /// <summary>
        /// Open containing folder with Windows Explorer
        /// </summary>
        /// <param name="fileName">Path or File name</param>
        /// <returns>True if open successfully</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static bool OpenContainingFolder(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the directory information for the specified path string
        /// </summary>
        /// <param name="value">The path of a file or directory</param>
        /// <returns>A System.String containing directory information for path, or null if path
        /// denotes a root directory, is the empty string (""), or is null. Returns System.String.Empty
        /// if path does not contain directory information</returns>
        public static string GetDirectoryName(this string value)
        {
            return Path.GetDirectoryName(value);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string
        /// </summary>
        /// <param name="value">The file or directory for which to obtain absolute path information</param>
        /// <returns>A string containing the fully qualified location of path, such as "C:\MyFile.txt"</returns>
        public static string GetFullPath(this string value)
        {
            return Path.GetFullPath(value);
        }

        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="value">The file to check</param>
        /// <returns>true if the caller has the required permissions and path contains the name
        /// of an existing file; otherwise, false. This method also returns false if
        /// path is null, an invalid path, or a zero-length string. If the caller does
        /// not have sufficient permissions to read the specified file, no exception
        /// is thrown and the method returns false regardless of the existence of path.</returns>
        public static bool ExistsFile(this string value)
        {
            return File.Exists(value);
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk
        /// </summary>
        /// <param name="value">The path to test</param>
        /// <returns>true if path refers to an existing directory; otherwise, false</returns>
        public static bool ExistsDirectory(this string value)
        {
            return Directory.Exists(value);
        }
    }
}
