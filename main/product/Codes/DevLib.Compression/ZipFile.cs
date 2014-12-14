//-----------------------------------------------------------------------
// <copyright file="ZipFile.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides static methods for creating, extracting, and opening zip archives.
    /// </summary>
    public static class ZipFile
    {
        /// <summary>
        /// Opens a zip archive for reading at the specified path.
        /// </summary>
        /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <returns>The opened zip archive.</returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="archiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="archiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="archiveFileName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="archiveFileName"/> could not be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="archiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName"/>.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="archiveFileName"/> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="archiveFileName"/> contains an invalid format.</exception>
        /// <exception cref="T:System.IO.InvalidDataException"><paramref name="archiveFileName"/> could not be interpreted as a zip archive.</exception>
        public static ZipArchive OpenRead(string archiveFileName)
        {
            return ZipFile.Open(archiveFileName, ZipArchiveMode.Read);
        }

        /// <summary>
        /// Opens a zip archive at the specified path and in the specified mode.
        /// </summary>
        /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="mode">One of the enumeration values that specifies the actions which are allowed on the entries in the opened archive.</param>
        /// <returns>The opened zip archive.</returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="archiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="archiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="archiveFileName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="archiveFileName"/> could not be opened.-or-<paramref name="mode"/> is set to <see cref="ZipArchiveMode.Create" />, but the file specified in <paramref name="archiveFileName"/> already exists.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="archiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName"/>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode"/> specifies an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException"><paramref name="mode"/> is set to <see cref="ZipArchiveMode.Read" />, but the file specified in <paramref name="archiveFileName"/> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="archiveFileName"/> contains an invalid format.</exception>
        /// <exception cref="T:System.IO.InvalidDataException"><paramref name="archiveFileName"/> could not be interpreted as a zip archive.-or-<paramref name="mode"/> is <see cref="ZipArchiveMode.Update" />, but an entry is missing or corrupt and cannot be read.-or-<paramref name="mode"/> is <see cref="ZipArchiveMode.Update" />, but an entry is too large to fit into memory.</exception>
        public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode)
        {
            return ZipFile.Open(archiveFileName, mode, null);
        }

        /// <summary>
        /// Opens a zip archive at the specified path, in the specified mode, and using the specified character encoding for entry names.
        /// </summary>
        /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="mode">One of the enumeration values that specifies the actions which are allowed on the entries in the opened archive.</param>
        /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when required for interoperability with ZIP archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
        /// <returns>The opened zip archive.</returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="archiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding"/> is set to a Unicode encoding other than UTF-8.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="archiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="archiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="archiveFileName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="archiveFileName"/> could not be opened.-or-<paramref name="mode"/> is set to <see cref="ZipArchiveMode.Create" />, but the file specified in <paramref name="archiveFileName"/> already exists.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="archiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the file specified in <paramref name="archiveFileName"/>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="mode"/> specifies an invalid value.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException"><paramref name="mode"/> is set to <see cref="ZipArchiveMode.Read" />, but the file specified in <paramref name="archiveFileName"/> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="archiveFileName"/> contains an invalid format.</exception>
        /// <exception cref="T:System.IO.InvalidDataException"><paramref name="archiveFileName"/> could not be interpreted as a zip archive.-or-<paramref name="mode"/> is <see cref="ZipArchiveMode.Update" />, but an entry is missing or corrupt and cannot be read.-or-<paramref name="mode"/> is <see cref="ZipArchiveMode.Update" />, but an entry is too large to fit into memory.</exception>
        public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode, Encoding entryNameEncoding)
        {
            FileMode mode2;
            FileAccess access;
            FileShare share;

            switch (mode)
            {
                case ZipArchiveMode.Read:
                    mode2 = FileMode.Open;
                    access = FileAccess.Read;
                    share = FileShare.Read;
                    break;

                case ZipArchiveMode.Create:
                    mode2 = FileMode.CreateNew;
                    access = FileAccess.Write;
                    share = FileShare.None;
                    break;

                case ZipArchiveMode.Update:
                    mode2 = FileMode.OpenOrCreate;
                    access = FileAccess.ReadWrite;
                    share = FileShare.None;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }

            FileStream fileStream = null;
            ZipArchive result;

            try
            {
                fileStream = File.Open(archiveFileName, mode2, access, share);
                result = new ZipArchive(fileStream, mode, false, entryNameEncoding);
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }

                throw;
            }

            return result;
        }

        /// <summary>
        /// Creates a zip archive that contains the files and directories from the specified directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="sourceDirectoryName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="destinationArchiveFileName"/> already exists.-or-A file in the specified directory could not be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="destinationArchiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName"/> or the file specified in <paramref name="destinationArchiveFileName"/>.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> contains an invalid format.-or-The zip archive does not support writing.</exception>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            ZipFile.DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, false, true, null);
        }

        /// <summary>
        /// Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level, and optionally includes the base directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="includeBaseDirectory">true to include the directory name from <paramref name="sourceDirectoryName"/> at the root of the archive; false to include only the contents of the directory.</param>
        /// <param name="includeSubDirectories">true to include all subdirectories from <paramref name="sourceDirectoryName"/>; false to include only the contents of the top directory.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="sourceDirectoryName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="destinationArchiveFileName"/> already exists.-or-A file in the specified directory could not be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="destinationArchiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName"/> or the file specified in <paramref name="destinationArchiveFileName"/>.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> contains an invalid format.-or-The zip archive does not support writing.</exception>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory, bool includeSubDirectories)
        {
            ZipFile.DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, includeBaseDirectory, includeSubDirectories, null);
        }

        /// <summary>
        /// Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level and character encoding for entry names, and optionally includes the base directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="includeBaseDirectory">true to include the directory name from <paramref name="sourceDirectoryName"/> at the root of the archive; false to include only the contents of the directory.</param>
        /// <param name="includeSubDirectories">true to include all subdirectories from <paramref name="sourceDirectoryName"/>; false to include only the contents of the top directory.</param>
        /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when required for interoperability with ZIP archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding"/> is set to a Unicode encoding other than UTF-8.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/>, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException"><paramref name="sourceDirectoryName"/> is invalid or does not exist (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="destinationArchiveFileName"/> already exists.-or-A file in the specified directory could not be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="destinationArchiveFileName"/> specifies a directory.-or-The caller does not have the required permission to access the directory specified in <paramref name="sourceDirectoryName"/> or the file specified in <paramref name="destinationArchiveFileName"/>.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="sourceDirectoryName"/> or <paramref name="destinationArchiveFileName"/> contains an invalid format.-or-The zip archive does not support writing.</exception>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory, bool includeSubDirectories, Encoding entryNameEncoding)
        {
            ZipFile.DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, includeBaseDirectory, includeSubDirectories, entryNameEncoding);
        }

        /// <summary>
        /// Extracts all the files in the specified zip archive to a directory on the file system.
        /// </summary>
        /// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
        /// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="overwrite">true to overwrite an existing file that has the same name as the destination file; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path in <paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="destinationDirectoryName"/> already exists.-or-The name of an entry in the archive is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.-or-Extracting an archive entry would create a file that is outside the directory specified by <paramref name="destinationDirectoryName"/>. (For example, this might happen if the entry name contains parent directory accessors.) -or-An archive entry to extract has the same name as an entry that has already been extracted from the same archive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to access the archive or the destination directory.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> contains an invalid format.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException"><paramref name="sourceArchiveFileName"/> was not found.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The archive specified by <paramref name="sourceArchiveFileName"/> is not a valid zip archive.-or-An archive entry was not found or was corrupt.-or-An archive entry was compressed by using a compression method that is not supported.</exception>
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwrite, bool throwOnError = false)
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwrite, null, throwOnError);
        }

        /// <summary>
        /// Extracts all the files in the specified zip archive to a directory on the file system using the specified character encoding for entry names.
        /// </summary>
        /// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
        /// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="overwrite">true to overwrite an existing file that has the same name as the destination file; otherwise, false.</param>
        /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when required for interoperability with ZIP archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.-or-<paramref name="entryNameEncoding"/> is set to a Unicode encoding other than UTF-8.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path in <paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> exceeds the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="destinationDirectoryName"/> already exists.-or-The name of an entry in the archive is <see cref="F:System.String.Empty"/>, contains only white space, or contains at least one invalid character.-or-Extracting an archive entry would create a file that is outside the directory specified by <paramref name="destinationDirectoryName"/>. (For example, this might happen if the entry name contains parent directory accessors.) -or-An archive entry to extract has the same name as an entry that has already been extracted from the same archive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to access the archive or the destination directory.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="destinationDirectoryName"/> or <paramref name="sourceArchiveFileName"/> contains an invalid format.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException"><paramref name="sourceArchiveFileName"/> was not found.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The archive specified by <paramref name="sourceArchiveFileName"/> is not a valid zip archive.-or-An archive entry was not found or was corrupt.-or-An archive entry was compressed by using a compression method that is not supported.</exception>
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwrite, Encoding entryNameEncoding, bool throwOnError = false)
        {
            if (sourceArchiveFileName == null)
            {
                throw new ArgumentNullException("sourceArchiveFileName");
            }

            using (ZipArchive zipArchive = ZipFile.Open(sourceArchiveFileName, ZipArchiveMode.Read, entryNameEncoding))
            {
                zipArchive.ExtractToDirectory(destinationDirectoryName, overwrite, throwOnError);
            }
        }

        /// <summary>
        /// Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level and character encoding for entry names, and optionally includes the base directory.
        /// </summary>
        /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="includeBaseDirectory">true to include the directory name from <paramref name="sourceDirectoryName"/> at the root of the archive; false to include only the contents of the directory.</param>
        /// <param name="includeSubDirectories">true to include all subdirectories from <paramref name="sourceDirectoryName"/>; false to include only the contents of the top directory.</param>
        /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive. Specify a value for this parameter only when required for interoperability with ZIP archive tools and libraries that do not support UTF-8 encoding for entry names.</param>
        private static void DoCreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory, bool includeSubDirectories, Encoding entryNameEncoding)
        {
            if (sourceDirectoryName == null)
            {
                throw new ArgumentNullException("sourceDirectoryName");
            }

            if (destinationArchiveFileName == null)
            {
                throw new ArgumentNullException("destinationArchiveFileName");
            }

            sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);

            if (!Directory.Exists(sourceDirectoryName))
            {
                throw new DirectoryNotFoundException(sourceDirectoryName);
            }

            destinationArchiveFileName = Path.GetFullPath(destinationArchiveFileName);

            using (ZipArchive destination = ZipFile.Open(destinationArchiveFileName, ZipArchiveMode.Create, entryNameEncoding))
            {
                bool flag = true;

                DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirectoryName);

                string fullName = directoryInfo.FullName;

                if (includeBaseDirectory && directoryInfo.Parent != null)
                {
                    fullName = directoryInfo.Parent.FullName;
                }

                List<FileSystemInfo> list = new List<FileSystemInfo>();

                if (includeSubDirectories)
                {
                    list.AddRange(directoryInfo.GetDirectories("*", SearchOption.AllDirectories));
                    list.AddRange(directoryInfo.GetFiles("*", SearchOption.AllDirectories));
                }
                else
                {
                    list.AddRange(directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly));
                }

                foreach (FileSystemInfo fileSystemInfo in list)
                {
                    flag = false;
                    int length = fileSystemInfo.FullName.Length - fullName.Length;
                    string entryName = fileSystemInfo.FullName.Substring(fullName.Length, length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (fileSystemInfo is FileInfo)
                    {
                        destination.CreateEntryFromFile(fileSystemInfo.FullName, entryName);
                    }
                    else
                    {
                        DirectoryInfo possiblyEmptyDir = fileSystemInfo as DirectoryInfo;

                        if (possiblyEmptyDir != null && ZipHelper.IsDirEmpty(possiblyEmptyDir))
                        {
                            destination.CreateEntryFromDirectory(possiblyEmptyDir.FullName, entryName + Path.DirectorySeparatorChar);
                        }
                    }
                }

                if (!includeBaseDirectory || !flag)
                {
                    return;
                }

                destination.CreateEntryFromDirectory(directoryInfo.FullName, directoryInfo.Name + Path.DirectorySeparatorChar);
            }
        }
    }
}
