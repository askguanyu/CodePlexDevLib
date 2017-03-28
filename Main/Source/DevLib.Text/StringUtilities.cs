//-----------------------------------------------------------------------
// <copyright file="StringUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Text
{
    using System;
    using System.Text;
    using System.Web.Security;
    using System.Collections.Generic;

    /// <summary>
    /// String Utilities.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Static Field _random.
        /// </summary>
        private static readonly Random RandomObj = new Random();

        /// <summary>
        /// Returns a random alphabet string of the desired size.
        /// </summary>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomAlphabetString(int size)
        {
            if (size < 1)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor((26 * RandomObj.NextDouble()) + 65))));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a random numeric string of the desired size.
        /// From 0123456789
        /// </summary>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomNumericString(int size)
        {
            return GetRandomString("0123456789", size);
        }

        /// <summary>
        /// Returns a random natural number string of the desired size.
        /// </summary>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomNaturalNumberString(int size)
        {
            return GetRandomString("123456789", size > 0 ? 1 : 0) + GetRandomString("0123456789", size - 1);
        }

        /// <summary>
        /// Returns a random alphabet and numeric string of the desired size.
        /// From 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ
        /// </summary>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomAlphaNumericString(int size)
        {
            return GetRandomString("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", size);
        }

        /// <summary>
        /// Returns a random string of the desired size with desired charset.
        /// </summary>
        /// <param name="charset">Chars set of the random.</param>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomString(string charset, int size)
        {
            if (string.IsNullOrEmpty(charset) || size < 1)
            {
                return string.Empty;
            }

            int charsetBound = charset.Length - 1;
            StringBuilder stringBuilder = new StringBuilder(size);

            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append(charset[RandomObj.Next(0, charsetBound)]);
            }

            return stringBuilder.ToString();
        }
    }
}
