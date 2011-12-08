//-----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Text;

    /// <summary>
    /// Byte Extensions
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Convert bytes to Hex string
        /// </summary>
        /// <param name="value">bytes</param>
        /// <param name="addSpace">Whether add space between Hex</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(this byte[] value, bool addSpace = true)
        {
            StringBuilder result = new StringBuilder(value.Length * 2);

            if (addSpace)
            {
                foreach (byte hex in value)
                {
                    result.AppendFormat("{0:X2}", hex);
                    result.Append(" ");
                }
            }
            else
            {
                foreach (byte hex in value)
                {
                    result.AppendFormat("{0:X2}", hex);
                }
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// Convert byte to Hex string
        /// </summary>
        /// <param name="value">byte</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(this byte value)
        {
            return Convert.ToString(value, 16).PadLeft(2, '0');
        }

        /// <summary>
        /// Convert bytes to Encoding string
        /// </summary>
        /// <param name="value">bytes</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Encoding string</returns>
        public static string ToEncodingString(this byte[] value, Encoding encoding)
        {
            return encoding.GetString(value);
        }
    }
}
