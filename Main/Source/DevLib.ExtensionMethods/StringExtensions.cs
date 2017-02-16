//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String Extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
        /// </summary>
        /// <param name = "source">A composite format string.</param>
        /// <param name = "args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
        /// </summary>
        /// <param name = "source">A composite format string.</param>
        /// <param name = "args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatInvariantCultureWith(this string source, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, source, args);
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
        /// </summary>
        /// <param name = "source">A composite format string.</param>
        /// <param name = "args">An object array that contains zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
        public static string FormatCurrentCultureWith(this string source, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, source, args);
        }

        /// <summary>
        /// Reports the indexes of all occurrence of the specified string in the current <see cref="T:System.String" /> object.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>The list of index positions of the value parameter if that string is found, or empty list if it is not. If value is System.String.Empty or null, the return value is empty list.</returns>
        public static List<int> AllIndexOf(this string source, string value, bool ignoreCase)
        {
            List<int> result = new List<int>();

            if (source == null || string.IsNullOrEmpty(value))
            {
                return result;
            }

            int valueLength = value.Length;
            int index = 0;

            if (ignoreCase)
            {
                while ((index = source.IndexOf(value, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result.Add(index);
                    index += valueLength;
                }

                return result;
            }
            else
            {
                while ((index = source.IndexOf(value, index)) >= 0)
                {
                    result.Add(index);
                    index += valueLength;
                }

                return result;
            }
        }

        /// <summary>
        /// Reports the indexes of all occurrence of the specified string in the current <see cref="T:System.String" /> object.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>The list of index positions of the value parameter if that string is found, or empty list if it is not. If value is System.String.Empty or null, the return value is empty list.</returns>
        public static List<int> AllIndexOf(this string source, char value, bool ignoreCase)
        {
            List<int> result = new List<int>();

            if (source == null)
            {
                return result;
            }

            int index = 0;

            if (ignoreCase)
            {
                string stringValue = value.ToString();

                while ((index = source.IndexOf(stringValue, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result.Add(index);
                    index += 1;
                }

                return result;
            }
            else
            {
                while ((index = source.IndexOf(value, index)) >= 0)
                {
                    result.Add(index);
                    index += 1;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified <see cref="T:System.String" /> object occurs within this string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <returns>true if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string ("") or null; otherwise, false.</returns>
        public static bool Contains(this string source, string value, bool ignoreCase)
        {
            if (source == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            if (ignoreCase)
            {
                return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                return source.Contains(value);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified <see cref="T:System.Char" /> object occurs within this string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the char to seek; otherwise, false.</param>
        /// <returns>true if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is null; otherwise, false.</returns>
        public static bool Contains(this string source, char value, bool ignoreCase)
        {
            if (source == null)
            {
                return false;
            }

            if (ignoreCase)
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return true;
                }

                return source.IndexOf(value.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                return source.Contains(value);
            }
        }

        /// <summary>
        /// Contains any instance of the given string from the current string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <param name="values">Strings to check.</param>
        /// <returns>true if the <paramref name="values" /> parameter occurs within this string, or if <paramref name="values" /> is null or empty; otherwise, false.</returns>
        public static bool ContainsAny(this string source, bool ignoreCase, params string[] values)
        {
            if (source == null)
            {
                return false;
            }

            if (values == null || values.Length < 1)
            {
                return true;
            }

            return values.Any(i => source.Contains(i, ignoreCase));
        }

        /// <summary>
        /// Contains any instance of the given string from the current string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="ignoreCase">true to ignore case when comparing the string to seek; otherwise, false.</param>
        /// <param name="values">Chars to check.</param>
        /// <returns>true if the <paramref name="values" /> parameter occurs within this string, or if <paramref name="values" /> is null or empty; otherwise, false.</returns>
        public static bool ContainsAny(this string source, bool ignoreCase, params char[] values)
        {
            if (source == null)
            {
                return false;
            }

            if (values == null || values.Length < 1)
            {
                return true;
            }

            return values.Any(i => source.Contains(i, ignoreCase));
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in this instance are replaced with another specified string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="oldValue">A string to be replaced.</param>
        /// <param name="newValue">A string to replace all occurrences of oldValue.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <returns>A System.String equivalent to this instance but with all instances of oldValue replaced with newValue.</returns>
        public static string Replace(this string source, string oldValue, string newValue, bool ignoreCase)
        {
            if (source == null || oldValue == null || newValue == null)
            {
                return source;
            }

            if (!ignoreCase)
            {
                return source.Replace(oldValue, newValue);
            }
            else
            {
                string result = source;

                int oldValueLength = oldValue.Length;
                int newValueLength = newValue.Length;
                int index = 0;

                while ((index = result.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result = result.Remove(index, oldValueLength).Insert(index, newValue);
                    index += newValueLength;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified char in this instance are replaced with another specified char.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="oldValue">A Unicode character to be replaced.</param>
        /// <param name="newValue">A Unicode character to replace all occurrences of oldChar.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <returns>A System.String equivalent to this instance but with all instances of oldValue replaced with newValue.</returns>
        public static string Replace(this string source, char oldValue, char newValue, bool ignoreCase)
        {
            if (source == null)
            {
                return source;
            }

            if (!ignoreCase)
            {
                return source.Replace(oldValue, newValue);
            }
            else
            {
                string result = source;
                string oldString = oldValue.ToString();
                int oldValueLength = oldString.Length;
                string newString = newValue.ToString();
                int newValueLength = newString.Length;
                int index = 0;

                while ((index = result.IndexOf(oldString, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result = result.Remove(index, oldValueLength).Insert(index, newString);
                    index += newValueLength;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified strings in this instance are replaced with another specified string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="newValue">A string to replace all occurrences of oldValues.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <param name="oldValues">A list of string to be replaced.</param>
        /// <returns>A System.String equivalent to this instance but with all instances of oldValues replaced with newValue.</returns>
        public static string ReplaceAny(this string source, string newValue, bool ignoreCase, params string[] oldValues)
        {
            if (source == null || newValue == null || oldValues == null || oldValues.Length < 1)
            {
                return source;
            }

            string result = source;

            foreach (string oldValue in oldValues)
            {
                result = result.Replace(oldValue, newValue, ignoreCase);
            }

            return result;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified chars in this instance are replaced with another specified char.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="newValue">A char to replace all occurrences of oldValues.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <param name="oldValues">A list of char to be replaced.</param>
        /// <returns>A System.String equivalent to this instance but with all instances of oldValues replaced with newValue.</returns>
        public static string ReplaceAny(this string source, char newValue, bool ignoreCase, params char[] oldValues)
        {
            if (source == null || oldValues == null || oldValues.Length < 1)
            {
                return source;
            }

            string result = source;

            foreach (char oldValue in oldValues)
            {
                result = result.Replace(oldValue, newValue, ignoreCase);
            }

            return result;
        }

        /// <summary>
        /// Deletes all the string from this string beginning at a specified position and continuing through the last position.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">A string to be removed.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <returns>A new System.String object that is equivalent to this string less the removed characters.</returns>
        public static string Remove(this string source, string value, bool ignoreCase)
        {
            if (source == null || value == null)
            {
                return source;
            }

            if (!ignoreCase)
            {
                return source.Replace(value, string.Empty);
            }
            else
            {
                string result = source;

                int valueLength = value.Length;
                int index = 0;

                while ((index = result.IndexOf(value, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result = result.Remove(index, valueLength);
                }

                return result;
            }
        }

        /// <summary>
        /// Deletes all the character from this string beginning at a specified position and continuing through the last position.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">A Unicode character to be removed.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <returns>A new System.String object that is equivalent to this string less the removed characters.</returns>
        public static string Remove(this string source, char value, bool ignoreCase)
        {
            if (source == null)
            {
                return source;
            }

            if (!ignoreCase)
            {
                return source.Replace(value.ToString(), string.Empty);
            }
            else
            {
                string result = source;
                string valueString = value.ToString();
                int valueLength = valueString.Length;
                int index = 0;

                while ((index = result.IndexOf(valueString, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    result = result.Remove(index, valueLength);
                }

                return result;
            }
        }

        /// <summary>
        /// Deletes all the string from this string beginning at a specified position and continuing through the last position.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <param name="values">A list of string to be removed.</param>
        /// <returns>A new System.String object that is equivalent to this string less the removed characters.</returns>
        public static string RemoveAny(this string source, bool ignoreCase, params string[] values)
        {
            if (source == null || values == null || values.Length < 1)
            {
                return source;
            }

            string result = source;

            foreach (string value in values)
            {
                result = result.Remove(value, ignoreCase);
            }

            return result;
        }

        /// <summary>
        /// Deletes all the characters from this string beginning at a specified position and continuing through the last position.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        /// <param name="values">A list of char to be removed.</param>
        /// <returns>A new System.String object that is equivalent to this string less the removed characters.</returns>
        public static string RemoveAny(this string source, bool ignoreCase, params char[] values)
        {
            if (source == null || values == null || values.Length < 1)
            {
                return source;
            }

            string result = source;

            foreach (char value in values)
            {
                result = result.Remove(value, ignoreCase);
            }

            return result;
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
        /// Indicates whether the specified string is neither null nor an System.String.Empty string.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>true if the value parameter is neither null nor an empty string (""); otherwise, false.</returns>
        public static bool IsNotNullNorEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Indicates whether a specified string is neither null, empty, nor consists only of white-space characters.
        /// </summary>
        /// <param name="source">The string to test.</param>
        /// <returns>true if the <paramref name="source" /> parameter is neither null nor <see cref="F:System.String.Empty" />, or if <paramref name="source" /> consists exclusively of white-space characters. </returns>
        public static bool IsNotNullNorWhiteSpace(this string source)
        {
            return !source.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Extracts all digits from a string.
        /// </summary>
        /// <param name="source">String containing digits to extract.</param>
        /// <returns>All digits contained within the input string.</returns>
        public static string ExtractDigits(this string source)
        {
            return source.ExtractChars(char.IsDigit);
        }

        /// <summary>
        /// Extracts all letters from a string.
        /// </summary>
        /// <param name="source">String containing letters to extract.</param>
        /// <returns>All letters contained within the input string.</returns>
        public static string ExtractLetters(this string source)
        {
            return source.ExtractChars(char.IsLetter);
        }

        /// <summary>
        /// Extracts all symbols from a string.
        /// </summary>
        /// <param name="source">String containing symbols to extract.</param>
        /// <returns>All symbols contained within the input string.</returns>
        public static string ExtractSymbols(this string source)
        {
            return source.ExtractChars(char.IsSymbol);
        }

        /// <summary>
        /// Extracts all control chars from a string.
        /// </summary>
        /// <param name="source">String containing control chars to extract.</param>
        /// <returns>All control chars contained within the input string.</returns>
        public static string ExtractControlChars(this string source)
        {
            return source.ExtractChars(char.IsControl);
        }

        /// <summary>
        /// Extracts all letters and digits from a string.
        /// </summary>
        /// <param name="source">String containing letters and digits to extract.</param>
        /// <returns>All letters and digits contained within the input string.</returns>
        public static string ExtractLettersDigits(this string source)
        {
            return source.ExtractChars(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Extracts all chars satisfy the condition from a string.
        /// </summary>
        /// <param name="source">String containing satisfied chars to extract.</param>
        /// <param name="predicate">A function to test char for a condition.</param>
        /// <returns>All satisfied chars contained within the input string.</returns>
        public static string ExtractChars(this string source, Func<char, bool> predicate)
        {
            if (source.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return source.Where(predicate).Aggregate(new StringBuilder(source.Length), (stringBuilder, item) => stringBuilder.Append(item)).ToString();
        }

        /// <summary>
        /// Convert string to byte array.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="encoding">Instance of Encoding.</param>
        /// <returns>Byte array.</returns>
        public static byte[] ToByteArray(this string source, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetBytes(source);
        }

        /// <summary>
        /// Base64 decodes a string.
        /// </summary>
        /// <param name="source">A base64 encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Base64Decode(this string source)
        {
            byte[] buffer = Convert.FromBase64String(source);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Base64 encodes a string.
        /// </summary>
        /// <param name="source">String to encode.</param>
        /// <returns>A base64 encoded string.</returns>
        public static string Base64Encode(this string source)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(source);

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

        /// <summary>
        /// Remove any invalid characters from Xml string and returns a clean Xml string.
        /// </summary>
        /// <param name="source">Xml string to check.</param>
        /// <returns>Clean Xml string.</returns>
        public static string ToCleanXmlString(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return new string(
                source.Where(p =>
                        (p >= 0x0020 && p <= 0xD7FF)
                        || (p >= 0xE000 && p <= 0xFFFD)
                        || p == 0x0009
                        || p == 0x000A
                        || p == 0x000D)
                      .ToArray());
        }

        /// <summary>
        /// Retrieves a truncated string from this instance.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="maxLength">Maximum length to truncate.</param>
        /// <returns>A truncated string.</returns>
        public static string Truncate(this string source, int maxLength)
        {
            if (string.IsNullOrEmpty(source) || source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);
        }

        /// <summary>
        /// Splits string by a specified delimiter and keep nested string with a specified qualifier.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="delimiter">Delimiter character.</param>
        /// <param name="qualifier">Qualifier character.</param>
        /// <returns>A list whose elements contain the substrings in this instance that are delimited by the delimiter.</returns>
        public static List<string> SplitNested(this string source, char delimiter = ' ', char qualifier = '"')
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<string>();
            }

            StringBuilder itemStringBuilder = new StringBuilder();
            List<string> result = new List<string>();
            bool inItem = false;
            bool inQuotes = false;

            for (int i = 0; i < source.Length; i++)
            {
                char character = source[i];

                if (!inItem)
                {
                    if (character == delimiter)
                    {
                        result.Add(string.Empty);
                        continue;
                    }

                    if (character == qualifier)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        itemStringBuilder.Append(character);
                    }

                    inItem = true;
                    continue;
                }

                if (inQuotes)
                {
                    if (character == qualifier && ((source.Length > (i + 1) && source[i + 1] == delimiter) || ((i + 1) == source.Length)))
                    {
                        inQuotes = false;
                        inItem = false;
                        i++;
                    }
                    else if (character == qualifier && source.Length > (i + 1) && source[i + 1] == qualifier)
                    {
                        i++;
                    }
                }
                else if (character == delimiter)
                {
                    inItem = false;
                }

                if (!inItem)
                {
                    result.Add(itemStringBuilder.ToString());
                    itemStringBuilder.Length = 0;
                }
                else
                {
                    itemStringBuilder.Append(character);
                }
            }

            if (inItem)
            {
                result.Add(itemStringBuilder.ToString());
            }

            return result;
        }

        /// <summary>
        /// Splits string by a specified delimiter and keep nested string with a specified qualifier.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="delimiters">Delimiter characters.</param>
        /// <param name="qualifier">Qualifier character.</param>
        /// <returns>A list whose elements contain the substrings in this instance that are delimited by the delimiter.</returns>
        public static List<string> SplitNested(this string source, char[] delimiters, char qualifier = '"')
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<string>();
            }

            StringBuilder itemStringBuilder = new StringBuilder();
            List<string> result = new List<string>();
            bool inItem = false;
            bool inQuotes = false;

            for (int i = 0; i < source.Length; i++)
            {
                char character = source[i];

                if (!inItem)
                {
                    if (delimiters.Contains(character))
                    {
                        result.Add(string.Empty);
                        continue;
                    }

                    if (character == qualifier)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        itemStringBuilder.Append(character);
                    }

                    inItem = true;
                    continue;
                }

                if (inQuotes)
                {
                    if (character == qualifier && ((source.Length > (i + 1) && delimiters.Contains(source[i + 1])) || ((i + 1) == source.Length)))
                    {
                        inQuotes = false;
                        inItem = false;
                        i++;
                    }
                    else if (character == qualifier && source.Length > (i + 1) && source[i + 1] == qualifier)
                    {
                        i++;
                    }
                }
                else if (delimiters.Contains(character))
                {
                    inItem = false;
                }

                if (!inItem)
                {
                    result.Add(itemStringBuilder.ToString());
                    itemStringBuilder.Length = 0;
                }
                else
                {
                    itemStringBuilder.Append(character);
                }
            }

            if (inItem)
            {
                result.Add(itemStringBuilder.ToString());
            }

            return result;
        }

        /// <summary>
        /// Word wrap text for a specified maximum line length.
        /// </summary>
        /// <param name="source">Text to word wrap.</param>
        /// <param name="maxLineLength">Maximum length of a line.</param>
        /// <returns>A list of lines for the word wrapped text.</returns>
        public static List<string> WordWrap(this string source, int maxLineLength = 80)
        {
            List<string> result = new List<string>();

            string currentLine = string.Empty;

            foreach (string word in source.Split(' '))
            {
                if (currentLine.Length + word.Length > maxLineLength)
                {
                    result.Add(currentLine);
                    currentLine = string.Empty;
                }

                currentLine += word;

                if (currentLine.Length != maxLineLength)
                {
                    currentLine += " ";
                }
            }

            if (!string.IsNullOrEmpty(currentLine.Trim()))
            {
                result.Add(currentLine);
            }

            return result;
        }

        /// <summary>
        /// Concatenates the members of a collection of string, using the specified separator between each member.
        /// </summary>
        /// <param name="source">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator. separator is included in the returned string only if values has more than one element.</param>
        /// <returns>A string that consists of the members of values delimited by the separator string. If values has no members, the method returns System.String.Empty.</returns>
        public static string JoinBy(this IEnumerable<string> source, string separator)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (separator == null)
            {
                separator = string.Empty;
            }

            using (IEnumerator<string> item = source.GetEnumerator())
            {
                if (!item.MoveNext())
                {
                    return string.Empty;
                }

                StringBuilder result = new StringBuilder();

                if (item.Current != null)
                {
                    result.Append(item.Current);
                }

                while (item.MoveNext())
                {
                    result.Append(separator);

                    if (item.Current != null)
                    {
                        result.Append(item.Current);
                    }
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Escapes a minimal set of characters (\, *, +, ?, |, {, [, (,), ^, $,., #, and white space) by replacing them with their escape codes. This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.
        /// </summary>
        /// <param name="source">The input string that contains the text to convert.</param>
        /// <returns>A string of characters with metacharacters converted to their escaped form.</returns>
        public static string Escape(this string source)
        {
            return Regex.Escape(source);
        }

        /// <summary>
        /// Converts any escaped characters in the source string.
        /// </summary>
        /// <param name="source">The input string containing the text to convert.</param>
        /// <returns>A string of characters with any escaped characters converted to their unescaped form.</returns>
        public static string Unescape(this string source)
        {
            return Regex.Unescape(source);
        }

        /// <summary>
        /// Gets empty string if string is null or empty.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>Empty string if source string is null or empty; otherwise, source string.</returns>
        public static string EmptyIfNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source) ? string.Empty : source;
        }

        /// <summary>
        /// Gets empty string if string is null or white space.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>Empty string if source string is null or white space; otherwise, source string.</returns>
        public static string EmptyIfNullOrWhiteSpace(this string source)
        {
            return source.IsNullOrWhiteSpace() ? string.Empty : source;
        }

        /// <summary>
        /// Gets null if string is null or empty.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>Null if source string is null or empty; otherwise, source string.</returns>
        public static string NullIfNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source) ? null : source;
        }

        /// <summary>
        /// Gets null if string is null or white space.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>Null if source string is null or white space; otherwise, source string.</returns>
        public static string NullIfNullOrWhiteSpace(this string source)
        {
            return source.IsNullOrWhiteSpace() ? null : source;
        }
    }
}
