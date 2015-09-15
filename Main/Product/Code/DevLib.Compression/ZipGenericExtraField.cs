//-----------------------------------------------------------------------
// <copyright file="ZipGenericExtraField.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.Collections.Generic;
    using System.IO;

    internal struct ZipGenericExtraField
    {
        private const int SizeOfHeader = 4;

        private ushort _tag;

        private ushort _size;

        private byte[] _data;

        public ushort Tag
        {
            get
            {
                return this._tag;
            }
        }

        public ushort Size
        {
            get
            {
                return this._size;
            }
        }

        public byte[] Data
        {
            get
            {
                return this._data;
            }
        }

        public static bool TryReadBlock(BinaryReader reader, long endExtraField, out ZipGenericExtraField field)
        {
            field = new ZipGenericExtraField();

            if (endExtraField - reader.BaseStream.Position < 4L)
            {
                return false;
            }

            field._tag = reader.ReadUInt16();

            field._size = reader.ReadUInt16();

            if (endExtraField - reader.BaseStream.Position < (long)field._size)
            {
                return false;
            }

            field._data = reader.ReadBytes((int)field._size);

            return true;
        }

        public static List<ZipGenericExtraField> ParseExtraField(Stream extraFieldData)
        {
            List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();
            using (BinaryReader reader = new BinaryReader(extraFieldData))
            {
                ZipGenericExtraField field;

                while (ZipGenericExtraField.TryReadBlock(reader, extraFieldData.Length, out field))
                {
                    list.Add(field);
                }
            }

            return list;
        }

        public static int TotalSize(List<ZipGenericExtraField> fields)
        {
            int num = 0;

            foreach (ZipGenericExtraField genericExtraField in fields)
            {
                num += (int)genericExtraField.Size + 4;
            }

            return num;
        }

        public static void WriteAllBlocks(List<ZipGenericExtraField> fields, Stream stream)
        {
            foreach (ZipGenericExtraField genericExtraField in fields)
            {
                genericExtraField.WriteBlock(stream);
            }
        }

        public void WriteBlock(Stream stream)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(this.Tag);
            binaryWriter.Write(this.Size);
            binaryWriter.Write(this.Data);
        }
    }
}
