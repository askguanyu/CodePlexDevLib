//-----------------------------------------------------------------------
// <copyright file="IOExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// IO Extensions.
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// </summary>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file successfully.</returns>
        public static string WriteTextFile(this string contents, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            if (!overwritten && fullName.ExistsFile())
            {
                throw new ArgumentException("The file exists.", fullName);
            }

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    throw;
                }
            }

            try
            {
                File.WriteAllText(fullName, contents);
                return fullName;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file successfully.</returns>
        public static string WriteTextFile(this string contents, string fileName, Encoding encoding, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            if (!overwritten && fullName.ExistsFile())
            {
                throw new ArgumentException("The file exists.", fullName);
            }

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    throw;
                }
            }

            try
            {
                File.WriteAllText(fullName, contents, encoding);
                return fullName;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="fileName">The file to open for reading.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadTextFile(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
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
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullName);
            }
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fileName">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadTextFile(this string fileName, Encoding encoding)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
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
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullName);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// </summary>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file successfully.</returns>
        public static string WriteBinaryFile(this byte[] bytes, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (bytes == null)
            {
                throw new ArgumentNullException("binary");
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            if (!overwritten && fullName.ExistsFile())
            {
                throw new ArgumentException("The file exists.", fullName);
            }

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    throw;
                }
            }

            try
            {
                File.WriteAllBytes(fullName, bytes);
                return fullName;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="fileName">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadBinaryFile(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullName = Path.GetFullPath(fileName);

            if (File.Exists(fullName))
            {
                try
                {
                    return File.ReadAllBytes(fullName);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullName);
            }
        }

        /// <summary>
        /// Open containing folder with Windows Explorer.
        /// </summary>
        /// <param name="fileName">Path or File name.</param>
        /// <returns>Full path or the file name if open folder successfully.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string OpenContainingFolder(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", fullPath);
                return fullName;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// </summary>
        /// <param name="source">The path of a file or directory.</param>
        /// <returns>
        /// Directory information for path, or null if path denotes a root directory or is null.
        /// Returns System.String.Empty if path does not contain directory information.
        /// </returns>
        public static string GetDirectoryName(this string source)
        {
            try
            {
                return Path.GetDirectoryName(source);
            }
            catch
            {
                return Path.GetDirectoryName(source.Remove("\""));
            }
        }

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="source">The file or directory for which to obtain absolute path information.</param>
        /// <returns>A string containing the fully qualified location of path, such as "C:\MyFile.txt".</returns>
        public static string GetFullPath(this string source)
        {
            try
            {
                return Path.GetFullPath(source);
            }
            catch
            {
                return Path.GetFullPath(source.Remove("\""));
            }
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="source">The file to check.</param>
        /// <returns>
        /// true if the caller has the required permissions and path contains the name of an existing file; otherwise, false.
        /// This method also returns false if path is null, an invalid path, or a zero-length string.
        /// If the caller does not have sufficient permissions to read the specified file,
        /// no exception is thrown and the method returns false regardless of the existence of path.
        /// </returns>
        public static bool ExistsFile(this string source)
        {
            return File.Exists(source);
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="source">The path to test.</param>
        /// <returns>true if path refers to an existing directory; otherwise, false.</returns>
        public static bool ExistsDirectory(this string source)
        {
            return Directory.Exists(source);
        }
    }
}
