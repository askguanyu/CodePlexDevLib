//-----------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="YuGuan Corporation">
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
    /// Provides access to customized configuration files for applications. This class cannot be inherited.
    /// </summary>
    public static class SettingsManager
    {
        /// <summary>
        /// Field SettingsDictionary.
        /// </summary>
        private static readonly Dictionary<string, Settings> SettingsDictionary = new Dictionary<string, Settings>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Field Lock.
        /// </summary>
        private static ReaderWriterLock Lock = new ReaderWriterLock();

        /// <summary>
        /// Opens the configuration file for the current application.
        /// </summary>
        /// <param name="configFile">Configuration file for the current application; if null or string.Empty use a new Settings instance.</param>
        /// <returns>Settings instance.</returns>
        public static Settings Open(string configFile = null)
        {
            if (string.IsNullOrEmpty(configFile))
            {
                return new Settings(null);
            }

            string key = Path.GetFullPath(configFile);

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (SettingsDictionary.ContainsKey(key))
                {
                    return SettingsDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (SettingsDictionary.ContainsKey(key))
                        {
                            return SettingsDictionary[key];
                        }
                        else
                        {
                            Settings result = new Settings(key);

                            SettingsDictionary.Add(key, result);

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
