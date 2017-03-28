//-----------------------------------------------------------------------
// <copyright file="TextUtilities.cs" company="Yu Guan Corporation">
//     Copyright (c) Yu Guan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Text
{
    using System;
    using System.Text;
    using System.Web.Security;

    /// <summary>
    /// Text Utilities.
    /// </summary>
    public static class TextUtilities
    {
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
        /// Shortens the specified source string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>4 shorten string candidates in string array.</returns>
        public static string[] Shorten(string source)
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
        public static string Base64Decode(string source)
        {
            byte[] buffer = Convert.FromBase64String(source);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Base64 encodes a string.
        /// </summary>
        /// <param name="source">String to encode.</param>
        /// <returns>A base64 encoded string.</returns>
        public static string Base64Encode(string source)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(source);

            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base26Encode(int source)
        {
            return Base26Encode((long)source);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base26Encode(long source)
        {
            return BaseEncode(source, Base26Chars);
        }

        /// <summary>
        /// Decodes ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base26Decode(string source)
        {
            return BaseDecode(source, Base26Chars);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base35Encode(int source)
        {
            return Base35Encode((long)source);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base35Encode(long source)
        {
            return BaseEncode(source, Base35Chars);
        }

        /// <summary>
        /// Decodes 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base35Decode(string source)
        {
            return BaseDecode(source, Base35Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base36Encode(int source)
        {
            return Base36Encode((long)source);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base36Encode(long source)
        {
            return BaseEncode(source, Base36Chars);
        }

        /// <summary>
        /// Decodes 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base36Decode(string source)
        {
            return BaseDecode(source, Base36Chars);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base52Encode(int source)
        {
            return Base52Encode((long)source);
        }

        /// <summary>
        /// Encodes to ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base52Encode(long source)
        {
            return BaseEncode(source, Base52Chars);
        }

        /// <summary>
        /// Decodes ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base52Decode(string source)
        {
            return BaseDecode(source, Base52Chars);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base61Encode(int source)
        {
            return Base61Encode((long)source);
        }

        /// <summary>
        /// Encodes to 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base61Encode(long source)
        {
            return BaseEncode(source, Base61Chars);
        }

        /// <summary>
        /// Decodes 123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base61Decode(string source)
        {
            return BaseDecode(source, Base61Chars);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base62Encode(int source)
        {
            return Base62Encode((long)source);
        }

        /// <summary>
        /// Encodes to 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <returns>A Based encoded string.</returns>
        public static string Base62Encode(long source)
        {
            return BaseEncode(source, Base62Chars);
        }

        /// <summary>
        /// Decodes 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <returns>The decoded number.</returns>
        public static long Base62Decode(string source)
        {
            return BaseDecode(source, Base62Chars);
        }

        /// <summary>
        /// Encodes to target Base chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>A Based encoded string.</returns>
        public static string BaseEncode(int source, string baseChars)
        {
            return BaseEncode((long)source, baseChars);
        }

        /// <summary>
        /// Encodes to target Base chars set.
        /// </summary>
        /// <param name="source">The source number.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>A Based encoded string.</returns>
        public static string BaseEncode(long source, string baseChars)
        {
            long targetBase = baseChars.Length;

            char[] result = new char[Math.Max((int)Math.Ceiling(Math.Log(source + 1, targetBase)), 1)];

            var i = result.Length;

            do
            {
                result[--i] = baseChars[(int)(source % targetBase)];
                source = source / targetBase;
            }
            while (source > 0);

            return new string(result);
        }

        /// <summary>
        /// Decodes Based string to number.
        /// </summary>
        /// <param name="source">The source Based string.</param>
        /// <param name="baseChars">The Base chars set.</param>
        /// <returns>The decoded number.</returns>
        public static long BaseDecode(string source, string baseChars)
        {
            char[] sourceChars = source.ToCharArray();
            int index = sourceChars.Length - 1;
            int targetBase = baseChars.Length;
            int x = 0;
            long result = 0;

            for (int i = 0; i < sourceChars.Length; i++)
            {
                x = baseChars.IndexOf(sourceChars[i]);
                result += x * (long)Math.Pow(targetBase, index--);
            }

            return result;
        }
    }
}
