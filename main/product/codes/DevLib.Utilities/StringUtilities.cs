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
    /// String Utilities
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Returns a random string of the desired size
        /// </summary>
        /// <param name="size">Size of the random string to return</param>
        /// <returns>The result string</returns>
        public static string GetRandomString(int size)
        {
            if (size < 1)
            {
                return string.Empty;
            }

            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor((26 * random.NextDouble()) + 65))));
            }

            return stringBuilder.ToString();
        }
    }
}
