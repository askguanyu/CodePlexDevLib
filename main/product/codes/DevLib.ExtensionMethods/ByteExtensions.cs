//-----------------------------------------------------------------------
// <copyright file="ByteExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Byte Extensions
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Convert bytes to Hex string
        /// </summary>
        /// <param name="data">bytes</param>
        /// <param name="addSpace">Whether add space between Hex</param>
        /// <returns>Hex string</returns>
        public static string ToHexString(this byte[] data, bool addSpace = true)
        {
            StringBuilder result = new StringBuilder(data.Length * 2);

            if (addSpace)
            {
                foreach (byte hex in data)
                {
                    result.AppendFormat("{0:X2}", hex);
                    result.Append(" ");
                }
            }
            else
            {
                foreach (byte hex in data)
                {
                    result.AppendFormat("{0:X2}", hex);
                }
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// Convert byte to Hex char
        /// </summary>
        /// <param name="data">byte</param>
        /// <returns>Hex char</returns>
        public static string ToHexString(this byte data)
        {
            return Convert.ToString(data, 16).PadLeft(2, '0');
        }
    }
}
