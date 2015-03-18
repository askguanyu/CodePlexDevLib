//-----------------------------------------------------------------------
// <copyright file="SecurityExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.Security.Cryptography;

    /// <summary>
    /// Security Extensions.
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// Encrypts data with the System.Security.Cryptography.RSA algorithm.
        /// </summary>
        /// <param name="source">The data to be encrypted.</param>
        /// <param name="key">Represents the key container name for System.Security.Cryptography.CspParameters.</param>
        /// <param name="fOAEP">true to perform direct RSA encryption using OAEP padding (only available on a computer running Microsoft Windows XP or later); otherwise, false to use PKCS#1 v1.5 padding.</param>
        /// <returns>The encrypted data.</returns>
        public static byte[] EncryptRSA(this byte[] source, string key = null, bool fOAEP = false)
        {
            if (source == null)
            {
                return null;
            }

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = key ?? string.Empty }))
            {
                return rsa.Encrypt(source, fOAEP);
            }
        }

        /// <summary>
        /// Decrypts data with the System.Security.Cryptography.RSA algorithm.
        /// </summary>
        /// <param name="source">The data to be decrypted.</param>
        /// <param name="key">Represents the key container name for System.Security.Cryptography.CspParameters.</param>
        /// <param name="fOAEP">true to perform direct RSA encryption using OAEP padding (only available on a computer running Microsoft Windows XP or later); otherwise, false to use PKCS#1 v1.5 padding.</param>
        /// <returns>The decrypted data, which is the original data before encryption.</returns>
        public static byte[] DecryptRSA(this byte[] source, string key = null, bool fOAEP = false)
        {
            if (source == null)
            {
                return null;
            }

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = key ?? string.Empty }))
            {
                return rsa.Decrypt(source, fOAEP);
            }
        }

        /// <summary>
        /// Computes the hash value for the specified byte array with System.Security.Cryptography.MD5 hash algorithm.
        /// </summary>
        /// <param name="source">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        public static byte[] HashMD5(this byte[] source)
        {
            using (MD5 hasher = MD5.Create())
            {
                return hasher.ComputeHash(source);
            }
        }
    }
}
