//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
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

            string temp = source.Remove(' ', '\n', '\r');

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
        /// Remove any instance of the given character from the current string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="removeChar"></param>
        /// <returns></returns>
        public static string Remove(this string source, params char[] removeChar)
        {
            var result = source;

            if (!string.IsNullOrEmpty(result) && removeChar != null)
            {
                Array.ForEach(removeChar, c => result = result.Remove(c.ToString()));
            }

            return result;
        }

        /// <summary>
        /// Remove any instance of the given string from the current string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="removeString"></param>
        /// <returns></returns>
        public static string Remove(this string source, params string[] removeString)
        {
            return removeString.Aggregate(source, (current, c) => current.Replace(c, string.Empty));
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

            return source.Where(char.IsDigit).Aggregate(new StringBuilder(source.Length), (stringBuilder, digi) => stringBuilder.Append(digi)).ToString();
        }

        /// <summary>
        /// Convert string to byte array by using Encoding.Default
        /// </summary>
        /// <param name="source">String</param>
        /// <returns>Byte array by using Encoding.Default</returns>
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
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="source">String</param>
        /// <param name="defaultValue">Default value of enum</param>
        /// <param name="ignoreCase">Whether ignore case</param>
        /// <param name="ignoreException">Whether ignore exception</param>
        /// <returns>Enum</returns>
        public static TEnum ToEnum<TEnum>(this string source, TEnum defaultValue = default(TEnum), bool ignoreCase = false, bool ignoreException = true) where TEnum : struct
        {
            TEnum result;

            if (ignoreException)
            {
                if (Enum.TryParse<TEnum>(source, ignoreCase, out result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                if (Enum.TryParse<TEnum>(source, ignoreCase, out result))
                {
                    return result;
                }
                else
                {
                    throw new ArgumentException("The source is not a item of Enum");
                }
            }
        }

        /// <summary>
        /// Whether string is in enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="source">String</param>
        /// <returns>True if string in enum</returns>
        public static bool IsItemInEnum<TEnum>(this string source) where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), source);
        }

        /// <summary>
        /// Base64 decodes a string
        /// </summary>
        /// <param name="source">A base64 encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Base64Decode(this string source)
        {
            byte[] buffer = Convert.FromBase64String(source);

            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Base64 encodes a string
        /// </summary>
        /// <param name="source">String to encode</param>
        /// <returns>A base64 encoded string</returns>
        public static string Base64Encode(this string source)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(source);

            return Convert.ToBase64String(buffer);
        }
    }
}
