//-----------------------------------------------------------------------
// <copyright file="IOExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.IO;
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
        public static string WriteTextFile(this string text, string fileName, bool overwritten = true)
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

            FileStream fileStream = null;
            bool result = false;

            try
            {
                fileStream = new FileStream(fullName, overwritten ? FileMode.Create : FileMode.CreateNew);

                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    fileStream = null;
                    streamWriter.Write(text);
                    streamWriter.Flush();
                }

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
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
        /// Creates a new binary file from an byte array
        /// </summary>
        /// <param name="binary">Binary to write to the file</param>
        /// <param name="fileName">Full path of the file</param>
        /// <param name="overwritten">Whether overwrite exists file</param>
        /// <returns>Full path of the file name if write file successfully, string.Empty if failed</returns>
        public static string WriteBinaryFile(this byte[] binary, string fileName, bool overwritten = true)
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

            FileStream fileStream = null;
            bool result = false;

            try
            {
                fileStream = new FileStream(fullName, overwritten ? FileMode.Create : FileMode.CreateNew);
                fileStream.Write(binary, 0, binary.Length);
                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }

            return result ? fullName : string.Empty;
        }

        /// <summary>
        /// Read binary file to byte array
        /// </summary>
        /// <param name="fileName">Binary file to be read</param>
        /// <returns>Byte array</returns>
        public static byte[] ReadBinaryFile(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            string fullName = Path.GetFullPath(fileName);

            FileStream fileStream = null;
            byte[] result = null;
            MemoryStream outputStream = null;

            if (File.Exists(fullName))
            {
                try
                {
                    outputStream = new MemoryStream();
                    fileStream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fileStream.CopyTo(outputStream);
                    result = outputStream.ToArray();
                }
                catch
                {
                    result = null;
                }
                finally
                {
                    if (outputStream != null)
                    {
                        outputStream.Close();
                    }

                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }
            }
            else
            {
                result = null;
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
        /// <param name="source">The path of a file or directory</param>
        /// <returns>A System.String containing directory information for path, or null if path
        /// denotes a root directory, is the empty string (""), or is null. Returns System.String.Empty
        /// if path does not contain directory information</returns>
        public static string GetDirectoryName(this string source)
        {
            return Path.GetDirectoryName(source);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string
        /// </summary>
        /// <param name="source">The file or directory for which to obtain absolute path information</param>
        /// <returns>A string containing the fully qualified location of path, such as "C:\MyFile.txt"</returns>
        public static string GetFullPath(this string source)
        {
            return Path.GetFullPath(source);
        }

        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="source">The file to check</param>
        /// <returns>true if the caller has the required permissions and path contains the name
        /// of an existing file; otherwise, false. This method also returns false if
        /// path is null, an invalid path, or a zero-length string. If the caller does
        /// not have sufficient permissions to read the specified file, no exception
        /// is thrown and the method returns false regardless of the existence of path.</returns>
        public static bool ExistsFile(this string source)
        {
            return File.Exists(source);
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk
        /// </summary>
        /// <param name="source">The path to test</param>
        /// <returns>true if path refers to an existing directory; otherwise, false</returns>
        public static bool ExistsDirectory(this string source)
        {
            return Directory.Exists(source);
        }
    }
}
