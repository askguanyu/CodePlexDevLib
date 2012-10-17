//-----------------------------------------------------------------------
// <copyright file="StringUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System;
    using System.Text;

    /// <summary>
    /// String Utilities.
    /// </summary>
    public static class StringUtilities
    {
        private static Random _random = new Random();

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
                stringBuilder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor((26 * _random.NextDouble()) + 65))));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a random alphabet and numeric string of the desired size.
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
        /// <param name="charset">charset of the random.</param>
        /// <param name="size">Size of the random string to return.</param>
        /// <returns>The result string.</returns>
        public static string GetRandomString(string charset, int size)
        {
            if ((string.IsNullOrEmpty(charset)) || (size < 1))
            {
                return string.Empty;
            }

            int charsetBound = charset.Length - 1;
            StringBuilder stringBuilder = new StringBuilder(size);

            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append(charset[_random.Next(0, charsetBound)]);
            }

            return stringBuilder.ToString();
        }
    }
}
