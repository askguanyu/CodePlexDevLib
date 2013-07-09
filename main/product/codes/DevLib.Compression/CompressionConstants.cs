//-----------------------------------------------------------------------
// <copyright file="CompressionConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    internal static class CompressionConstants
    {
        internal const string UpdateModeOneStream = "Entries cannot be opened multiple times in Update mode.";
        internal const string ArgumentNeedNonNegative = "The argument must be non-negative.";
        internal const string CannotBeEmpty = "String cannot be empty.";
        internal const string CDCorrupt = "Central Directory corrupt.";
        internal const string CentralDirectoryInvalid = "Central Directory is invalid.";
        internal const string CreateInReadMode = "Cannot create entries on an archive opened in read mode.";
        internal const string CreateModeCapabilities = "Cannot use create mode on a non-writeable stream.";
        internal const string CreateModeCreateEntryWhileOpen = "Entries cannot be created while previously created entries are still open.";
        internal const string CreateModeWriteOnceAndOneEntryAtATime = "Entries in create mode may only be written to once, and only one entry may be held open at a time.";
        internal const string DateTimeInvalid = "The DateTime in the Zip file is invalid.";
        internal const string DateTimeOutOfRange = "The DateTimeOffset specified cannot be converted into a Zip file timestamp.";
        internal const string DeletedEntry = "Cannot modify deleted entry.";
        internal const string DeleteOnlyInUpdate = "Delete can only be used when the archive is in Update mode.";
        internal const string DeleteOpenEntry = "Cannot delete an entry currently open for writing.";
        internal const string EntriesInCreateMode = "Cannot access entries in Create mode.";
        internal const string EntryNameEncodingNotSupported = "The specified entry name encoding is not supported.";
        internal const string EntryNamesTooLong = "Entry names cannot require more than 2^16 bits.";
        internal const string EntryTooLarge = "Entries larger than 4GB are not supported in Update mode.";
        internal const string EOCDNotFound = "End of Central Directory record could not be found.";
        internal const string FieldTooBigCompressedSize = "Compressed Size cannot be held in an Int64.";
        internal const string FieldTooBigLocalHeaderOffset = "Local Header Offset cannot be held in an Int64.";
        internal const string FieldTooBigNumEntries = "Number of Entries cannot be held in an Int64.";
        internal const string FieldTooBigOffsetToCD = "Offset to Central Directory cannot be held in an Int64.";
        internal const string FieldTooBigOffsetToZip64EOCD = "Offset to Zip64 End Of Central Directory record cannot be held in an Int64.";
        internal const string FieldTooBigStartDiskNumber = "Start Disk Number cannot be held in an Int64.";
        internal const string FieldTooBigUncompressedSize = "Uncompressed Size cannot be held in an Int64.";
        internal const string FrozenAfterWrite = "Cannot modify entry in Create mode after entry has been opened for writing.";
        internal const string HiddenStreamName = "A stream from ZipArchiveEntry has been disposed.";
        internal const string LengthAfterWrite = "Length properties are unavailable once an entry has been opened for writing.";
        internal const string LocalFileHeaderCorrupt = "A local file header is corrupt.";
        internal const string NumEntriesWrong = "Number of entries expected in End Of Central Directory does not correspond to number of entries in Central Directory.";
        internal const string OffsetLengthInvalid = "The offset and length parameters are not valid for the array that was given.";
        internal const string ReadingNotSupported = "This stream from ZipArchiveEntry does not support reading.";
        internal const string ReadModeCapabilities = "Cannot use read mode on a non-readable stream.";
        internal const string ReadOnlyArchive = "Cannot modify read-only archive.";
        internal const string SeekingNotSupported = "This stream from ZipArchiveEntry does not support seeking.";
        internal const string SetLengthRequiresSeekingAndWriting = "SetLength requires a stream that supports seeking and writing.";
        internal const string SplitSpanned = "Split or spanned archives are not supported.";
        internal const string UnexpectedEndOfStream = "Zip file corrupt: unexpected end of stream reached.";
        internal const string UnsupportedCompression = "The archive entry was compressed using an unsupported compression method.";
        internal const string UpdateModeCapabilities = "Update mode requires a stream with read, write, and seek capabilities.";
        internal const string WritingNotSupported = "This stream from ZipArchiveEntry does not support writing.";
        internal const string Zip64EOCDNotWhereExpected = "Zip 64 End of Central Directory Record not where indicated.";
        internal const string ExtractingResultsInOutside = "Extracting Zip entry would have resulted in a file outside the specified destination directory.";
        internal const string DirectoryNameWithData = "Zip entry name ends in directory separator character but contains data.";
    }
}
