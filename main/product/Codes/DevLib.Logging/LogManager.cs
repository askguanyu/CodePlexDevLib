//-----------------------------------------------------------------------
// <copyright file="LogManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Provides access to log for applications. This class cannot be inherited.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// Field LoggerDictionary.
        /// </summary>
        private static readonly Dictionary<int, Logger> LoggerDictionary = new Dictionary<int, Logger>();

        /// <summary>
        /// Field Lock.
        /// </summary>
        private static readonly ReaderWriterLock Lock = new ReaderWriterLock();

        /// <summary>
        /// Opens the log file for the current application.
        /// </summary>
        /// <param name="logFile">Log file for the current application; if null or string.Empty use the default log file.</param>
        /// <returns>Logger instance.</returns>
        public static Logger Open(string logFile = null)
        {
            LogConfig logConfig = new LogConfig();

            if (!string.IsNullOrEmpty(logFile))
            {
                logConfig.LogFile = logFile;
            }

            int key = logConfig.GetHashCode();

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (LoggerDictionary.ContainsKey(key))
                        {
                            return LoggerDictionary[key];
                        }
                        else
                        {
                            Logger result = new Logger(logConfig);

                            LoggerDictionary.Add(key, result);

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

        /// <summary>
        /// Opens the log file for the current application.
        /// </summary>
        /// <param name="logFile">Log file for the current application; if null or string.Empty use the default log file.</param>
        /// <param name="loggerSetup">LoggerSetup info for the logger instance; if null use the default LoggerSetup info.</param>
        /// <returns>Logger instance.</returns>
        public static Logger Open(string logFile, LoggerSetup loggerSetup)
        {
            LogConfig logConfig = new LogConfig();

            if (!string.IsNullOrEmpty(logFile))
            {
                logConfig.LogFile = logFile;
            }

            if (loggerSetup != null)
            {
                logConfig.LoggerSetup = loggerSetup;
            }

            int key = logConfig.GetHashCode();

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (LoggerDictionary.ContainsKey(key))
                        {
                            return LoggerDictionary[key];
                        }
                        else
                        {
                            Logger result = new Logger(logConfig);

                            LoggerDictionary.Add(key, result);

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

        /// <summary>
        /// Opens the log file for the current application.
        /// </summary>
        /// <param name="logFile">Log file for the current application; if null or string.Empty use the default log file.</param>
        /// <param name="configFile">Configuration file which contains LoggerSetup info; if null or string.Empty use the default configuration file.</param>
        /// <returns>Logger instance.</returns>
        public static Logger Open(string logFile, string configFile)
        {
            LogConfig logConfig = new LogConfig();

            if (!string.IsNullOrEmpty(logFile))
            {
                logConfig.LogFile = logFile;
            }

            logConfig.LoggerSetup = LogConfigManager.GetLoggerSetup(configFile);

            int key = logConfig.GetHashCode();

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (LoggerDictionary.ContainsKey(key))
                        {
                            return LoggerDictionary[key];
                        }
                        else
                        {
                            Logger result = new Logger(logConfig);

                            LoggerDictionary.Add(key, result);

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

        /// <summary>
        /// Opens the log configuration file for the current application.
        /// </summary>
        /// <param name="configFile">Configuration file which contains LogConfig info; if null or string.Empty use the default configuration file.</param>
        /// <returns>Logger instance.</returns>
        public static Logger OpenConfig(string configFile = null)
        {
            LogConfig logConfig = LogConfigManager.GetLogConfig(configFile);

            int key = logConfig.GetHashCode();

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (LoggerDictionary.ContainsKey(key))
                        {
                            return LoggerDictionary[key];
                        }
                        else
                        {
                            Logger result = new Logger(logConfig);

                            LoggerDictionary.Add(key, result);

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

        /// <summary>
        /// Opens the log configuration file for the current application.
        /// </summary>
        /// <param name="logConfig">LogConfig instance; if null, use the default configuration.</param>
        /// <returns>Logger instance.</returns>
        public static Logger OpenConfig(LogConfig logConfig)
        {
            LogConfig logConfigInfo = logConfig ?? new LogConfig();

            int key = logConfigInfo.GetHashCode();

            Lock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (LoggerDictionary.ContainsKey(key))
                {
                    return LoggerDictionary[key];
                }
                else
                {
                    LockCookie lockCookie = Lock.UpgradeToWriterLock(Timeout.Infinite);

                    try
                    {
                        if (LoggerDictionary.ContainsKey(key))
                        {
                            return LoggerDictionary[key];
                        }
                        else
                        {
                            Logger result = new Logger(logConfigInfo);

                            LoggerDictionary.Add(key, result);

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
