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
        /// <param name="value">Hex string</param>
        /// <returns>bytes</returns>
        public static byte[] ToHexByte(this string value)
        {
            string hexPattern = "^[0-9a-fA-F]+$";

            string temp = value.Trim(new char[] { ' ', '\n', '\r' });

            if (Regex.IsMatch(temp, hexPattern))
            {
                if (value.Length % 2 == 1)
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
                throw new ArgumentException(string.Format("\"{0}\" is not a Hex String.", value));
            }
        }

        /// <summary>
        /// Formats the value with the parameters using string.Format.
        /// </summary>
        /// <param name = "value">The input string</param>
        /// <param name = "parameters">The parameters</param>
        /// <returns>The formated string</returns>
        public static string FormatWith(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }

        /// <summary>
        /// String is null or empty
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns>True if string is null or empty</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Invoke Console.WriteLine
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns>The input string</returns>
        public static string ConsoleWriteLine(this string value)
        {
            Console.WriteLine(value);
            return value;
        }
    }
}
