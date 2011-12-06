//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String Extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert Hex string to bytes
        /// </summary>
        /// <param name="data">Hex string</param>
        /// <returns>bytes</returns>
        public static byte[] ToHexByte(this string data)
        {
            string hexPattern = "^[0-9a-fA-F]+$";

            string temp = data.Trim(new char[] { ' ', '\n', '\r' });

            if (Regex.IsMatch(temp, hexPattern))
            {
                if (data.Length % 2 == 1)
                {
                    temp = "0" + temp;
                }

                int length = temp.Length / 2;
                byte[] result = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    result[i] = Convert.ToByte(temp.Substring(i * 2, 2), 16);
                }

                return result;
            }
            else
            {
                throw new ArgumentException(string.Format("\"{0}\" is not a Hex String.", data));
            }
        }
    }
}
