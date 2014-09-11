//-----------------------------------------------------------------------
// <copyright file="ZipArchiveMode.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    /// <summary>
    /// Specifies values for interacting with zip archive entries.
    /// </summary>
    public enum ZipArchiveMode
    {
        /// <summary>
        /// Only reading archive entries is permitted.
        /// </summary>
        Read,

        /// <summary>
        /// Only creating new archive entries is permitted.
        /// </summary>
        Create,

        /// <summary>
        /// Both read and write operations are permitted for archive entries.
        /// </summary>
        Update,
    }
}
