//-----------------------------------------------------------------------
// <copyright file="IOExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// IO Extensions.
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteTextFile(this string contents, string fileName, bool overwritten = true, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwritten && fullPath.ExistsFile())
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

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

            try
            {
                if (encoding == null)
                {
                    File.WriteAllText(fullPath, contents);
                }
                else
                {
                    File.WriteAllText(fullPath, contents, encoding);
                }

                return fullPath;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="fileName">The file to append the specified string to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string AppendTextFile(this string contents, string fileName, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
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

            try
            {
                if (encoding == null)
                {
                    File.AppendAllText(fullPath, contents);
                }
                else
                {
                    File.AppendAllText(fullPath, contents, encoding);
                }

                return fullPath;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fileName">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadTextFile(this string fileName, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    return encoding == null ? File.ReadAllText(fullPath) : File.ReadAllText(fullPath, encoding);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullPath);
            }
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="fileName">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public static string[] ReadFileAllLines(this string fileName, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    return encoding == null ? File.ReadAllLines(fullPath) : File.ReadAllLines(fullPath, encoding);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullPath);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// </summary>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteBinaryFile(this byte[] bytes, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwritten && fullPath.ExistsFile())
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

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

            try
            {
                File.WriteAllBytes(fullPath, bytes);
                return fullPath;
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

            string fullPath = Path.GetFullPath(fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    return File.ReadAllBytes(fullPath);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(fullPath);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified stream to the file, and then closes the file.
        /// </summary>
        /// <param name="source">The stream to write to the file.</param>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteFile(this Stream source, string fileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwritten && fullPath.ExistsFile())
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

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

            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[81920];
                int count;
                while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fileStream.Write(buffer, 0, count);
                }

                return fullPath;
            }
        }

        /// <summary>
        /// Open containing folder with Windows Explorer.
        /// </summary>
        /// <param name="fileName">Path or File name.</param>
        /// <returns>Full path or the file name if open folder succeeded.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static string OpenContainingFolder(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string fullPath = Path.GetFullPath(fileName);
            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            try
            {
                Process.Start("explorer.exe", fullDirectoryPath);
                return fullPath;
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
            return Path.GetDirectoryName(source);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="source">The file or directory for which to obtain absolute path information.</param>
        /// <returns>A string containing the fully qualified location of path, such as "C:\MyFile.txt".</returns>
        public static string GetFullPath(this string source)
        {
            return Path.GetFullPath(source);
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

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move.</param>
        /// <param name="destFileName">The new path for the file.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <returns>Full path of the destination file name if move file succeeded.</returns>
        public static string MoveTo(this string sourceFileName, string destFileName, bool overwritten = true)
        {
            if (string.IsNullOrEmpty(sourceFileName))
            {
                throw new ArgumentNullException("sourceFileName");
            }

            if (string.IsNullOrEmpty(destFileName))
            {
                throw new ArgumentNullException("destFileName");
            }

            string sourceFullPath = Path.GetFullPath(sourceFileName);
            string sourceFullDirectoryPath = Path.GetDirectoryName(sourceFullPath);

            string destFullPath = Path.GetFullPath(destFileName);
            string destFullDirectoryPath = Path.GetDirectoryName(destFileName);

            if (destFullPath.ExistsFile())
            {
                if (overwritten)
                {
                    File.Delete(destFullPath);
                }
                else
                {
                    throw new ArgumentException("The specified file already exists.", sourceFullPath);
                }
            }

            if (!Directory.Exists(destFullDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(destFullDirectoryPath);
                }
                catch
                {
                    throw;
                }
            }

            try
            {
                File.Move(sourceFullPath, destFullPath);
                return destFullPath;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Stream object to bytes.
        /// </summary>
        /// <param name="source">Stream source.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ToByteArray(this Stream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source is MemoryStream)
            {
                return ((MemoryStream)source).ToArray();
            }
            else
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[81920];
                    int count;
                    while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memoryStream.Write(buffer, 0, count);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Bytes to Stream object.
        /// </summary>
        /// <param name="source">Bytes source.</param>
        /// <returns>Stream object.</returns>
        public static Stream ToStream(this byte[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new MemoryStream(source);
        }

        /// <summary>
        /// Copy source stream to destination stream.
        /// </summary>
        /// <param name="source">Source stream.</param>
        /// <param name="destinationStream">Destination stream.</param>
        /// <returns>This instance.</returns>
        public static Stream CopyTo(this Stream source, Stream destinationStream)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }

            byte[] buffer = new byte[81920];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destinationStream.Write(buffer, 0, count);
            }

            return source;
        }
    }
}
