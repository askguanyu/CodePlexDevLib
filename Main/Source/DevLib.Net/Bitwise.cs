//-----------------------------------------------------------------------
// <copyright file="Bitwise.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Bitwise operation utility class.
    /// </summary>
    internal static class Bitwise
    {
        /// <summary>
        /// NOT operator.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>The result bytes.</returns>
        public static byte[] Not(byte[] bytes)
        {
            byte[] result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)~bytes[i];
            }

            return result;
        }

        /// <summary>
        /// AND operator.
        /// </summary>
        /// <param name="bytesLeft">The left source bytes.</param>
        /// <param name="bytesRight">The right source bytes.</param>
        /// <returns>The result bytes.</returns>
        public static byte[] And(byte[] bytesLeft, byte[] bytesRight)
        {
            int length = Math.Min(bytesLeft.Length, bytesRight.Length);

            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(bytesLeft[i] & bytesRight[i]);
            }

            return result;
        }

        /// <summary>
        /// OR operator.
        /// </summary>
        /// <param name="bytesLeft">The left source bytes.</param>
        /// <param name="bytesRight">The right source bytes.</param>
        /// <returns>The result bytes.</returns>
        public static byte[] Or(byte[] bytesLeft, byte[] bytesRight)
        {
            int length = Math.Min(bytesLeft.Length, bytesRight.Length);

            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(bytesLeft[i] | bytesRight[i]);
            }

            return result;
        }

        /// <summary>
        /// Greater or Equal operator.
        /// </summary>
        /// <param name="bytesLeft">The left source bytes.</param>
        /// <param name="bytesRight">The right source bytes.</param>
        /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
        public static bool GreaterEqual(byte[] bytesLeft, byte[] bytesRight)
        {
            int length = Math.Min(bytesLeft.Length, bytesRight.Length);

            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = bytesLeft[i] == bytesRight[i] ? 0 : bytesLeft[i] < bytesRight[i] ? 1 : -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (result[i] != 0)
                {
                    return result[i] >= 0;
                }
                else
                {
                    continue;
                }
            }

            return true;
        }

        /// <summary>
        /// Less or Equal operator.
        /// </summary>
        /// <param name="bytesLeft">The left source bytes.</param>
        /// <param name="bytesRight">The right source bytes.</param>
        /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
        public static bool LessEqual(byte[] bytesLeft, byte[] bytesRight)
        {
            int length = Math.Min(bytesLeft.Length, bytesRight.Length);

            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = bytesLeft[i] == bytesRight[i] ? 0 : bytesLeft[i] < bytesRight[i] ? 1 : -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (result[i] != 0)
                {
                    return result[i] <= 0;
                }
                else
                {
                    continue;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the bit mask.
        /// </summary>
        /// <param name="sizeOfBuffer">The size of buffer.</param>
        /// <param name="bitLength">The length of the bit.</param>
        /// <returns>The bit mask bytes.</returns>
        public static byte[] GetBitMask(int sizeOfBuffer, int bitLength)
        {
            var maskBytes = new byte[sizeOfBuffer];
            var bytesLength = bitLength / 8;
            var bitsLength = bitLength % 8;

            for (var i = 0; i < bytesLength; i++)
            {
                maskBytes[i] = 0xff;
            }

            if (bitsLength > 0)
            {
                byte[] range = new byte[8 - bitsLength];

                for (int i = 1; i <= range.Length; i++)
                {
                    range[i - 1] = (byte)(1 << i - 1);
                }

                int result = range[0];

                for (int i = 1; i < range.Length; i++)
                {
                    result = result | range[i];
                }

                maskBytes[bytesLength] = (byte)~result;
            }

            return maskBytes;
        }

        /// <summary>
        /// Gets the length of the bit mask.
        /// </summary>
        /// <param name="bytes">The bit mask bytes.</param>
        /// <returns>The the length of the bit mask.</returns>
        public static int? GetBitMaskLength(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            var bitLength = 0;
            var index = 0;

            for (; index < bytes.Length && bytes[index] == 0xff; index++) ;

            bitLength = 8 * index;

            if (index < bytes.Length)
            {
                switch (bytes[index])
                {
                    case 0xFE: bitLength += 7;
                        break;
                    case 0xFC: bitLength += 6;
                        break;
                    case 0xF8: bitLength += 5;
                        break;
                    case 0xF0: bitLength += 4;
                        break;
                    case 0xE0: bitLength += 3;
                        break;
                    case 0xC0: bitLength += 2;
                        break;
                    case 0x80: bitLength += 1;
                        break;
                    case 0x00:
                        break;
                    default:
                        return null;
                }

                for (int i = index + 1; i < bytes.Length; i++)
                {
                    if (bytes[i] != 0x00)
                    {
                        return null;
                    }
                }
            }

            return bitLength;
        }

        /// <summary>
        /// Increments the specified bytes.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <returns>New bytes.</returns>
        public static byte[] Increment(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            var incrementIndex = Array.FindLastIndex(bytes, x => x < byte.MaxValue);

            if (incrementIndex < 0)
            {
                throw new OverflowException();
            }

            var result2 = new byte[] { (byte)(bytes[incrementIndex] + 1) };
            var result3 = new byte[bytes.Length - incrementIndex - 1];

            var result1 = new byte[incrementIndex + result2.Length + result3.Length];

            Array.Copy(bytes, 0, result1, 0, incrementIndex);
            Array.Copy(result2, 0, result1, incrementIndex, result2.Length);
            Array.Copy(result3, 0, result1, incrementIndex + result2.Length, result3.Length);

            return result1;
        }
    }
}
