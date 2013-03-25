//-----------------------------------------------------------------------
// <copyright file="SecurityExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Security Extensions.
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// Verifies a original string against the passed MD5 hash.
        /// </summary>
        /// <param name="source">The <see cref="string">string</see> to compare.</param>
        /// <param name="hash">The hash to compare against.</param>
        /// <returns>true if the input and the hash are the same; otherwise, false.</returns>
        public static bool MD5VerifyToHash(this string source, string hash)
        {
            string sourceHash = source.ToMD5();

            return sourceHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies a MD5 hash against the passed original string.
        /// </summary>
        /// <param name="source">The MD5 hash to compare.</param>
        /// <param name="original">The original to compare against.</param>
        /// <returns>true if the input and the hash are the same; otherwise, false.</returns>
        public static bool MD5VerifyToOriginal(this string source, string original)
        {
            string originalHash = original.ToMD5();

            return source.Equals(originalHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// MD5 encodes the passed <see cref="string">string</see>.
        /// </summary>
        /// <param name="source">The <see cref="string">string</see> to encode.</param>
        /// <returns>An encoded <see cref="string">string</see>; string.Empty if hash failed.</returns>
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

        /// <summary>
        /// Decrypts a string using the supplied key. Decoding is done using RSA encryption.
        /// </summary>
        /// <param name="source">String to decrypt.</param>
        /// <param name="key">Decryption key.</param>
        /// <returns>The decrypted string or string. Empty if decryption failed.</returns>
        public static string RSADecrypt(this string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("An empty string value cannot be encrypted.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot decrypt using an empty key. Please supply a decryption key.");
            }

            string[] decryptArray = source.Split(new string[] { "-" }, StringSplitOptions.None);
            byte[] decryptByteArray = Array.ConvertAll<string, byte>(decryptArray, a => Convert.ToByte(byte.Parse(a, NumberStyles.HexNumber)));

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = key }))
            {
                rsa.PersistKeyInCsp = true;
                byte[] bytes = rsa.Decrypt(decryptByteArray, true);
                return Encoding.UTF8.GetString(bytes);
            }
        }

        /// <summary>
        /// Encrypts a string using the supplied key. Encoding is done using RSA encryption.
        /// </summary>
        /// <param name="source">String to encrypt.</param>
        /// <param name="key">Encryption key.</param>
        /// <returns>A string representing a byte array separated by a minus sign.</returns>
        public static string RSAEncrypt(this string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("An empty string value cannot be encrypted.");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot encrypt using an empty key. Please supply an encryption key.");
            }

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = key }))
            {
                rsa.PersistKeyInCsp = true;
                byte[] bytes = rsa.Encrypt(Encoding.UTF8.GetBytes(source), true);
                return BitConverter.ToString(bytes);
            }
        }
    }
}
