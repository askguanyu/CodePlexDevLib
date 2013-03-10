//-----------------------------------------------------------------------
// <copyright file="FtpFileInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.IO;
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
        /// Combines two strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string CombinePath(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path2))
            {
                return Path.AltDirectorySeparatorChar + path1.Trim(Path.AltDirectorySeparatorChar);
            }

            if (string.IsNullOrEmpty(path1))
            {
                return Path.AltDirectorySeparatorChar + path2.Trim(Path.AltDirectorySeparatorChar);
            }

            return Path.AltDirectorySeparatorChar + path1.Trim(Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar + path2.Trim(Path.AltDirectorySeparatorChar);
        }
    }
}
