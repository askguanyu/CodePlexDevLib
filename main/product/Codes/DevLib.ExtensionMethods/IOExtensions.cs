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
    using System.Linq;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// IO Extensions.
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Renames the file to "* (n).*" if file already exists.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>New file name "* (n).*" if file already exists; otherwise, the original file name.</returns>
        public static string RenameFile(this string filename)
        {
            if (!File.Exists(filename))
            {
                return filename;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
            string fileExtension = Path.GetExtension(filename);
            string directoryName = Path.GetDirectoryName(Path.GetFullPath(filename));

            string pattern = "^" + fileNameWithoutExtension + "\\s*(\\d+)?";
            Regex regex = new Regex(pattern);
            int count = Directory.GetFiles(directoryName, fileNameWithoutExtension + "*" + fileExtension).Where(i => regex.IsMatch(Path.GetFileNameWithoutExtension(i))).Count();

            return fileNameWithoutExtension + " (" + (count + 1).ToString() + ")" + fileExtension;
        }

        /// <summary>
        /// Renames the folder to "* (n)" if folder already exists.
        /// </summary>
        /// <param name="path">The folder path.</param>
        /// <returns>New folder name "* (n)" if folder already exists; otherwise, the original folder name.</returns>
        public static string RenameFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return path;
            }

            string folderName = Path.GetFileName(path);
            string directoryName = Path.GetDirectoryName(Path.GetFullPath(path));

            string pattern = "^" + folderName + "\\s*(\\d+)?";
            Regex regex = new Regex(pattern);
            int count = Directory.GetDirectories(directoryName, folderName + "*").Where(i => regex.IsMatch(Path.GetFileName(i))).Count();

            return folderName + " (" + (count + 1).ToString() + ")";
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="filename">The file to write to.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteTextFile(this string contents, string filename, bool overwrite = false, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fullPath))
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

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

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="filename">The file to append the specified string to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string AppendTextFile(this string contents, string filename, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

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

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="filename">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadTextFile(this string filename, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            if (File.Exists(fullPath))
            {
                return encoding == null ? File.ReadAllText(fullPath) : File.ReadAllText(fullPath, encoding);
            }
            else
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="filename">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public static string[] ReadFileAllLines(this string filename, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            if (File.Exists(fullPath))
            {
                return encoding == null ? File.ReadAllLines(fullPath) : File.ReadAllLines(fullPath, encoding);
            }
            else
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// </summary>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <param name="filename">The file to write to.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteBinaryFile(this byte[] bytes, string filename, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fullPath))
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            File.WriteAllBytes(fullPath, bytes);

            return fullPath;
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="filename">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadBinaryFile(this string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            else
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }
        }

        /// <summary>
        /// Creates a new file, writes the specified stream to the file, and then closes the file.
        /// </summary>
        /// <param name="source">The stream to write to the file.</param>
        /// <param name="filename">The file to write to.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>Full path of the file name if write file succeeded.</returns>
        public static string WriteFile(this Stream source, string filename, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fullPath))
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
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
        /// <param name="filename">Path or File name.</param>
        /// <returns>Full path or the file name if open folder succeeded.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static string OpenContainingFolder(this string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            Process.Start("explorer.exe", fullDirectoryPath);

            return fullPath;
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
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <returns>Full path of the destination file name if move file succeeded.</returns>
        public static string MoveFileTo(this string sourceFileName, string destFileName, bool overwrite = false)
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

            string destFullPath = Path.GetFullPath(destFileName);

            string destFullDirectoryPath = Path.GetDirectoryName(destFileName);

            if (File.Exists(destFullPath))
            {
                if (overwrite)
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
                Directory.CreateDirectory(destFullDirectoryPath);
            }

            File.Move(sourceFullPath, destFullPath);

            return destFullPath;
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        /// <returns>Full path of the destination file name if copy file succeeded.</returns>
        public static string CopyFileTo(this string sourceFileName, string destFileName, bool overwrite = false)
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

            string destFullPath = Path.GetFullPath(destFileName);

            string destFullDirectoryPath = Path.GetDirectoryName(destFileName);

            if (!Directory.Exists(destFullDirectoryPath))
            {
                Directory.CreateDirectory(destFullDirectoryPath);
            }

            File.Copy(sourceFullPath, destFullPath, overwrite);

            return destFullPath;
        }

        /// <summary>
        /// Copies a directory to a new directory. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceDirectory">The directory to copy.</param>
        /// <param name="destDirectory">The name of the destination directory.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Full path of the destination directory if copy succeeded; otherwise, String.Empty.</returns>
        public static string CopyDirectoryTo(this string sourceDirectory, string destDirectory, bool overwrite = true, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("sourceFileName");
                }
                else
                {
                    return string.Empty;
                }
            }

            if (string.IsNullOrEmpty(destDirectory))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("destFileName");
                }
                else
                {
                    return string.Empty;
                }
            }

            string sourceFullPath = Path.GetFullPath(sourceDirectory);

            string destFullPath = Path.GetFullPath(destDirectory);

            if (sourceFullPath.Equals(destFullPath, StringComparison.OrdinalIgnoreCase))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("Source directory and destination directory are the same.");
                }
                else
                {
                    return string.Empty;
                }
            }

            if (!Directory.Exists(sourceDirectory))
            {
                if (throwOnError)
                {
                    throw new DirectoryNotFoundException(string.Format("{0} does not exist.", sourceFullPath));
                }
                else
                {
                    return string.Empty;
                }
            }

            try
            {
                string[] files = Directory.GetFiles(sourceFullPath, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    try
                    {
                        file.CopyFileTo(file.Replace(sourceFullPath, destFullPath), overwrite);
                    }
                    catch
                    {
                        if (throwOnError)
                        {
                            throw;
                        }
                    }
                }

                return destFullPath;
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Determines whether the given path is a directory or not.
        /// </summary>
        /// <param name="sourcePath">The path to test.</param>
        /// <returns>true if path is a directory; otherwise, false.</returns>
        public static bool IsDirectory(this string sourcePath)
        {
            FileInfo fileInfo = new FileInfo(sourcePath);
            return fileInfo.Attributes != (FileAttributes)(-1) && (fileInfo.Attributes & FileAttributes.Directory) != 0;
        }

        /// <summary>
        /// Determines whether the specified path is empty directory.
        /// </summary>
        /// <param name="sourcePath">The path to check.</param>
        /// <returns>true if the specified path is empty directory; otherwise, false.</returns>
        public static bool IsDirectoryEmpty(this string sourcePath)
        {
            string[] dirs = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);

            if (dirs.Length == 0)
            {
                string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                return files.Length == 0;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory or a file on disk.
        /// </summary>
        /// <param name="source">The path to test.</param>
        /// <returns>true if path refers to an existing directory or a file; otherwise, false.</returns>
        public static bool ExistsFileSystem(this string source)
        {
            FileInfo fileInfo = new FileInfo(source);
            bool isDirectory = fileInfo.Attributes != (FileAttributes)(-1) && (fileInfo.Attributes & FileAttributes.Directory) != 0;
            return isDirectory ? true : fileInfo.Exists;
        }

        /// <summary>
        /// Deletes an empty directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="source">The name of the directory to remove.</param>
        /// <param name="recursive">true to remove directories, subdirectories, and files in path; otherwise, false.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool DeleteDirectory(this string source, bool recursive)
        {
            DirectoryInfo directory = new DirectoryInfo(source);

            if (!directory.Exists)
            {
                return true;
            }

            foreach (var item in directory.GetFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                try
                {
                    item.Attributes = FileAttributes.Normal;
                }
                catch
                {
                }

                try
                {
                    item.Delete();
                }
                catch
                {
                }
            }

            foreach (var item in directory.GetDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                try
                {
                    item.Attributes = FileAttributes.Normal;
                }
                catch
                {
                }

                try
                {
                    item.Delete(recursive);
                }
                catch
                {
                }
            }

            try
            {
                directory.Attributes = FileAttributes.Normal;
            }
            catch
            {
            }

            try
            {
                directory.Delete(recursive);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="source">The name of the file to be deleted.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool DeleteFile(this string source)
        {
            FileInfo file = new FileInfo(source);

            if (!file.Exists)
            {
                return true;
            }

            try
            {
                file.Attributes = FileAttributes.Normal;
            }
            catch
            {
            }

            try
            {
                file.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Execute a command line.
        /// </summary>
        /// <param name="sourceCmd">A command line to execute.</param>
        /// <param name="runasAdmin">true to run as Administrator; false to run as current user.</param>
        /// <param name="hidden">true if want to hide window; otherwise, false.</param>
        /// <param name="milliseconds">
        /// The amount of time, in milliseconds, to wait for the command to exit.
        /// The maximum is the largest possible value of a 32-bit integer, which represents infinity to the operating system.
        /// Less than or equal to Zero if do not wait for the command to exit.
        /// </param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static void ExecuteCmdLine(this string sourceCmd, bool runasAdmin = true, bool hidden = true, int milliseconds = 0)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"));
            startInfo.Arguments = string.Format(" /C {0}", sourceCmd);
            startInfo.CreateNoWindow = hidden;
            startInfo.ErrorDialog = true;
            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;

            if (runasAdmin)
            {
                startInfo.Verb = "runas";
            }

            Process process = Process.Start(startInfo);

            if (milliseconds > 0)
            {
                if (process.WaitForExit(milliseconds))
                {
                    process.Dispose();
                }
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
        /// Reads the bytes from the current stream and writes them to another stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        public static void CopyStreamTo(this Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            byte[] buffer = new byte[81920];

            int count;

            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }

        /// <summary>
        /// Reads the bytes from the current stream and writes them to another stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="startPosition">The zero-based starting position of the source stream to be copied.</param>
        /// <param name="length">Length of the source stream to be copied.</param>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        public static void CopyStreamTo(this Stream source, long startPosition, int length, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            source.Position = startPosition;

            byte[] buffer = new byte[81920];

            int count;

            int rest = length;

            while (rest > 0)
            {
                if (rest <= buffer.Length)
                {
                    count = source.Read(buffer, 0, rest);

                    if (count > 0)
                    {
                        destination.Write(buffer, 0, count);
                    }

                    break;
                }
                else
                {
                    count = source.Read(buffer, 0, buffer.Length);

                    if (count > 0)
                    {
                        destination.Write(buffer, 0, count);

                        if (count == buffer.Length)
                        {
                            rest -= buffer.Length;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Formats the long length of a file to a more friendly string, e.g. "1.23 GB", "456 KB", etc.,
        /// </summary>
        /// <param name="fileSize">The file size for which to determine the format.</param>
        /// <returns>The resulting string.</returns>
        public static string ToFileSizeFriendlyString(this double fileSize)
        {
            if (fileSize < 0D)
            {
                return fileSize.ToString();
            }

            if (fileSize >= 1099511627776D)
            {
                return (fileSize / 1099511627776D).ToString("########0.00") + " TB";
            }
            else if (fileSize >= 1073741824D)
            {
                return (fileSize / 1073741824D).ToString("########0.00") + " GB";
            }
            else if (fileSize >= 1048576D)
            {
                return (fileSize / 1048576D).ToString("####0.00") + " MB";
            }
            else if (fileSize >= 1024D)
            {
                return (fileSize / 1024D).ToString("####0.00") + " KB";
            }
            else
            {
                return fileSize.ToString("####0") + " bytes";
            }
        }

        /// <summary>
        /// Formats the long length of a file to a more friendly string, e.g. "1.23 GB", "456 KB", etc.,
        /// </summary>
        /// <param name="fileSize">The file size for which to determine the format.</param>
        /// <returns>The resulting string.</returns>
        public static string ToFileSizeFriendlyString(this float fileSize)
        {
            return ((double)fileSize).ToFileSizeFriendlyString();
        }

        /// <summary>
        /// Formats the long length of a file to a more friendly string, e.g. "1.23 GB", "456 KB", etc.,
        /// </summary>
        /// <param name="fileSize">The file size for which to determine the format.</param>
        /// <returns>The resulting string.</returns>
        public static string ToFileSizeFriendlyString(this long fileSize)
        {
            return ((double)fileSize).ToFileSizeFriendlyString();
        }

        /// <summary>
        /// Formats the long length of a file to a more friendly string, e.g. "1.23 GB", "456 KB", etc.,
        /// </summary>
        /// <param name="fileSize">The file size for which to determine the format.</param>
        /// <returns>The resulting string.</returns>
        public static string ToFileSizeFriendlyString(this int fileSize)
        {
            return ((double)fileSize).ToFileSizeFriendlyString();
        }
    }
}
