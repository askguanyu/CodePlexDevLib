//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String Extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert Hex string to byte array
        /// </summary>
        /// <param name="source">Hex string</param>
        /// <returns>Byte array</returns>
        public static byte[] ToHexByteArray(this string source)
        {
            string hexPattern = "^[0-9a-fA-F]+$";

            string temp = source.Trim(new char[] { ' ', '\n', '\r' });

            if (!Regex.IsMatch(temp, hexPattern))
            {
                throw new ArgumentException(string.Format("\"{0}\" is not a Hex String.", source));
            }

            if (source.Length % 2 == 1)
            {
                temp = "0" + temp;
            }

            int resultLength = temp.Length / 2;
            byte[] result = new byte[resultLength];

            for (int i = 0; i < resultLength; i++)
            {
                result[i] = Convert.ToByte(temp.Substring(i * 2, 2), 16);
            }

            return result;
        }

        /// <summary>
        /// Formats the value with the parameters using string.Format.
        /// </summary>
        /// <param name = "source">The input string</param>
        /// <param name = "parameters">The parameters</param>
        /// <returns>The formated string</returns>
        public static string FormatWith(this string source, params object[] parameters)
        {
            return string.Format(source, parameters);
        }

        /// <summary>
        /// String is null or empty
        /// </summary>
        /// <param name="source">The input string</param>
        /// <returns>True if string is null or empty</returns>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Extracts all digits from a string
        /// </summary>
        /// <param name="source">String containing digits to extract</param>
        /// <returns>All digits contained within the input string</returns>
        public static string ExtractDigits(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return source.Where(Char.IsDigit).Aggregate(new StringBuilder(source.Length), (stringBuilder, digi) => stringBuilder.Append(digi)).ToString();
        }

        /// <summary>
        /// Convert string to byte array
        /// </summary>
        /// <param name="source">String</param>
        /// <returns>Byte array</returns>
        public static byte[] ToByteArray(this string source)
        {
            return Encoding.Default.GetBytes(source);
        }

        /// <summary>
        /// Convert string to byte array
        /// </summary>
        /// <param name="source">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Byte array</returns>
        public static byte[] ToByteArray(this string source, Encoding encoding)
        {
            return encoding.GetBytes(source);
        }

        /// <summary>
        /// Convert string to enum
        /// </summary>
        /// <param name="source">String</param>
        /// <param name="ignoreCase">Whether ignore case</param>
        /// <returns>Enum</returns>
        public static TEnum ToEnum<TEnum>(this string source, bool ignoreCase = false) where TEnum : struct
        {
            return source.IsItemInEnum<TEnum>()() ? default(TEnum) : (TEnum)Enum.Parse(typeof(TEnum), source, ignoreCase);
        }

        /// <summary>
        /// Whether string is in enum
        /// </summary>
        /// <param name="source">String</param>
        /// <returns>True if string in enum</returns>
        public static Func<bool> IsItemInEnum<TEnum>(this string source) where TEnum : struct
        {
            return () => { return string.IsNullOrEmpty(source) || !Enum.IsDefined(typeof(TEnum), source); };
        }
    }
}
