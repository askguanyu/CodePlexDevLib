//-----------------------------------------------------------------------
// <copyright file="Zip64EndOfCentralDirectoryLocator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.IO;

    internal struct Zip64EndOfCentralDirectoryLocator
    {
        public const uint SignatureConstant = 117853008U;

        public const int SizeOfBlockWithoutSignature = 16;

        public uint NumberOfDiskWithZip64EOCD;

        public ulong OffsetOfZip64EOCD;

        public uint TotalNumberOfDisks;

        public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

            if ((int)reader.ReadUInt32() != 117853008)
            {
                return false;
            }

            zip64EOCDLocator.NumberOfDiskWithZip64EOCD = reader.ReadUInt32();
            zip64EOCDLocator.OffsetOfZip64EOCD = reader.ReadUInt64();
            zip64EOCDLocator.TotalNumberOfDisks = reader.ReadUInt32();

            return true;
        }

        public static void WriteBlock(Stream stream, long zip64EOCDRecordStart)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(117853008U);
            binaryWriter.Write(0U);
            binaryWriter.Write(zip64EOCDRecordStart);
            binaryWriter.Write(1U);
        }
    }
}
