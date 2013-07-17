//-----------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Provides access to customized configuration files for applications. This class cannot be inherited.
    /// </summary>
    public static class SettingsManager
    {
        /// <summary>
        /// Field _settingsDictionary.
        /// </summary>
        private static Dictionary<string, Settings> _settingsDictionary = new Dictionary<string, Settings>(StringComparer.OrdinalIgnoreCase);

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

            lock (((ICollection)_settingsDictionary).SyncRoot)
            {
                if (_settingsDictionary.ContainsKey(key))
                {
                    return _settingsDictionary[key];
                }
                else
                {
                    Settings result = new Settings(key);
                    _settingsDictionary.Add(key, result);
                    return result;
                }
            }
        }
    }
}
