//-----------------------------------------------------------------------
// <copyright file="ZipHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Compression
{
    using System;
    using System.IO;

    internal static class ZipHelper
    {
        internal const uint Mask32Bit = 4294967295U;

        internal const ushort Mask16Bit = (ushort)65535;

        internal const int ValidZipDate_YearMin = 1980;

        internal const int ValidZipDate_YearMax = 2107;

        private const int BackwardsSeekingBufferSize = 32;

        private static readonly DateTime InvalidDateIndicator = new DateTime(1980, 1, 1, 0, 0, 0);

        internal static bool EndsWithDirChar(string test)
        {
            return Path.GetFileName(test) == string.Empty;
        }

        internal static bool IsDirEmpty(DirectoryInfo possiblyEmptyDir)
        {
            return possiblyEmptyDir.GetFileSystemInfos("*").Length == 0;
        }

        internal static bool RequiresUnicode(string test)
        {
            foreach (int num in test)
            {
                if (num > (int)sbyte.MaxValue)
                {
                    return true;
                }
            }

            return false;
        }

        internal static void ReadBytes(Stream stream, byte[] buffer, int bytesToRead)
        {
            int count = bytesToRead;
            int offset = 0;

            while (count > 0)
            {
                int num = stream.Read(buffer, offset, count);

                if (num == 0)
                {
                    throw new IOException(CompressionConstants.UnexpectedEndOfStream);
                }

                offset += num;
                count -= num;
            }
        }

        internal static DateTime DosTimeToDateTime(uint dateTime)
        {
            int year = 1980 + (int)(dateTime >> 25);
            int month = (int)(dateTime >> 21) & 15;
            int day = (int)(dateTime >> 16) & 31;
            int hour = (int)(dateTime >> 11) & 31;
            int minute = (int)(dateTime >> 5) & 63;
            int second = ((int)dateTime & 31) * 2;
            try
            {
                return new DateTime(year, month, day, hour, minute, second, 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ZipHelper.InvalidDateIndicator;
            }
            catch (ArgumentException)
            {
                return ZipHelper.InvalidDateIndicator;
            }
        }

        internal static uint DateTimeToDosTime(DateTime dateTime)
        {
            int num = dateTime.Year - 1980 & 127;
            num = (num << 4) + dateTime.Month;
            num = (num << 5) + dateTime.Day;
            num = (num << 5) + dateTime.Hour;
            num = (num << 6) + dateTime.Minute;
            return (uint)((num << 5) + (dateTime.Second / 2));
        }

        internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind)
        {
            int num = 0;
            uint num2 = 0u;
            byte[] array = new byte[32];
            bool flag = false;
            bool flag2 = false;

            while (!flag2 && !flag)
            {
                flag = ZipHelper.SeekBackwardsAndRead(stream, array, out num);
                while (num >= 0 && !flag2)
                {
                    num2 = num2 << 8 | (uint)array[num];
                    if (num2 == signatureToFind)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        num--;
                    }
                }
            }

            if (!flag2)
            {
                return false;
            }

            stream.Seek((long)num, SeekOrigin.Current);

            return true;
        }

        private static bool SeekBackwardsAndRead(Stream stream, byte[] buffer, out int bufferPointer)
        {
            if (stream.Position >= (long)buffer.Length)
            {
                stream.Seek((long)-buffer.Length, SeekOrigin.Current);
                ZipHelper.ReadBytes(stream, buffer, buffer.Length);
                stream.Seek((long)-buffer.Length, SeekOrigin.Current);
                bufferPointer = buffer.Length - 1;
                return false;
            }
            else
            {
                int bytesToRead = (int)stream.Position;
                stream.Seek(0L, SeekOrigin.Begin);
                ZipHelper.ReadBytes(stream, buffer, bytesToRead);
                stream.Seek(0L, SeekOrigin.Begin);
                bufferPointer = bytesToRead - 1;
                return true;
            }
        }
    }
}
