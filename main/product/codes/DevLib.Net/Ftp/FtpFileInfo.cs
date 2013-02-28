﻿//-----------------------------------------------------------------------
// <copyright file="FtpFileInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;

    /// <summary>
    /// FtpFileInfo Class.
    /// </summary>
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
    }
}
