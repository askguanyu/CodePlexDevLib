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
    /// String Extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert Hex string to byte array.
        /// </summary>
        /// <param name="source">Hex string.</param>
        /// <returns>Byte array.</returns>
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
        /// <param name = "source">The input string.</param>
        /// <param name = "parameters">The parameters.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatWith(this string source, params object[] parameters)
        {
            return string.Format(source, parameters);
        }

        /// <summary>
        /// Remove any instance of the given character from the current string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="removeChar">Chars to remove.</param>
        /// <returns>Result string.</returns>
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
        /// Remove any instance of the given string from the current string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="removeString">String to remove.</param>
        /// <returns>Result string.</returns>
        public static string Remove(this string source, params string[] removeString)
        {
            return removeString.Aggregate(source, (current, c) => current.Replace(c, string.Empty));
        }

        /// <summary>
        /// Indicates whether the specified string is null or an System.String.Empty string.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>true if the <paramref name="source" /> parameter is null or <see cref="F:System.String.Empty" />, or if <paramref name="source" /> consists exclusively of white-space characters. </returns>
        public static bool IsNullOrWhiteSpace(this string source)
        {
            if (source == null)
            {
                return true;
            }

            for (int i = 0; i < source.Length; i++)
            {
                if (!char.IsWhiteSpace(source[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Extracts all digits from a string.
        /// </summary>
        /// <param name="source">String containing digits to extract.</param>
        /// <returns>All digits contained within the input string.</returns>
        public static string ExtractDigits(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return source.Where(char.IsDigit).Aggregate(new StringBuilder(source.Length), (stringBuilder, digi) => stringBuilder.Append(digi)).ToString();
        }

        /// <summary>
        /// Convert string to byte array.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="encoding">Instance of Encoding.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ToByteArray(this string source, Encoding encoding = null)
        {
            return (encoding ?? Encoding.Default).GetBytes(source);
        }

        /// <summary>
        /// Convert string to enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of <paramref name="returns"/> enum.</typeparam>
        /// <param name="source">Source string.</param>
        /// <param name="defaultValue">Default value of enum.</param>
        /// <param name="ignoreCase">Whether ignore case.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Instance of TEnum.</returns>
        public static TEnum ToEnum<TEnum>(this string source, TEnum defaultValue = default(TEnum), bool ignoreCase = false, bool throwOnError = false) where TEnum : struct
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), source, ignoreCase);
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Whether string is in enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="source">Source string.</param>
        /// <returns>true if string in enum; otherwise, false.</returns>
        public static bool IsItemInEnum<TEnum>(this string source) where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), source);
        }

        /// <summary>
        /// Base64 decodes a string.
        /// </summary>
        /// <param name="source">A base64 encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Base64Decode(this string source)
        {
            byte[] buffer = Convert.FromBase64String(source);

            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Base64 encodes a string.
        /// </summary>
        /// <param name="source">String to encode.</param>
        /// <returns>A base64 encoded string.</returns>
        public static string Base64Encode(this string source)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(source);

            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Retrieves left part substring from this instance. The substring ends at the first occurrence of the specified string position. If the specified string is not found, the return value is <see cref="F:System.String.Empty" />.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>A string equivalent to the substring that ends at the first occurrence of the specified string position.</returns>
        public static string LeftSubstringIndexOf(this string source, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return source;
            }

            int index = ignoreCase ? source.IndexOf(value, StringComparison.OrdinalIgnoreCase) : source.IndexOf(value);

            // The index position of the value parameter if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.
            if (index == -1 || index == 0)
            {
                return string.Empty;
            }

            return source.Substring(0, index);
        }

        /// <summary>
        /// Retrieves left part substring from this instance. The substring ends at the last occurrence of the specified string position. If the specified string is not found, the return value is <see cref="F:System.String.Empty" />.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>A string equivalent to the substring that ends at the last occurrence of the specified string position.</returns>
        public static string LeftSubstringLastIndexOf(this string source, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return source;
            }

            int index = ignoreCase ? source.LastIndexOf(value, StringComparison.OrdinalIgnoreCase) : source.LastIndexOf(value);

            // The index position of the value parameter if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.
            if (index == -1 || index == 0)
            {
                return string.Empty;
            }

            return source.Substring(0, index);
        }

        /// <summary>
        /// Retrieves right part substring from this instance. The substring starts at the first occurrence of the specified string position. If the specified string is not found, the return value is <see cref="F:System.String.Empty" />.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>A string equivalent to the substring that starts at the first occurrence of the specified string position.</returns>
        public static string RightSubstringIndexOf(this string source, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return source;
            }

            int index = ignoreCase ? source.IndexOf(value, StringComparison.OrdinalIgnoreCase) : source.IndexOf(value);

            // The index position of the value parameter if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.
            if (index == -1 || index == 0)
            {
                return string.Empty;
            }

            return source.Substring(index + 1);
        }

        /// <summary>
        /// Retrieves right part substring from this instance. The substring starts at the last occurrence of the specified string position. If the specified string is not found, the return value is <see cref="F:System.String.Empty" />.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>A string equivalent to the substring that starts at the last occurrence of the specified string position.</returns>
        public static string RightSubstringLastIndexOf(this string source, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return source;
            }

            int index = ignoreCase ? source.LastIndexOf(value, StringComparison.OrdinalIgnoreCase) : source.LastIndexOf(value);

            // The index position of the value parameter if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.
            if (index == -1 || index == 0)
            {
                return string.Empty;
            }

            return source.Substring(index + 1);
        }
    }
}
