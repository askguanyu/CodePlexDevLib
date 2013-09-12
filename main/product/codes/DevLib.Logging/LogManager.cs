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
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Provides access to log for applications. This class cannot be inherited.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// Field LoggerSetupInfo.
        /// </summary>
        private static readonly LoggerSetup LoggerSetupInfo = new LoggerSetup();

        /// <summary>
        /// Field LoggerDictionary.
        /// </summary>
        private static readonly Dictionary<string, Logger> LoggerDictionary = new Dictionary<string, Logger>(StringComparer.OrdinalIgnoreCase);

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
        /// Opens the log file for the current application.
        /// </summary>
        /// <param name="logFile">Log file for the current application; if null or string.Empty use the default log file.</param>
        /// <param name="loggerSetup">Logger setup for the logger instance; if null use the default logger setup.</param>
        /// <returns>Logger instance.</returns>
        public static Logger Open(string logFile = null, LoggerSetup loggerSetup = null)
        {
            if (string.IsNullOrEmpty(logFile))
            {
                logFile = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, "log");
            }

            string key = Path.GetFullPath(logFile);

            if (loggerSetup == null)
            {
                loggerSetup = LoggerSetupInfo;
            }

            lock (((ICollection)LoggerDictionary).SyncRoot)
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    Logger result = new Logger(key, loggerSetup);
                    LoggerDictionary.Add(key, result);
                    return result;
                }
            }
        }
    }
}
