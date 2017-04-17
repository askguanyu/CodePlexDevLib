//-----------------------------------------------------------------------
// <copyright file="TextExtensions.cs" company="Yu Guan Corporation">
//     Copyright (c) Yu Guan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web.Security;

    /// <summary>
    /// Text Extensions.
    /// </summary>
    public static class TextExtensions
    {
        /// <summary>
        /// The Base 15 chars set.
        /// 123456789ABCDEF
        /// </summary>
        public const string Base15Chars = "123456789ABCDEF";

        /// <summary>
        /// The Base 26 chars set.
        /// 0123456789ABCDEF
        /// </summary>
        public const string Base16Chars = "0123456789ABCDEF";

        /// <summary>
        /// The Base 26 chars set.
        /// ABCDEFGHIJKLMNOPQRSTUVWXYZ
        /// </summary>
        public const string Base26Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// The Base 35 chars set.
        /// 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ
        /// </summary>
        public const string Base35Chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// The Base 36 chars set.
        /// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ
        /// </summary>
        public const string Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// The Base 52 chars set.
        /// ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
        /// </summary>
        public const string Base52Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The Base 61 chars set.
        /// 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
        /// </summary>
        public const string Base61Chars = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The Base 62 chars set.
        /// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
        /// </summary>
        public const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The alphabet and numeric chars.
        /// </summary>
        private static readonly string[] AlphaNumericChars = new string[]
        {
            "a", "b", "c", "d", "e", "f", "g",
            "h", "i", "j", "k", "l", "m", "n",
            "o", "p", "q", "r", "s", "t",
            "u", "v", "w", "x", "y", "z",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "A", "B", "C", "D", "E", "F", "G",
            "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        };

        /// <summary>
        /// Field CP1252Encoding.
        /// </summary>
        private static volatile Encoding CP1252Encoding;

        /// <summary>
        /// Gets an encoding for the CP1252 (Windows-1252) character set.
        /// </summary>
        public static Encoding CP1252
        {
            get
            {
                if (CP1252Encoding == null)
                {
                    CP1252Encoding = Encoding.GetEncoding(1252);
                }

                return CP1252Encoding;
            }
        }

        /// <summary>
        /// Gets an encoding for the CP1252 (Windows-1252) character set.
        /// </summary>
        /// <param name="source">Any object.</param>
        /// <returns>Encoding instance for the CP1252 (Windows-1252) character set.</returns>
        public static Encoding GetCP1252Encoding(this object source)
        {
            return TextExtensions.CP1252;
        }

        /// <summary>
        /// Shortens the specified source string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>4 shorten string candidates in string array.</returns>
        public static string[] Shorten(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new[] { source, source, source, source };
            }

            string hex = FormsAuthentication.HashPasswordForStoringInConfigFile(source, "md5");

            string[] result = new string[4];

            for (int i = 0; i < 4; i++)
            {
                int hexInt = 0x3FFFFFFF & Convert.ToInt32("0x" + hex.Substring(i * 8, 8), 16);

                string outChars = string.Empty;

                for (int j = 0; j < 6; j++)
                {
                    int index = 0x0000003D & hexInt;
                    outChars += AlphaNumericChars[index];
                    hexInt = hexInt >> 5;
                }

                result[i] = outChars;
            }

            return result;
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
        /// Encodes to 123456789ABCDEF chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base15Encode(this int source)
        {
            return ((long)source).Base15Encode();
        }

        /// <summary>
        /// Encodes to 123456789ABCDEF chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base15Encode(this long source)
        {
            return source.BaseEncode(Base15Chars);
        }

        /// <summary>
        /// Decodes 123456789ABCDEF Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base15Decode(this string source)
        {
            return source.BaseDecode(Base15Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEF chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base16Encode(this int source)
        {
            return ((long)source).Base16Encode();
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEF chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base16Encode(this long source)
        {
            return source.BaseEncode(Base16Chars);
        }

        /// <summary>
        /// Decodes 0123456789ABCDEF Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base16Decode(this string source)
        {
            return source.BaseDecode(Base16Chars);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base26Encode(this int source)
        {
            return ((long)source).Base26Encode();
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base26Encode(this long source)
        {
            return source.BaseEncode(Base26Chars);
        }

        /// <summary>
        /// Decodes ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base26Decode(this string source)
        {
            return source.BaseDecode(Base26Chars);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base35Encode(this int source)
        {
            return ((long)source).Base35Encode();
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base35Encode(this long source)
        {
            return source.BaseEncode(Base35Chars);
        }

        /// <summary>
        /// Decodes 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base35Decode(this string source)
        {
            return source.BaseDecode(Base35Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base36Encode(this int source)
        {
            return ((long)source).Base36Encode();
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base36Encode(this long source)
        {
            return source.BaseEncode(Base36Chars);
        }

        /// <summary>
        /// Decodes 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base36Decode(this string source)
        {
            return source.BaseDecode(Base36Chars);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base52Encode(this int source)
        {
            return ((long)source).Base52Encode();
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base52Encode(this long source)
        {
            return source.BaseEncode(Base52Chars);
        }

        /// <summary>
        /// Decodes ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base52Decode(this string source)
        {
            return source.BaseDecode(Base52Chars);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base61Encode(this int source)
        {
            return ((long)source).Base61Encode();
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base61Encode(this long source)
        {
            return source.BaseEncode(Base61Chars);
        }

        /// <summary>
        /// Decodes 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base61Decode(this string source)
        {
            return source.BaseDecode(Base61Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base62Encode(this int source)
        {
            return ((long)source).Base62Encode();
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base62Encode(this long source)
        {
            return source.BaseEncode(Base62Chars);
        }

        /// <summary>
        /// Decodes 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base62Decode(this string source)
        {
            return source.BaseDecode(Base62Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source bytes.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base62EncodeBytes(this byte[] source)
        {
            StringBuilder result = new StringBuilder();
            BitStream stream = new BitStream(source);
            byte[] read = new byte[1];

            while (true)
            {
                read[0] = 0;
                int length = stream.Read(read, 0, 6);

                if (length == 6)
                {
                    if ((int)(read[0] >> 3) == 0x1f)
                    {
                        result.Append(Base62Chars[61]);
                        stream.Seek(-1, SeekOrigin.Current);
                    }
                    else if ((int)(read[0] >> 3) == 0x1e)
                    {
                        result.Append(Base62Chars[60]);
                        stream.Seek(-1, SeekOrigin.Current);
                    }
                    else
                    {
                        result.Append(Base62Chars[(int)(read[0] >> 2)]);
                    }
                }
                else if (length == 0)
                {
                    break;
                }
                else
                {
                    result.Append(Base62Chars[(int)(read[0] >> (int)(8 - length))]);
                    break;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Decodes 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded bytes.</returns>
        public static byte[] Base62DecodeBytes(this string source)
        {
            int count = 0;

            BitStream stream = new BitStream(source.Length * 6 / 8);

            foreach (char c in source)
            {
                int index = Base62Chars.IndexOf(c);

                if (count == source.Length - 1)
                {
                    int mod = (int)(stream.Position % 8);

                    if (mod == 0)
                    {
                        throw new InvalidDataException("An extra character was found");
                    }

                    if ((index >> (8 - mod)) > 0)
                    {
                        throw new InvalidDataException("Invalid ending character was found");
                    }

                    stream.Write(new byte[] { (byte)(index << mod) }, 0, 8 - mod);
                }
                else
                {
                    if (index == 60)
                    {
                        stream.Write(new byte[] { 0xf0 }, 0, 5);
                    }
                    else if (index == 61)
                    {
                        stream.Write(new byte[] { 0xf8 }, 0, 5);
                    }
                    else
                    {
                        stream.Write(new byte[] { (byte)index }, 2, 6);
                    }
                }

                count++;
            }

            byte[] result = new byte[stream.Position / 8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length * 8);

            return result;
        }

        /// <summary>
        /// Encodes to target Base chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>A Based encoded string.</returns>
        public static string BaseEncode(this int source, string baseChars)
        {
            return ((long)source).BaseEncode(baseChars);
        }

        /// <summary>
        /// Encodes to target Base chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>A Based encoded string.</returns>
        public static string BaseEncode(this long source, string baseChars)
        {
            decimal input = (decimal)source;

            bool negative = input < 0;

            if (negative)
            {
                input = (decimal)ulong.MaxValue + input;
            }

            long targetBaseLength = baseChars.Length;

            var result = new Stack<char>();

            do
            {
                result.Push(baseChars[(int)(input % targetBaseLength)]);
                input = (long)(input / targetBaseLength);
            }
            while (input != 0);

            var resultString = new string(result.ToArray());

            return negative ? ("-" + resultString) : resultString;
        }

        /// <summary>
        /// Decodes Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>The decoded number.</returns>
        public static long BaseDecode(this string source, string baseChars)
        {
            bool negative = source.StartsWith("-") && source.Length > 1;

            if (negative)
            {
                source = source.Substring(1);
            }

            int targetBaseLength = baseChars.Length;

            decimal result = 0;

            foreach (char position in source)
            {
                result = (result * targetBaseLength) + baseChars.IndexOf(position);
            }

            return (long)(negative ? (result - (decimal)ulong.MaxValue) : result);
        }
    }
}
