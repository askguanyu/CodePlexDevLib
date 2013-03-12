//-----------------------------------------------------------------------
// <copyright file="FtpFileInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Class FtpFileInfo.
    /// </summary>
    [Serializable]
    public class FtpFileInfo
    {
        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets file parent directory.
        /// </summary>
        public string ParentDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets file full path.
        /// </summary>
        [XmlIgnore]
        public string FullPath
        {
            get
            {
                return FtpFileInfo.CombinePath(this.ParentDirectory, this.Name);
            }
        }

        /// <summary>
        /// Gets or sets file size in byte.
        /// </summary>
        public long Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a folder or not.
        /// </summary>
        public bool IsDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets file last modified time.
        /// </summary>
        public DateTime LastModifiedTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unix file flags.
        /// </summary>
        public string Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unix file owner.
        /// </summary>
        public string Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unix file group.
        /// </summary>
        public string Group
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether unix file is symlink.
        /// </summary>
        public bool IsSymLink
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets unix file symlink target path.
        /// </summary>
        public string SymLinkTargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Combines an array of strings into a ftp path.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <returns>The combined paths.</returns>
        public static string CombinePath(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            for (int i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                {
                    paths[i] = paths[i].Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    CheckInvalidPathChars(paths[i], false);
                }
                else
                {
                    paths[i] = string.Empty;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(Path.AltDirectorySeparatorChar);

            for (int j = 0; j < paths.Length; j++)
            {
                if (paths[j].Length != 0)
                {
                    char rearChar = stringBuilder[stringBuilder.Length - 1];

                    if (rearChar != Path.DirectorySeparatorChar && rearChar != Path.AltDirectorySeparatorChar && rearChar != Path.VolumeSeparatorChar)
                    {
                        stringBuilder.Append(Path.AltDirectorySeparatorChar);
                    }

                    stringBuilder.Append(paths[j]);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>Directory information for <paramref name="path" />, or null if <paramref name="path" /> denotes a root directory or is null. Returns <see cref="F:System.String.Empty" /> if <paramref name="path" /> does not contain directory information.</returns>
        public static string GetDirectoryName(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path, false);

                int rootLength = GetRootLength(path);
                int num = path.Length;

                if (num > rootLength)
                {
                    num = path.Length;

                    if (num == rootLength)
                    {
                        return null;
                    }

                    while (num > rootLength && path[--num] != Path.DirectorySeparatorChar && path[num] != Path.AltDirectorySeparatorChar)
                    {
                    }

                    return path.Substring(0, num);
                }
            }

            return null;
        }

        /// <summary>
        /// Method GetRootLength.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The length of root.</returns>
        private static int GetRootLength(string path)
        {
            CheckInvalidPathChars(path, false);

            int i = 0;
            int length = path.Length;

            if (length >= 1 && IsDirectorySeparator(path[0]))
            {
                i = 1;

                if (length >= 2 && IsDirectorySeparator(path[1]))
                {
                    i = 2;
                    int num = 2;

                    while (i < length)
                    {
                        if ((path[i] == Path.DirectorySeparatorChar || path[i] == Path.AltDirectorySeparatorChar) && --num <= 0)
                        {
                            break;
                        }

                        i++;
                    }
                }
            }
            else
            {
                if (length >= 2 && path[1] == Path.VolumeSeparatorChar)
                {
                    i = 2;

                    if (length >= 3 && IsDirectorySeparator(path[2]))
                    {
                        i++;
                    }
                }
            }

            return i;
        }

        /// <summary>
        /// Method IsDirectorySeparator.
        /// </summary>
        /// <param name="c">Char to check.</param>
        /// <returns>true if char is directory separator; otherwise, false.</returns>
        private static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Static Method CheckInvalidPathChars.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <param name="checkAdditional">Whether with additional check.</param>
        private static void CheckInvalidPathChars(string path, bool checkAdditional = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (HasIllegalCharacters(path, checkAdditional))
            {
                throw new ArgumentException("Illegal characters in path.");
            }
        }

        /// <summary>
        /// Static Method HasIllegalCharacters.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <param name="checkAdditional">Whether with additional check.</param>
        /// <returns>true if the path has illegal characters; otherwise, false.</returns>
        private static bool HasIllegalCharacters(string path, bool checkAdditional)
        {
            for (int i = 0; i < path.Length; i++)
            {
                int num = (int)path[i];

                if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
                {
                    return true;
                }

                if (checkAdditional && (num == 63 || num == 42))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
