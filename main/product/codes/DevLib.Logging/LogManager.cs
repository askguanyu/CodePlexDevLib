//-----------------------------------------------------------------------
// <copyright file="LogManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;

    /// <summary>
    /// Provides access to log for applications. This class cannot be inherited.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// Field LoggerDictionary.
        /// </summary>
        private static readonly Dictionary<string, Logger> LoggerDictionary = new Dictionary<string, Logger>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Field LoggerSetupInfo.
        /// </summary>
        private static readonly LoggerSetup LoggerSetupInfo = new LoggerSetup();

        /// <summary>
        /// Field _defaultLogFile.
        /// </summary>
        private static string _defaultLogFile;

        /// <summary>
        /// Field _defaultLogConfigFile.
        /// </summary>
        private static string _defaultLogConfigFile;

        /// <summary>
        /// Initializes static members of the <see cref="LogManager" /> class.
        /// </summary>
        static LogManager()
        {
            DefaultLogFile = null;
            DefaultLogConfigFile = null;
        }

        /// <summary>
        /// Gets the default logger setup.
        /// </summary>
        public static LoggerSetup DefaultLoggerSetup
        {
            get
            {
                return LoggerSetupInfo;
            }
        }

        /// <summary>
        /// Gets or sets default log file.
        /// </summary>
        public static string DefaultLogFile
        {
            get
            {
                return _defaultLogFile;
            }

            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _defaultLogFile = Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName + ".log");
                }
                else
                {
                    _defaultLogFile = Path.GetFullPath(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets default log configuration file.
        /// </summary>
        public static string DefaultLogConfigFile
        {
            get
            {
                return _defaultLogConfigFile;
            }

            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _defaultLogConfigFile = Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName + ".config");
                }
                else
                {
                    _defaultLogConfigFile = Path.GetFullPath(value);
                }
            }
        }

        /// <summary>
        /// Opens the log file for the current application.
        /// </summary>
        /// <param name="logFile">Log file for the current application; if null or string.Empty use the default log file.</param>
        /// <param name="loggerSetup">Logger setup for the logger instance; if null use the default logger setup.</param>
        /// <returns>Logger instance.</returns>
        public static Logger Open(string logFile = null, LoggerSetup loggerSetup = null)
        {
            if (string.IsNullOrEmpty(logFile))
            {
                logFile = DefaultLogFile;
            }

            string key = Path.GetFullPath(logFile);

            lock (((ICollection)LoggerDictionary).SyncRoot)
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    Logger result = new Logger(key, loggerSetup ?? DefaultLoggerSetup);
                    LoggerDictionary.Add(key, result);
                    return result;
                }
            }
        }

        /// <summary>
        /// Opens the log configuration file for the current application.
        /// </summary>
        /// <param name="logConfigFile">Log configuration file for the current application; if null or string.Empty use the default configuration file.</param>
        /// <returns>Logger instance.</returns>
        public static Logger OpenConfig(string logConfigFile = null)
        {
            if (string.IsNullOrEmpty(logConfigFile))
            {
                logConfigFile = DefaultLogConfigFile;
            }

            string key = Path.GetFullPath(logConfigFile);

            lock (((ICollection)LoggerDictionary).SyncRoot)
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LogConfig logConfig = LogConfigManager.GetConfig(logConfigFile);
                    Logger result = new Logger(logConfig.LogFile, logConfig.LoggerSetup);
                    LoggerDictionary.Add(key, result);
                    return result;
                }
            }
        }
    }
}
