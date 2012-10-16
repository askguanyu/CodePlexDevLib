//-----------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Provides access to configuration files for client applications
    /// </summary>
    public static class SettingsManager
    {
        private static Dictionary<string, Settings> _settingsDict = new Dictionary<string, Settings>();

        /// <summary>
        /// Opens the configuration file for the current application
        /// </summary>
        /// <param name="configFile">Configuration file for the current application, can be a new one</param>
        public static Settings Open(string configFile)
        {
            if (_settingsDict.ContainsKey(configFile))
            {
                return _settingsDict[configFile];
            }

            Configuration configuration = null;
            try
            {
                configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = configFile }, ConfigurationUserLevel.None);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(SettingsConstants.ExceptionStringFormat, "SettingsManager.SettingsManager", e.Source, e.Message, e.StackTrace));
                configuration = ConfigurationManager.OpenExeConfiguration(Path.GetTempFileName());
            }
            _settingsDict.Add(configFile, new Settings(configFile, configuration));
            return _settingsDict[configFile];

        }
    }
}