//-----------------------------------------------------------------------
// <copyright file="Zip64EndOfCentralDirectoryRecord.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.IO;

    internal struct Zip64EndOfCentralDirectoryRecord
    {
        public ulong SizeOfThisRecord;
        public ushort VersionMadeBy;
        public ushort VersionNeededToExtract;
        public uint NumberOfThisDisk;
        public uint NumberOfDiskWithStartOfCD;
        public ulong NumberOfEntriesOnThisDisk;
        public ulong NumberOfEntriesTotal;
        public ulong SizeOfCentralDirectory;
        public ulong OffsetOfCentralDirectory;
        private const uint SignatureConstant = 101075792U;
        private const ulong NormalSize = 44UL;

        public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

            if ((int)reader.ReadUInt32() != 101075792)
            {
                return false;
            }

            zip64EOCDRecord.SizeOfThisRecord = reader.ReadUInt64();
            zip64EOCDRecord.VersionMadeBy = reader.ReadUInt16();
            zip64EOCDRecord.VersionNeededToExtract = reader.ReadUInt16();
            zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
            zip64EOCDRecord.NumberOfDiskWithStartOfCD = reader.ReadUInt32();
            zip64EOCDRecord.NumberOfEntriesOnThisDisk = reader.ReadUInt64();
            zip64EOCDRecord.NumberOfEntriesTotal = reader.ReadUInt64();
            zip64EOCDRecord.SizeOfCentralDirectory = reader.ReadUInt64();
            zip64EOCDRecord.OffsetOfCentralDirectory = reader.ReadUInt64();

            return true;
        }

        public static void WriteBlock(Stream stream, long numberOfEntries, long startOfCentralDirectory, long sizeOfCentralDirectory)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);

            int num1 = 101075792;
            binaryWriter.Write((uint)num1);

            long num2 = 44L;
            binaryWriter.Write((ulong)num2);

            int num3 = 45;
            binaryWriter.Write((ushort)num3);

            int num4 = 45;
            binaryWriter.Write((ushort)num4);

            int num5 = 0;
            binaryWriter.Write((uint)num5);

            int num6 = 0;
            binaryWriter.Write((uint)num6);

            long num7 = numberOfEntries;
            binaryWriter.Write(num7);

            long num8 = numberOfEntries;
            binaryWriter.Write(num8);

            long num9 = sizeOfCentralDirectory;
            binaryWriter.Write(num9);

            long num10 = startOfCentralDirectory;
            binaryWriter.Write(num10);
        }
    }
}
