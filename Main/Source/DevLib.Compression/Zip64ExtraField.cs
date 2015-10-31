//-----------------------------------------------------------------------
// <copyright file="Zip64ExtraField.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System.Collections.Generic;
    using System.IO;

    internal struct Zip64ExtraField
    {
        public const int OffsetToFirstField = 4;
        private const ushort TagConstant = (ushort)1;
        private ushort _size;
        private long? _uncompressedSize;
        private long? _compressedSize;
        private long? _localHeaderOffset;
        private int? _startDiskNumber;

        public ushort TotalSize
        {
            get
            {
                return (ushort)((uint)this._size + 4U);
            }
        }

        public long? UncompressedSize
        {
            get
            {
                return this._uncompressedSize;
            }

            set
            {
                this._uncompressedSize = value;
                this.UpdateSize();
            }
        }

        public long? CompressedSize
        {
            get
            {
                return this._compressedSize;
            }

            set
            {
                this._compressedSize = value;
                this.UpdateSize();
            }
        }

        public long? LocalHeaderOffset
        {
            get
            {
                return this._localHeaderOffset;
            }

            set
            {
                this._localHeaderOffset = value;
                this.UpdateSize();
            }
        }

        public int? StartDiskNumber
        {
            get
            {
                return this._startDiskNumber;
            }
        }

        public static Zip64ExtraField GetJustZip64Block(Stream extraFieldStream, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber)
        {
            using (BinaryReader reader = new BinaryReader(extraFieldStream))
            {
                ZipGenericExtraField field;

                while (ZipGenericExtraField.TryReadBlock(reader, extraFieldStream.Length, out field))
                {
                    Zip64ExtraField zip64Block;

                    if (Zip64ExtraField.TryGetZip64BlockFromGenericExtraField(field, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out zip64Block))
                    {
                        return zip64Block;
                    }
                }
            }

            return new Zip64ExtraField()
            {
                _compressedSize = new long?(),
                _uncompressedSize = new long?(),
                _localHeaderOffset = new long?(),
                _startDiskNumber = new int?()
            };
        }

        public static Zip64ExtraField GetAndRemoveZip64Block(List<ZipGenericExtraField> extraFields, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber)
        {
            Zip64ExtraField zip64Block = new Zip64ExtraField();
            zip64Block._compressedSize = new long?();
            zip64Block._uncompressedSize = new long?();
            zip64Block._localHeaderOffset = new long?();
            zip64Block._startDiskNumber = new int?();
            List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();
            bool flag = false;

            foreach (ZipGenericExtraField extraField in extraFields)
            {
                if ((int)extraField.Tag == 1)
                {
                    list.Add(extraField);

                    if (!flag && Zip64ExtraField.TryGetZip64BlockFromGenericExtraField(extraField, readUncompressedSize, readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out zip64Block))
                    {
                        flag = true;
                    }
                }
            }

            foreach (ZipGenericExtraField genericExtraField in list)
            {
                extraFields.Remove(genericExtraField);
            }

            return zip64Block;
        }

        public static void RemoveZip64Blocks(List<ZipGenericExtraField> extraFields)
        {
            List<ZipGenericExtraField> list = new List<ZipGenericExtraField>();

            foreach (ZipGenericExtraField genericExtraField in extraFields)
            {
                if ((int)genericExtraField.Tag == 1)
                {
                    list.Add(genericExtraField);
                }
            }

            foreach (ZipGenericExtraField genericExtraField in list)
            {
                extraFields.Remove(genericExtraField);
            }
        }

        public void WriteBlock(Stream stream)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);

            binaryWriter.Write((ushort)1);
            binaryWriter.Write(this._size);

            if (this._uncompressedSize.HasValue)
            {
                binaryWriter.Write(this._uncompressedSize.Value);
            }

            if (this._compressedSize.HasValue)
            {
                binaryWriter.Write(this._compressedSize.Value);
            }

            if (this._localHeaderOffset.HasValue)
            {
                binaryWriter.Write(this._localHeaderOffset.Value);
            }

            if (this._startDiskNumber.HasValue)
            {
                binaryWriter.Write(this._startDiskNumber.Value);
            }
        }

        private static bool TryGetZip64BlockFromGenericExtraField(ZipGenericExtraField extraField, bool readUncompressedSize, bool readCompressedSize, bool readLocalHeaderOffset, bool readStartDiskNumber, out Zip64ExtraField zip64Block)
        {
            zip64Block = new Zip64ExtraField();
            zip64Block._compressedSize = new long?();
            zip64Block._uncompressedSize = new long?();
            zip64Block._localHeaderOffset = new long?();
            zip64Block._startDiskNumber = new int?();

            if ((int)extraField.Tag != 1)
            {
                return false;
            }

            MemoryStream memoryStream = null;

            try
            {
                memoryStream = new MemoryStream(extraField.Data);

                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    memoryStream = null;
                    zip64Block._size = extraField.Size;
                    ushort num1 = (ushort)0;

                    if (readUncompressedSize)
                    {
                        num1 += (ushort)8;
                    }

                    if (readCompressedSize)
                    {
                        num1 += (ushort)8;
                    }

                    if (readLocalHeaderOffset)
                    {
                        num1 += (ushort)8;
                    }

                    if (readStartDiskNumber)
                    {
                        num1 += (ushort)4;
                    }

                    if ((int)num1 != (int)zip64Block._size)
                    {
                        return false;
                    }

                    if (readUncompressedSize)
                    {
                        zip64Block._uncompressedSize = new long?(binaryReader.ReadInt64());
                    }

                    if (readCompressedSize)
                    {
                        zip64Block._compressedSize = new long?(binaryReader.ReadInt64());
                    }

                    if (readLocalHeaderOffset)
                    {
                        zip64Block._localHeaderOffset = new long?(binaryReader.ReadInt64());
                    }

                    if (readStartDiskNumber)
                    {
                        zip64Block._startDiskNumber = new int?(binaryReader.ReadInt32());
                    }

                    long? nullable1 = zip64Block._uncompressedSize;
                    long num2 = 0L;

                    if ((nullable1.GetValueOrDefault() < num2 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                    {
                        throw new InvalidDataException(CompressionConstants.FieldTooBigUncompressedSize);
                    }

                    long? nullable2 = zip64Block._compressedSize;
                    long num3 = 0L;

                    if ((nullable2.GetValueOrDefault() < num3 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                    {
                        throw new InvalidDataException(CompressionConstants.FieldTooBigCompressedSize);
                    }

                    long? nullable3 = zip64Block._localHeaderOffset;
                    long num4 = 0L;

                    if ((nullable3.GetValueOrDefault() < num4 ? (nullable3.HasValue ? 1 : 0) : 0) != 0)
                    {
                        throw new InvalidDataException(CompressionConstants.FieldTooBigLocalHeaderOffset);
                    }

                    int? nullable4 = zip64Block._startDiskNumber;
                    int num5 = 0;

                    if ((nullable4.GetValueOrDefault() < num5 ? (nullable4.HasValue ? 1 : 0) : 0) != 0)
                    {
                        throw new InvalidDataException(CompressionConstants.FieldTooBigStartDiskNumber);
                    }

                    return true;
                }
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
            }
        }

        private void UpdateSize()
        {
            this._size = (ushort)0;

            if (this._uncompressedSize.HasValue)
            {
                this._size = (ushort)((uint)this._size + 8U);
            }

            if (this._compressedSize.HasValue)
            {
                this._size = (ushort)((uint)this._size + 8U);
            }

            if (this._localHeaderOffset.HasValue)
            {
                this._size = (ushort)((uint)this._size + 8U);
            }

            if (this._startDiskNumber.HasValue)
            {
                this._size = (ushort)((uint)this._size + 4U);
            }
        }
    }
}
