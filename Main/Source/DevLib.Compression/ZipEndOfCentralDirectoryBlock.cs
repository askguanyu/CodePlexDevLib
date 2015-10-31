//-----------------------------------------------------------------------
// <copyright file="ZipEndOfCentralDirectoryBlock.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.IO;

    internal struct ZipEndOfCentralDirectoryBlock
    {
        public const uint SignatureConstant = 101010256U;
        public const int SizeOfBlockWithoutSignature = 18;
        public uint Signature;
        public ushort NumberOfThisDisk;
        public ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
        public ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
        public ushort NumberOfEntriesInTheCentralDirectory;
        public uint SizeOfCentralDirectory;
        public uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
        public byte[] ArchiveComment;

        public static void WriteBlock(Stream stream, long numberOfEntries, long startOfCentralDirectory, long sizeOfCentralDirectory, byte[] archiveComment)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);

            ushort num1 = numberOfEntries > (long)ushort.MaxValue ? ushort.MaxValue : (ushort)numberOfEntries;
            uint num2 = startOfCentralDirectory > (long)uint.MaxValue ? uint.MaxValue : (uint)startOfCentralDirectory;
            uint num3 = sizeOfCentralDirectory > (long)uint.MaxValue ? uint.MaxValue : (uint)sizeOfCentralDirectory;
            binaryWriter.Write(101010256U);
            binaryWriter.Write((ushort)0);
            binaryWriter.Write((ushort)0);
            binaryWriter.Write(num1);
            binaryWriter.Write(num1);
            binaryWriter.Write(num3);
            binaryWriter.Write(num2);
            binaryWriter.Write(archiveComment != null ? (ushort)archiveComment.Length : (ushort)0);

            if (archiveComment != null)
            {
                binaryWriter.Write(archiveComment);
            }
        }

        public static bool TryReadBlock(BinaryReader reader, out ZipEndOfCentralDirectoryBlock eocdBlock)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();

            if ((int)reader.ReadUInt32() != 101010256)
            {
                return false;
            }

            eocdBlock.Signature = 101010256U;
            eocdBlock.NumberOfThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectory = reader.ReadUInt16();
            eocdBlock.SizeOfCentralDirectory = reader.ReadUInt32();
            eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();
            ushort num = reader.ReadUInt16();
            eocdBlock.ArchiveComment = reader.ReadBytes((int)num);

            return true;
        }
    }
}
