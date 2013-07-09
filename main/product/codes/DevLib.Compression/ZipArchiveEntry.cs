//-----------------------------------------------------------------------
// <copyright file="ZipArchiveEntry.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Represents a compressed file within a zip archive.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Reviewed.")]
    public class ZipArchiveEntry
    {
        private const ushort DefaultVersionToExtract = (ushort)10;

        private readonly bool _originallyInArchive;

        private readonly int _diskNumberStart;

        private ZipArchive _archive;

        private ZipVersionNeededValues _versionToExtract;

        private ZipArchiveEntry.BitFlagValues _generalPurposeBitFlag;

        private ZipArchiveEntry.CompressionMethodValues _storedCompressionMethod;

        private DateTimeOffset _lastModified;

        private long _compressedSize;

        private long _uncompressedSize;

        private long _offsetOfLocalHeader;

        private long? _storedOffsetOfCompressedData;

        private uint _crc32;

        private byte[] _compressedBytes;

        private MemoryStream _storedUncompressedData;

        private bool _currentlyOpenForWrite;

        private bool _everOpenedForWrite;

        private Stream _outstandingWriteStream;

        private string _storedEntryName;

        private byte[] _storedEntryNameBytes;

        private List<ZipGenericExtraField> _cdUnknownExtraFields;

        private List<ZipGenericExtraField> _lhUnknownExtraFields;

        private byte[] _fileComment;

        internal ZipArchiveEntry(ZipArchive archive, ZipCentralDirectoryFileHeader cd)
        {
            this._archive = archive;
            this._originallyInArchive = true;
            this._diskNumberStart = cd.DiskNumberStart;
            this._versionToExtract = (ZipVersionNeededValues)cd.VersionNeededToExtract;
            this._generalPurposeBitFlag = (ZipArchiveEntry.BitFlagValues)cd.GeneralPurposeBitFlag;
            this.CompressionMethod = (ZipArchiveEntry.CompressionMethodValues)cd.CompressionMethod;
            this._lastModified = new DateTimeOffset(ZipHelper.DosTimeToDateTime(cd.LastModified));
            this._compressedSize = cd.CompressedSize;
            this._uncompressedSize = cd.UncompressedSize;
            this._offsetOfLocalHeader = cd.RelativeOffsetOfLocalHeader;
            this._storedOffsetOfCompressedData = new long?();
            this._crc32 = cd.Crc32;
            this._compressedBytes = (byte[])null;
            this._storedUncompressedData = (MemoryStream)null;
            this._currentlyOpenForWrite = false;
            this._everOpenedForWrite = false;
            this._outstandingWriteStream = (Stream)null;
            this.FullName = this.DecodeEntryName(cd.Filename);
            this._lhUnknownExtraFields = (List<ZipGenericExtraField>)null;
            this._cdUnknownExtraFields = cd.ExtraFields;
            this._fileComment = cd.FileComment;
        }

        internal ZipArchiveEntry(ZipArchive archive, string entryName)
        {
            this._archive = archive;
            this._originallyInArchive = false;
            this._diskNumberStart = 0;
            this._versionToExtract = ZipVersionNeededValues.Default;
            this._generalPurposeBitFlag = (ZipArchiveEntry.BitFlagValues)0;
            this.CompressionMethod = ZipArchiveEntry.CompressionMethodValues.Deflate;
            this._lastModified = DateTimeOffset.Now;
            this._compressedSize = 0L;
            this._uncompressedSize = 0L;
            this._offsetOfLocalHeader = 0L;
            this._storedOffsetOfCompressedData = new long?();
            this._crc32 = 0U;
            this._compressedBytes = (byte[])null;
            this._storedUncompressedData = (MemoryStream)null;
            this._currentlyOpenForWrite = false;
            this._everOpenedForWrite = false;
            this._outstandingWriteStream = (Stream)null;
            this.FullName = entryName;
            this._cdUnknownExtraFields = (List<ZipGenericExtraField>)null;
            this._lhUnknownExtraFields = (List<ZipGenericExtraField>)null;
            this._fileComment = (byte[])null;

            if (this._storedEntryNameBytes.Length > (int)ushort.MaxValue)
            {
                throw new ArgumentException(CompressionConstants.EntryNamesTooLong);
            }

            if (this._archive.Mode != ZipArchiveMode.Create)
            {
                return;
            }

            this._archive.AcquireArchiveStream(this);
        }

        [Flags]
        private enum BitFlagValues : ushort
        {
            DataDescriptor = (ushort)8,
            UnicodeFileName = (ushort)2048,
        }

        private enum CompressionMethodValues : ushort
        {
            Stored = (ushort)0,
            Deflate = (ushort)8,
        }

        private enum OpenableValues
        {
            Openable,
            FileNonExistent,
            FileTooLarge,
        }

        /// <summary>
        /// Gets the zip archive that the entry belongs to.
        /// </summary>
        /// <returns>The zip archive that the entry belongs to, or null if the entry has been deleted.</returns>
        public ZipArchive Archive
        {
            get
            {
                return this._archive;
            }
        }

        /// <summary>
        /// Gets the compressed size of the entry in the zip archive.
        /// </summary>
        /// <returns>The compressed size of the entry in the zip archive.</returns>
        /// <exception cref="T:System.InvalidOperationException">The value of the property is not available because the entry has been modified.</exception>
        public long CompressedLength
        {
            get
            {
                if (this._everOpenedForWrite)
                {
                    throw new InvalidOperationException(CompressionConstants.LengthAfterWrite);
                }
                else
                {
                    return this._compressedSize;
                }
            }
        }

        /// <summary>
        /// Gets the relative path of the entry in the zip archive.
        /// </summary>
        /// <returns>The relative path of the entry in the zip archive.</returns>
        public string FullName
        {
            get
            {
                return this._storedEntryName;
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("FullName");
                }

                bool isUTF8;
                this._storedEntryNameBytes = this.EncodeEntryName(value, out isUTF8);
                this._storedEntryName = value;

                if (isUTF8)
                {
                    this._generalPurposeBitFlag |= ZipArchiveEntry.BitFlagValues.UnicodeFileName;
                }
                else
                {
                    this._generalPurposeBitFlag &= ~ZipArchiveEntry.BitFlagValues.UnicodeFileName;
                }

                if (!ZipHelper.EndsWithDirChar(value))
                {
                    return;
                }

                this.VersionToExtractAtLeast(ZipVersionNeededValues.ExplicitDirectory);
            }
        }

        /// <summary>
        /// Gets or sets the last time the entry in the zip archive was changed.
        /// </summary>
        /// <returns>The last time the entry in the zip archive was changed.</returns>
        /// <exception cref="T:System.NotSupportedException">The attempt to set this property failed, because the zip archive for the entry is in <see cref="ZipArchiveMode.Read" /> mode.</exception>
        /// <exception cref="T:System.IO.IOException">The archive mode is set to <see cref="ZipArchiveMode.Create"/>.- or -The archive mode is set to <see cref="ZipArchiveMode.Update"/> and the entry has been opened.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">An attempt was made to set this property to a value that is either earlier than 1980 January 1 0:00:00 (midnight) or later than 2107 December 31 23:59:58 (one second before midnight).</exception>
        public DateTimeOffset LastWriteTime
        {
            get
            {
                return this._lastModified;
            }

            set
            {
                this.ThrowIfInvalidArchive();

                if (this._archive.Mode == ZipArchiveMode.Read)
                {
                    throw new NotSupportedException(CompressionConstants.ReadOnlyArchive);
                }

                if (this._archive.Mode == ZipArchiveMode.Create && this._everOpenedForWrite)
                {
                    throw new IOException(CompressionConstants.FrozenAfterWrite);
                }

                if (value.DateTime.Year < 1980 || value.DateTime.Year > 2107)
                {
                    throw new ArgumentOutOfRangeException("value", CompressionConstants.DateTimeOutOfRange);
                }

                this._lastModified = value;
            }
        }

        /// <summary>
        /// Gets the uncompressed size of the entry in the zip archive.
        /// </summary>
        /// <returns>The uncompressed size of the entry in the zip archive.</returns>
        /// <exception cref="T:System.InvalidOperationException">The value of the property is not available because the entry has been modified.</exception>
        public long Length
        {
            get
            {
                if (this._everOpenedForWrite)
                {
                    throw new InvalidOperationException(CompressionConstants.LengthAfterWrite);
                }
                else
                {
                    return this._uncompressedSize;
                }
            }
        }

        /// <summary>
        /// Gets the file name of the entry in the zip archive.
        /// </summary>
        /// <returns>The file name of the entry in the zip archive.</returns>
        public string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }

        internal bool EverOpenedForWrite
        {
            get
            {
                return this._everOpenedForWrite;
            }
        }

        private long OffsetOfCompressedData
        {
            get
            {
                if (!this._storedOffsetOfCompressedData.HasValue)
                {
                    this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader, SeekOrigin.Begin);

                    if (!ZipLocalFileHeader.TrySkipBlock(this._archive.ArchiveReader))
                    {
                        throw new InvalidDataException(CompressionConstants.LocalFileHeaderCorrupt);
                    }

                    this._storedOffsetOfCompressedData = new long?(this._archive.ArchiveStream.Position);
                }

                return this._storedOffsetOfCompressedData.Value;
            }
        }

        private MemoryStream UncompressedData
        {
            get
            {
                if (this._storedUncompressedData == null)
                {
                    this._storedUncompressedData = new MemoryStream((int)this._uncompressedSize);

                    if (this._originallyInArchive)
                    {
                        using (Stream stream = this.OpenInReadMode(false))
                        {
                            try
                            {
                                StreamHelper.Copy(stream, (Stream)this._storedUncompressedData);
                            }
                            catch (InvalidDataException)
                            {
                                this._storedUncompressedData.Dispose();
                                this._storedUncompressedData = (MemoryStream)null;
                                this._currentlyOpenForWrite = false;
                                this._everOpenedForWrite = false;
                                throw;
                            }
                        }
                    }

                    this.CompressionMethod = ZipArchiveEntry.CompressionMethodValues.Deflate;
                }

                return this._storedUncompressedData;
            }
        }

        private ZipArchiveEntry.CompressionMethodValues CompressionMethod
        {
            get
            {
                return this._storedCompressionMethod;
            }

            set
            {
                if (value == ZipArchiveEntry.CompressionMethodValues.Deflate)
                {
                    this.VersionToExtractAtLeast(ZipVersionNeededValues.ExplicitDirectory);
                }

                this._storedCompressionMethod = value;
            }
        }

        /// <summary>
        /// Deletes the entry from the zip archive.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">The entry is already open for reading or writing.</exception>
        /// <exception cref="T:System.NotSupportedException">The zip archive for this entry was opened in a mode other than <see cref="ZipArchiveMode.Update" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive for this entry has been disposed.</exception>
        public void Delete()
        {
            if (this._archive == null)
            {
                return;
            }

            if (this._currentlyOpenForWrite)
            {
                throw new IOException(CompressionConstants.DeleteOpenEntry);
            }

            if (this._archive.Mode != ZipArchiveMode.Update)
            {
                throw new NotSupportedException(CompressionConstants.DeleteOnlyInUpdate);
            }

            this._archive.ThrowIfDisposed();
            this._archive.RemoveEntry(this);
            this._archive = (ZipArchive)null;
            this.UnloadStreams();
        }

        /// <summary>
        /// Opens the entry from the zip archive.
        /// </summary>
        /// <returns>The stream that represents the contents of the entry.</returns>
        /// <exception cref="T:System.IO.IOException">The entry is already currently open for writing.-or-The entry has been deleted from the archive.-or-The archive for this entry was opened with the <see cref="ZipArchiveMode.Create"/> mode, and this entry has already been written to.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The entry is either missing from the archive or is corrupt and cannot be read. -or-The entry has been compressed by using a compression method that is not supported.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive for this entry has been disposed.</exception>
        public Stream Open()
        {
            this.ThrowIfInvalidArchive();

            switch (this._archive.Mode)
            {
                case ZipArchiveMode.Read:
                    return this.OpenInReadMode(true);
                case ZipArchiveMode.Create:
                    return this.OpenInWriteMode();
                default:
                    return this.OpenInUpdateMode();
            }
        }

        /// <summary>
        /// Extracts an entry in the zip archive to a file, and optionally overwrites an existing file that has the same name.
        /// </summary>
        /// <param name="destinationFileName">The path of the file to create from the contents of the entry. You can specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="overwrite">true to overwrite an existing file that has the same name as the destination file; otherwise, false.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="destinationFileName"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars"/>.-or-<paramref name="destinationFileName"/> specifies a directory.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="destinationFileName"/> is null. </exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.IO.IOException"><paramref name="destinationFileName"/> already exists and <paramref name="overwrite"/> is false.-or- An I/O error occurred.-or-The entry is currently open for writing.-or-The entry has been deleted from the archive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to create the new file.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The entry is missing from the archive or is corrupt and cannot be read.-or-The entry has been compressed by using a compression method that is not supported.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive that this entry belongs to has been disposed.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="destinationFileName"/> is in an invalid format. -or-The zip archive for this entry was opened in <see cref="ZipArchiveMode.Create" /> mode, which does not permit the retrieval of entries.</exception>
        public void ExtractToFile(string destinationFileName, bool overwrite)
        {
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }

            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

            using (Stream stream = File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None))
            {
                using (Stream stream2 = this.Open())
                {
                    StreamHelper.Copy(stream2, stream);
                }
            }

            File.SetLastWriteTime(destinationFileName, this.LastWriteTime.DateTime);
        }

        /// <summary>
        /// Retrieves the relative path of the entry in the zip archive.
        /// </summary>
        /// <returns>The relative path of the entry, which is the value stored in the <see cref="ZipArchiveEntry.FullName" /> property.</returns>
        public override string ToString()
        {
            return this.FullName;
        }

        internal void WriteAndFinishLocalEntry()
        {
            this.CloseStreams();
            this.WriteLocalFileHeaderAndDataIfNeeded();
            this.UnloadStreams();
        }

        internal void WriteCentralDirectoryFileHeader()
        {
            BinaryWriter binaryWriter = new BinaryWriter(this._archive.ArchiveStream);
            Zip64ExtraField zip64ExtraField = new Zip64ExtraField();
            bool flag = false;
            uint num1;
            uint num2;
            if (this.SizesTooLarge())
            {
                flag = true;
                num1 = uint.MaxValue;
                num2 = uint.MaxValue;
                zip64ExtraField.CompressedSize = new long?(this._compressedSize);
                zip64ExtraField.UncompressedSize = new long?(this._uncompressedSize);
            }
            else
            {
                num1 = (uint)this._compressedSize;
                num2 = (uint)this._uncompressedSize;
            }

            uint num3;

            if (this._offsetOfLocalHeader > (long)uint.MaxValue)
            {
                flag = true;
                num3 = uint.MaxValue;
                zip64ExtraField.LocalHeaderOffset = new long?(this._offsetOfLocalHeader);
            }
            else
            {
                num3 = (uint)this._offsetOfLocalHeader;
            }

            if (flag)
            {
                this.VersionToExtractAtLeast(ZipVersionNeededValues.Zip64);
            }

            int num4 = (flag ? (int)zip64ExtraField.TotalSize : 0) + (this._cdUnknownExtraFields != null ? ZipGenericExtraField.TotalSize(this._cdUnknownExtraFields) : 0);

            ushort num5;

            if (num4 > (int)ushort.MaxValue)
            {
                num5 = flag ? zip64ExtraField.TotalSize : (ushort)0;
                this._cdUnknownExtraFields = (List<ZipGenericExtraField>)null;
            }
            else
            {
                num5 = (ushort)num4;
            }

            binaryWriter.Write(33639248U);
            binaryWriter.Write((ushort)this._versionToExtract);
            binaryWriter.Write((ushort)this._versionToExtract);
            binaryWriter.Write((ushort)this._generalPurposeBitFlag);
            binaryWriter.Write((ushort)this.CompressionMethod);
            binaryWriter.Write(ZipHelper.DateTimeToDosTime(this._lastModified.DateTime));
            binaryWriter.Write(this._crc32);
            binaryWriter.Write(num1);
            binaryWriter.Write(num2);
            binaryWriter.Write((ushort)this._storedEntryNameBytes.Length);
            binaryWriter.Write(num5);
            binaryWriter.Write(this._fileComment != null ? (ushort)this._fileComment.Length : (ushort)0);
            binaryWriter.Write((ushort)0);
            binaryWriter.Write((ushort)0);
            binaryWriter.Write(0U);
            binaryWriter.Write(num3);
            binaryWriter.Write(this._storedEntryNameBytes);

            if (flag)
            {
                zip64ExtraField.WriteBlock(this._archive.ArchiveStream);
            }

            if (this._cdUnknownExtraFields != null)
            {
                ZipGenericExtraField.WriteAllBlocks(this._cdUnknownExtraFields, this._archive.ArchiveStream);
            }

            if (this._fileComment == null)
            {
                return;
            }

            binaryWriter.Write(this._fileComment);
        }

        internal bool LoadLocalHeaderExtraFieldAndCompressedBytesIfNeeded()
        {
            if (this._originallyInArchive)
            {
                this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader, SeekOrigin.Begin);
                this._lhUnknownExtraFields = ZipLocalFileHeader.GetExtraFields(this._archive.ArchiveReader);
            }

            if (!this._everOpenedForWrite && this._originallyInArchive)
            {
                this._compressedBytes = new byte[this._compressedSize];
                this._archive.ArchiveStream.Seek(this.OffsetOfCompressedData, SeekOrigin.Begin);
                ZipHelper.ReadBytes(this._archive.ArchiveStream, this._compressedBytes, (int)this._compressedSize);
            }

            return true;
        }

        internal void ThrowIfNotOpenable(bool needToUncompress, bool needToLoadIntoMemory)
        {
            string message;

            if (!this.IsOpenable(needToUncompress, needToLoadIntoMemory, out message))
            {
                throw new InvalidDataException(message);
            }
        }

        private string DecodeEntryName(byte[] entryNameBytes)
        {
            return ((this._generalPurposeBitFlag & ZipArchiveEntry.BitFlagValues.UnicodeFileName) != (ZipArchiveEntry.BitFlagValues)0 ? Encoding.UTF8 : (this._archive == null ? Encoding.Default : this._archive.EntryNameEncoding ?? Encoding.Default)).GetString(entryNameBytes);
        }

        private byte[] EncodeEntryName(string entryName, out bool isUTF8)
        {
            Encoding encoding = this._archive == null || this._archive.EntryNameEncoding == null ? (ZipHelper.RequiresUnicode(entryName) ? Encoding.UTF8 : Encoding.Default) : this._archive.EntryNameEncoding;
            isUTF8 = encoding is UTF8Encoding && encoding.Equals((object)Encoding.UTF8);
            return encoding.GetBytes(entryName);
        }

        private CheckSumAndSizeWriteStream GetDataCompressor(Stream backingStream, bool leaveBackingStreamOpen, EventHandler onClose)
        {
            Stream baseStream = (Stream)new DeflateStream(backingStream, CompressionMode.Compress, leaveBackingStreamOpen);
            bool flag = true;
            bool leaveOpenOnClose = leaveBackingStreamOpen && !flag;

            return new CheckSumAndSizeWriteStream(baseStream, backingStream, leaveOpenOnClose, (Action<long, long, uint>)((initialPosition, currentPosition, checkSum) =>
            {
                this._crc32 = checkSum;
                this._uncompressedSize = currentPosition;
                this._compressedSize = backingStream.Position - initialPosition;

                if (onClose == null)
                {
                    return;
                }

                onClose((object)this, EventArgs.Empty);
            }));
        }

        private Stream GetDataDecompressor(Stream compressedStreamToRead)
        {
            Stream stream;
            switch (this.CompressionMethod)
            {
                case ZipArchiveEntry.CompressionMethodValues.Deflate:
                    stream = (Stream)new DeflateStream(compressedStreamToRead, CompressionMode.Decompress);
                    break;
                default:
                    stream = compressedStreamToRead;
                    break;
            }

            return stream;
        }

        private Stream OpenInReadMode(bool checkOpenable)
        {
            if (checkOpenable)
            {
                this.ThrowIfNotOpenable(true, false);
            }

            return this.GetDataDecompressor((Stream)new SubReadStream(this._archive.ArchiveStream, this.OffsetOfCompressedData, this._compressedSize));
        }

        private Stream OpenInWriteMode()
        {
            if (this._everOpenedForWrite)
            {
                throw new IOException(CompressionConstants.CreateModeWriteOnceAndOneEntryAtATime);
            }

            this._everOpenedForWrite = true;
            this._outstandingWriteStream = (Stream)new ZipArchiveEntry.DirectToArchiveWriterStream(this.GetDataCompressor(this._archive.ArchiveStream, true, (EventHandler)((o, e) =>
            {
                this._archive.ReleaseArchiveStream(this);
                this._outstandingWriteStream = (Stream)null;
            })), this);

            return (Stream)new WrappedStream(this._outstandingWriteStream, (EventHandler)((o, e) => this._outstandingWriteStream.Close()));
        }

        private Stream OpenInUpdateMode()
        {
            if (this._currentlyOpenForWrite)
            {
                throw new IOException(CompressionConstants.UpdateModeOneStream);
            }

            this.ThrowIfNotOpenable(true, true);
            this._everOpenedForWrite = true;
            this._currentlyOpenForWrite = true;
            this.UncompressedData.Seek(0L, SeekOrigin.Begin);

            return (Stream)new WrappedStream((Stream)this.UncompressedData, (EventHandler)((o, e) => this._currentlyOpenForWrite = false));
        }

        private bool IsOpenable(bool needToUncompress, bool needToLoadIntoMemory, out string message)
        {
            message = (string)null;

            if (this._originallyInArchive)
            {
                if (needToUncompress && this.CompressionMethod != ZipArchiveEntry.CompressionMethodValues.Stored && this.CompressionMethod != ZipArchiveEntry.CompressionMethodValues.Deflate)
                {
                    message = CompressionConstants.UnsupportedCompression;
                    return false;
                }
                else if ((long)this._diskNumberStart != (long)this._archive.NumberOfThisDisk)
                {
                    message = CompressionConstants.SplitSpanned;
                    return false;
                }
                else if (this._offsetOfLocalHeader > this._archive.ArchiveStream.Length)
                {
                    message = CompressionConstants.LocalFileHeaderCorrupt;
                    return false;
                }
                else
                {
                    this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader, SeekOrigin.Begin);
                    if (!ZipLocalFileHeader.TrySkipBlock(this._archive.ArchiveReader))
                    {
                        message = CompressionConstants.LocalFileHeaderCorrupt;
                        return false;
                    }
                    else if (this.OffsetOfCompressedData + this._compressedSize > this._archive.ArchiveStream.Length)
                    {
                        message = CompressionConstants.LocalFileHeaderCorrupt;
                        return false;
                    }
                    else if (needToLoadIntoMemory && this._compressedSize > (long)int.MaxValue)
                    {
                        message = CompressionConstants.EntryTooLarge;
                        return false;
                    }
                }
            }

            return true;
        }

        private bool SizesTooLarge()
        {
            if (this._compressedSize <= (long)uint.MaxValue)
            {
                return this._uncompressedSize > (long)uint.MaxValue;
            }
            else
            {
                return true;
            }
        }

        private bool WriteLocalFileHeader(bool isEmptyFile)
        {
            BinaryWriter binaryWriter = new BinaryWriter(this._archive.ArchiveStream);
            Zip64ExtraField zip64ExtraField = new Zip64ExtraField();
            bool flag = false;
            uint num1;
            uint num2;

            if (isEmptyFile)
            {
                this.CompressionMethod = ZipArchiveEntry.CompressionMethodValues.Stored;
                num1 = 0U;
                num2 = 0U;
            }
            else if (this._archive.Mode == ZipArchiveMode.Create && !this._archive.ArchiveStream.CanSeek && !isEmptyFile)
            {
                this._generalPurposeBitFlag |= ZipArchiveEntry.BitFlagValues.DataDescriptor;
                flag = false;
                num1 = 0U;
                num2 = 0U;
            }
            else if (this.SizesTooLarge())
            {
                flag = true;
                num1 = uint.MaxValue;
                num2 = uint.MaxValue;
                zip64ExtraField.CompressedSize = new long?(this._compressedSize);
                zip64ExtraField.UncompressedSize = new long?(this._uncompressedSize);
                this.VersionToExtractAtLeast(ZipVersionNeededValues.Zip64);
            }
            else
            {
                flag = false;
                num1 = (uint)this._compressedSize;
                num2 = (uint)this._uncompressedSize;
            }

            this._offsetOfLocalHeader = (long)(uint)binaryWriter.BaseStream.Position;
            int num3 = (flag ? (int)zip64ExtraField.TotalSize : 0) + (this._lhUnknownExtraFields != null ? ZipGenericExtraField.TotalSize(this._lhUnknownExtraFields) : 0);
            ushort num4;

            if (num3 > (int)ushort.MaxValue)
            {
                num4 = flag ? zip64ExtraField.TotalSize : (ushort)0;
                this._lhUnknownExtraFields = (List<ZipGenericExtraField>)null;
            }
            else
            {
                num4 = (ushort)num3;
            }

            binaryWriter.Write(67324752U);
            binaryWriter.Write((ushort)this._versionToExtract);
            binaryWriter.Write((ushort)this._generalPurposeBitFlag);
            binaryWriter.Write((ushort)this.CompressionMethod);
            binaryWriter.Write(ZipHelper.DateTimeToDosTime(this._lastModified.DateTime));
            binaryWriter.Write(this._crc32);
            binaryWriter.Write(num1);
            binaryWriter.Write(num2);
            binaryWriter.Write((ushort)this._storedEntryNameBytes.Length);
            binaryWriter.Write(num4);
            binaryWriter.Write(this._storedEntryNameBytes);

            if (flag)
            {
                zip64ExtraField.WriteBlock(this._archive.ArchiveStream);
            }

            if (this._lhUnknownExtraFields != null)
            {
                ZipGenericExtraField.WriteAllBlocks(this._lhUnknownExtraFields, this._archive.ArchiveStream);
            }

            return flag;
        }

        private void WriteLocalFileHeaderAndDataIfNeeded()
        {
            if (this._storedUncompressedData != null || this._compressedBytes != null)
            {
                if (this._storedUncompressedData != null)
                {
                    this._uncompressedSize = this._storedUncompressedData.Length;
                    using (Stream destination = (Stream)new ZipArchiveEntry.DirectToArchiveWriterStream(this.GetDataCompressor(this._archive.ArchiveStream, true, (EventHandler)null), this))
                    {
                        this._storedUncompressedData.Seek(0L, SeekOrigin.Begin);
                        StreamHelper.Copy(this._storedUncompressedData, destination);
                        this._storedUncompressedData.Close();
                        this._storedUncompressedData = (MemoryStream)null;
                    }
                }
                else
                {
                    if (this._uncompressedSize == 0L)
                    {
                        this.CompressionMethod = ZipArchiveEntry.CompressionMethodValues.Stored;
                    }

                    this.WriteLocalFileHeader(false);

                    using (MemoryStream memoryStream = new MemoryStream(this._compressedBytes))
                    {
                        StreamHelper.Copy(memoryStream, this._archive.ArchiveStream);
                    }
                }
            }
            else
            {
                if (this._archive.Mode != ZipArchiveMode.Update && this._everOpenedForWrite)
                {
                    return;
                }

                this._everOpenedForWrite = true;
                this.WriteLocalFileHeader(true);
            }
        }

        private void WriteCrcAndSizesInLocalHeader(bool zip64HeaderUsed)
        {
            long position = this._archive.ArchiveStream.Position;
            BinaryWriter binaryWriter = new BinaryWriter(this._archive.ArchiveStream);
            bool flag1 = this.SizesTooLarge();
            bool flag2 = flag1 && !zip64HeaderUsed;
            uint num1 = flag1 ? uint.MaxValue : (uint)this._compressedSize;
            uint num2 = flag1 ? uint.MaxValue : (uint)this._uncompressedSize;

            if (flag2)
            {
                this._generalPurposeBitFlag |= ZipArchiveEntry.BitFlagValues.DataDescriptor;
                this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader + 6L, SeekOrigin.Begin);
                binaryWriter.Write((ushort)this._generalPurposeBitFlag);
            }

            this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader + 14L, SeekOrigin.Begin);

            if (!flag2)
            {
                binaryWriter.Write(this._crc32);
                binaryWriter.Write(num1);
                binaryWriter.Write(num2);
            }
            else
            {
                binaryWriter.Write(0U);
                binaryWriter.Write(0U);
                binaryWriter.Write(0U);
            }

            if (zip64HeaderUsed)
            {
                this._archive.ArchiveStream.Seek(this._offsetOfLocalHeader + 30L + (long)this._storedEntryNameBytes.Length + 4L, SeekOrigin.Begin);
                binaryWriter.Write(this._uncompressedSize);
                binaryWriter.Write(this._compressedSize);
                this._archive.ArchiveStream.Seek(position, SeekOrigin.Begin);
            }

            this._archive.ArchiveStream.Seek(position, SeekOrigin.Begin);

            if (!flag2)
            {
                return;
            }

            binaryWriter.Write(this._crc32);
            binaryWriter.Write(this._compressedSize);
            binaryWriter.Write(this._uncompressedSize);
        }

        private void WriteDataDescriptor()
        {
            BinaryWriter binaryWriter = new BinaryWriter(this._archive.ArchiveStream);
            binaryWriter.Write(134695760U);
            binaryWriter.Write(this._crc32);

            if (this.SizesTooLarge())
            {
                binaryWriter.Write(this._compressedSize);
                binaryWriter.Write(this._uncompressedSize);
            }
            else
            {
                binaryWriter.Write((uint)this._compressedSize);
                binaryWriter.Write((uint)this._uncompressedSize);
            }
        }

        private void UnloadStreams()
        {
            if (this._storedUncompressedData != null)
            {
                this._storedUncompressedData.Close();
            }

            this._compressedBytes = (byte[])null;
            this._outstandingWriteStream = (Stream)null;
        }

        private void CloseStreams()
        {
            if (this._outstandingWriteStream == null)
            {
                return;
            }

            this._outstandingWriteStream.Close();
        }

        private void VersionToExtractAtLeast(ZipVersionNeededValues value)
        {
            if (this._versionToExtract >= value)
            {
                return;
            }

            this._versionToExtract = value;
        }

        private void ThrowIfInvalidArchive()
        {
            if (this._archive == null)
            {
                throw new InvalidOperationException(CompressionConstants.DeletedEntry);
            }

            this._archive.ThrowIfDisposed();
        }

        private class DirectToArchiveWriterStream : Stream
        {
            private long _position;

            private CheckSumAndSizeWriteStream _crcSizeStream;

            private bool _everWritten;

            private bool _isDisposed;

            private ZipArchiveEntry _entry;

            private bool _usedZip64inLH;

            private bool _canWrite;

            public DirectToArchiveWriterStream(CheckSumAndSizeWriteStream crcSizeStream, ZipArchiveEntry entry)
            {
                this._position = 0L;
                this._crcSizeStream = crcSizeStream;
                this._everWritten = false;
                this._isDisposed = false;
                this._entry = entry;
                this._usedZip64inLH = false;
                this._canWrite = true;
            }

            public override long Length
            {
                get
                {
                    this.ThrowIfDisposed();
                    throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
                }
            }

            public override long Position
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this._position;
                }

                set
                {
                    this.ThrowIfDisposed();
                    throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
                }
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this._canWrite;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                this.ThrowIfDisposed();
                throw new NotSupportedException(CompressionConstants.ReadingNotSupported);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                this.ThrowIfDisposed();
                throw new NotSupportedException(CompressionConstants.SeekingNotSupported);
            }

            public override void SetLength(long value)
            {
                this.ThrowIfDisposed();
                throw new NotSupportedException(CompressionConstants.SetLengthRequiresSeekingAndWriting);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException("offset", CompressionConstants.ArgumentNeedNonNegative);
                }

                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count", CompressionConstants.ArgumentNeedNonNegative);
                }

                if (buffer.Length - offset < count)
                {
                    throw new ArgumentException(CompressionConstants.OffsetLengthInvalid);
                }

                this.ThrowIfDisposed();

                if (count == 0)
                {
                    return;
                }

                if (!this._everWritten)
                {
                    this._everWritten = true;
                    this._usedZip64inLH = this._entry.WriteLocalFileHeader(false);
                }

                this._crcSizeStream.Write(buffer, offset, count);
                this._position += (long)count;
            }

            public override void Flush()
            {
                this.ThrowIfDisposed();
                this._crcSizeStream.Flush();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && !this._isDisposed)
                {
                    this._crcSizeStream.Close();

                    if (!this._everWritten)
                    {
                        this._entry.WriteLocalFileHeader(true);
                    }
                    else if (this._entry._archive.ArchiveStream.CanSeek)
                    {
                        this._entry.WriteCrcAndSizesInLocalHeader(this._usedZip64inLH);
                    }
                    else
                    {
                        this._entry.WriteDataDescriptor();
                    }

                    this._canWrite = false;
                    this._isDisposed = true;
                }

                base.Dispose(disposing);
            }

            private void ThrowIfDisposed()
            {
                if (this._isDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name, CompressionConstants.HiddenStreamName);
                }
            }
        }
    }
}
