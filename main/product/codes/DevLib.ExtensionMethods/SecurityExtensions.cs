//-----------------------------------------------------------------------
// <copyright file="SecurityExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Security Extensions
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// Verifies a <see cref="string">string</see> against the passed MD5 hash
        /// </summary>
        /// <param name="source">The <see cref="string">string</see> to compare</param>
        /// <param name="hash">The hash to compare against</param>
        /// <returns>True if the input and the hash are the same, false otherwise</returns>
        public static bool MD5Verify(this string source, string hash)
        {
            // Hash the input.
            string sourceHash = source.ToMD5();

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(sourceHash, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// MD5 encodes the passed <see cref="string">string</see>
        /// </summary>
        /// <param name="source">The <see cref="string">string</see> to encode</param>
        /// <returns>An encoded <see cref="string">string; string.Empty if hash failed</see></returns>
        public static string ToMD5(this string source)
        {
            byte[] data;

            using (MD5 hasher = MD5.Create())
            {
                data = hasher.ComputeHash(Encoding.Default.GetBytes(source));
            }

            if (data != null)
            {
                return data.ToHexString(false);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
