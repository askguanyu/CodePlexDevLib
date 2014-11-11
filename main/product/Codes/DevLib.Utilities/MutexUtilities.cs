//-----------------------------------------------------------------------
// <copyright file="MutexUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Mutex Utilities.
    /// </summary>
    public static class MutexUtilities
    {
        /// <summary>
        /// Field SharedMutexFileNamePrefix.
        /// </summary>
        private const string SharedMutexFileNamePrefix = @"Global\DevLibUtilitiesMutexUtilities_";

        /// <summary>
        /// Create a global shared mutex.
        /// </summary>
        /// <param name="mutexName">The mutex name.</param>
        /// <returns>Instance of Mutex.</returns>
        public static Mutex CreateSharedMutex(string mutexName)
        {
            try
            {
                MutexSecurity mutexSecurity = new MutexSecurity();

                mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));

                bool createdNew;

                return new Mutex(false, mutexName, out createdNew, mutexSecurity);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a global shared mutex name according to file name.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <returns>Mutex name.</returns>
        public static string GetSharedFileMutexName(string filename)
        {
            byte[] hash;

            using (MD5 hasher = MD5.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(filename).ToLowerInvariant()));
            }

            return SharedMutexFileNamePrefix + BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
