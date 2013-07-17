//-----------------------------------------------------------------------
// <copyright file="ConfigManager.cs" company="YuGuan Corporation">
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
    /// Provides access to system configuration files for applications. This class cannot be inherited.
    /// </summary>
    public static class ConfigManager
    {
        /// <summary>
        /// Field _configDictionary.
        /// </summary>
        private static Dictionary<string, Config> _configDictionary = new Dictionary<string, Config>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Opens the configuration file for the current application.
        /// </summary>
        /// <param name="configFile">Configuration file for the current application; if null or string.Empty use the default ExeConfigFile.</param>
        /// <returns>Config instance.</returns>
        public static Config Open(string configFile = null)
        {
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }

            string key = Path.GetFullPath(configFile);

            lock (((ICollection)_configDictionary).SyncRoot)
            {
                if (_configDictionary.ContainsKey(key))
                {
                    return _configDictionary[key];
                }
                else
                {
                    Config result = new Config(key);
                    _configDictionary.Add(key, result);
                    return result;
                }
            }
        }
    }
}
