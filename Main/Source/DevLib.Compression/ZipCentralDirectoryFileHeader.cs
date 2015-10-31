//-----------------------------------------------------------------------
// <copyright file="ZipCentralDirectoryFileHeader.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.Collections.Generic;
    using System.IO;

    internal struct ZipCentralDirectoryFileHeader
    {
        public const uint SignatureConstant = 33639248U;
        public ushort VersionMadeBy;
        public ushort VersionNeededToExtract;
        public ushort GeneralPurposeBitFlag;
        public ushort CompressionMethod;
        public uint LastModified;
        public uint Crc32;
        public long CompressedSize;
        public long UncompressedSize;
        public ushort FilenameLength;
        public ushort ExtraFieldLength;
        public ushort FileCommentLength;
        public int DiskNumberStart;
        public ushort InternalFileAttributes;
        public uint ExternalFileAttributes;
        public long RelativeOffsetOfLocalHeader;
        public byte[] Filename;
        public byte[] FileComment;
        public List<ZipGenericExtraField> ExtraFields;

        public static bool TryReadBlock(BinaryReader reader, bool saveExtraFieldsAndComments, out ZipCentralDirectoryFileHeader header)
        {
            header = new ZipCentralDirectoryFileHeader();

            if (reader.ReadUInt32() != 33639248u)
            {
                return false;
            }

            header.VersionMadeBy = reader.ReadUInt16();
            header.VersionNeededToExtract = reader.ReadUInt16();
            header.GeneralPurposeBitFlag = reader.ReadUInt16();
            header.CompressionMethod = reader.ReadUInt16();
            header.LastModified = reader.ReadUInt32();
            header.Crc32 = reader.ReadUInt32();
            uint num = reader.ReadUInt32();
            uint num2 = reader.ReadUInt32();
            header.FilenameLength = reader.ReadUInt16();
            header.ExtraFieldLength = reader.ReadUInt16();
            header.FileCommentLength = reader.ReadUInt16();
            ushort num3 = reader.ReadUInt16();
            header.InternalFileAttributes = reader.ReadUInt16();
            header.ExternalFileAttributes = reader.ReadUInt32();
            uint num4 = reader.ReadUInt32();
            header.Filename = reader.ReadBytes((int)header.FilenameLength);
            bool readUncompressedSize = num2 == 4294967295u;
            bool readCompressedSize = num == 4294967295u;
            bool readLocalHeaderOffset = num4 == 4294967295u;
            bool readStartDiskNumber = num3 == 65535;
            long position = reader.BaseStream.Position + (long)((ulong)header.ExtraFieldLength);
            Zip64ExtraField zip64ExtraField;

            using (Stream stream = new SubReadStream(reader.BaseStream, reader.BaseStream.Position, (long)((ulong)header.ExtraFieldLength)))
            {
                if (saveExtraFieldsAndComments)
                {
                    header.ExtraFields = ZipGenericExtraField.ParseExtraField(stream);
                    zip64ExtraField = Zip64ExtraField.GetAndRemoveZip64Block(header.ExtraFields, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber);
                }
                else
                {
                    header.ExtraFields = null;
                    zip64ExtraField = Zip64ExtraField.GetJustZip64Block(stream, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber);
                }
            }

            ZipHelper.AdvanceToPosition(reader.BaseStream, position);

            if (saveExtraFieldsAndComments)
            {
                header.FileComment = reader.ReadBytes((int)header.FileCommentLength);
            }
            else
            {
                reader.BaseStream.Position += (long)((ulong)header.FileCommentLength);
                header.FileComment = null;
            }

            header.UncompressedSize = (long)((!zip64ExtraField.UncompressedSize.HasValue) ? ((ulong)num2) : ((ulong)zip64ExtraField.UncompressedSize.Value));
            header.CompressedSize = (long)((!zip64ExtraField.CompressedSize.HasValue) ? ((ulong)num) : ((ulong)zip64ExtraField.CompressedSize.Value));
            header.RelativeOffsetOfLocalHeader = (long)((!zip64ExtraField.LocalHeaderOffset.HasValue) ? ((ulong)num4) : ((ulong)zip64ExtraField.LocalHeaderOffset.Value));
            header.DiskNumberStart = ((!zip64ExtraField.StartDiskNumber.HasValue) ? ((int)num3) : zip64ExtraField.StartDiskNumber.Value);

            return true;
        }
    }
}
