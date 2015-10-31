//-----------------------------------------------------------------------
// <copyright file="ZipLocalFileHeader.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct ZipLocalFileHeader
    {
        public const uint DataDescriptorSignature = 134695760U;
        public const uint SignatureConstant = 67324752U;
        public const int OffsetToCrcFromHeaderStart = 14;
        public const int OffsetToBitFlagFromHeaderStart = 6;
        public const int SizeOfLocalHeader = 30;

        public static List<ZipGenericExtraField> GetExtraFields(BinaryReader reader)
        {
            reader.BaseStream.Seek(26L, SeekOrigin.Current);
            ushort num1 = reader.ReadUInt16();
            ushort num2 = reader.ReadUInt16();
            reader.BaseStream.Seek((long)num1, SeekOrigin.Current);
            List<ZipGenericExtraField> extraFields;

            using (Stream extraFieldData = (Stream)new SubReadStream(reader.BaseStream, reader.BaseStream.Position, (long)num2))
            {
                extraFields = ZipGenericExtraField.ParseExtraField(extraFieldData);
            }

            Zip64ExtraField.RemoveZip64Blocks(extraFields);

            return extraFields;
        }

        public static bool TrySkipBlock(BinaryReader reader)
        {
            if ((int)reader.ReadUInt32() != 67324752 || reader.BaseStream.Length < reader.BaseStream.Position + 22L)
            {
                return false;
            }

            reader.BaseStream.Seek(22L, SeekOrigin.Current);
            ushort num1 = reader.ReadUInt16();
            ushort num2 = reader.ReadUInt16();

            if (reader.BaseStream.Length < reader.BaseStream.Position + (long)num1 + (long)num2)
            {
                return false;
            }

            reader.BaseStream.Seek((long)((int)num1 + (int)num2), SeekOrigin.Current);

            return true;
        }
    }
}
