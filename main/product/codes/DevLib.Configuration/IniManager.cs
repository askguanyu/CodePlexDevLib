//-----------------------------------------------------------------------
// <copyright file="IniManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Provides access to ini files for applications. This class cannot be inherited.
    /// </summary>
    public static class IniManager
    {
        /// <summary>
        /// Field IniDictionary.
        /// </summary>
        private static readonly Dictionary<string, IniEntry> IniDictionary = new Dictionary<string, IniEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Field Lock.
        /// </summary>
        private static ReaderWriterLock Lock = new ReaderWriterLock();

        /// <summary>
        /// Opens the ini file for the current application.
        /// </summary>
        /// <param name="iniFile">Ini file for the current application; if null or string.Empty use the default AppDomain ini File.</param>
        /// <returns>IniEntry instance.</returns>
        public static IniEntry Open(string iniFile = null)
        {
            if (string.IsNullOrEmpty(iniFile))
            {
                iniFile = Path.ChangeExtension(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, "ini");
            }

            string key = Path.GetFullPath(iniFile);

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (IniDictionary.ContainsKey(key))
                {
                    return IniDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (IniDictionary.ContainsKey(key))
                        {
                            return IniDictionary[key];
                        }
                        else
                        {
                            IniEntry result = new IniEntry(key);

                            IniDictionary.Add(key, result);

                            return result;
                        }
                    }
                    finally
                    {
                        Lock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }
            finally
            {
                Lock.ReleaseReaderLock();
            }
        }
    }
}
