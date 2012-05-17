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

        /// <summary>
        /// Decryptes a string using the supplied key. Decoding is done using RSA encryption.
        /// </summary>
        /// <param name="source">String to decrypt</param>
        /// <param name="key">Decryption key</param>
        /// <returns>The decrypted string or string.Empty if decryption failed</returns>
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

            string result = string.Empty;
            RSACryptoServiceProvider rsa = null;

            try
            {
                CspParameters cspParameters = new CspParameters();
                cspParameters.KeyContainerName = key;

                rsa = new RSACryptoServiceProvider(cspParameters);
                rsa.PersistKeyInCsp = true;

                string[] decryptArray = source.Split(new string[] { "-" }, StringSplitOptions.None);
                byte[] decryptByteArray = Array.ConvertAll<string, byte>(decryptArray, (a => Convert.ToByte(byte.Parse(a, System.Globalization.NumberStyles.HexNumber))));

                byte[] bytes = rsa.Decrypt(decryptByteArray, true);
                result = System.Text.UTF8Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (rsa != null)
                {
                    rsa.Clear();
                }
            }

            return result;
        }

        /// <summary>
        /// Encryptes a string using the supplied key. Encoding is done using RSA encryption.
        /// </summary>
        /// <param name="source">String to encrypt</param>
        /// <param name="key">Encryption key</param>
        /// <returns>A string representing a byte array separated by a minus sign</returns>
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

            string result = string.Empty;
            RSACryptoServiceProvider rsa = null;

            try
            {
                CspParameters cspParameters = new CspParameters();
                cspParameters.KeyContainerName = key;

                rsa = new RSACryptoServiceProvider(cspParameters);
                rsa.PersistKeyInCsp = true;

                byte[] bytes = rsa.Encrypt(System.Text.UTF8Encoding.UTF8.GetBytes(source), true);
                result = BitConverter.ToString(bytes);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (rsa != null)
                {
                    rsa.Clear();
                }
            }

            return result;
        }
    }
}
