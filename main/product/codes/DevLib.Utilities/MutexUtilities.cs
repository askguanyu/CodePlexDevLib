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
        private const string SharedMutexFileNamePrefix = @"Global\DevLib.Utilities.MutexUtilities/";

        /// <summary>
        /// Field MaxMutexNameLength.
        /// </summary>
        private const int MaxMutexNameLength = 260;

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
            string mutexName = new Uri(Path.GetFullPath(filename)).AbsolutePath.ToLowerInvariant();

            if (SharedMutexFileNamePrefix.Length + mutexName.Length <= MaxMutexNameLength)
            {
                return SharedMutexFileNamePrefix + mutexName;
            }
            else
            {
                string hash;

                using (MD5 md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(mutexName));

                    hash = Convert.ToBase64String(bytes);
                }

                int index = mutexName.Length - (MaxMutexNameLength - SharedMutexFileNamePrefix.Length - hash.Length);

                return SharedMutexFileNamePrefix + hash + mutexName.Substring(index);
            }
        }
    }
}
