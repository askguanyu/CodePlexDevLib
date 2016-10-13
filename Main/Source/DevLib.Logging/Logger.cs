//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Class Logger. Provides logging functions.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Reviewed.")]
    public sealed class Logger
    {
        /// <summary>
        /// Field _logFile.
        /// </summary>
        private readonly string _logFile;

        /// <summary>
        /// Synchronizes access to <see cref="_queue" />.
        /// </summary>
        private readonly object _queueSyncRoot = new object();

        /// <summary>
        /// The <see cref="Queue{T}" /> that contains the data items.
        /// </summary>
        private readonly Queue<string> _queue;

        /// <summary>
        /// Allows the consumer thread to block when no items are available in the <see cref="_queue" />.
        /// </summary>
        private readonly AutoResetEvent _queueWaitHandle;

        /// <summary>
        /// Field _consumerThread.
        /// </summary>
        private readonly Thread _consumerThread;

        /// <summary>
        /// Field _configFile.
        /// </summary>
        private readonly string _configFile;

        /// <summary>
        /// Field _configFileWatcher.
        /// </summary>
        private readonly FileSystemWatcher _configFileWatcher;

        /// <summary>
        /// Field _loggerSetup.
        /// </summary>
        private LoggerSetup _loggerSetup;

        /// <summary>
        /// Field _fileAppender.
        /// </summary>
        private MutexMultiProcessFileAppender _fileAppender;

        /// <summary>
        /// Field _isConfigFileChanged.
        /// </summary>
        private volatile bool _isConfigFileChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class.
        /// </summary>
        /// <param name="logFile">Log file.</param>
        /// <param name="loggerSetup">Log setup.</param>
        /// <param name="configFile">Config file.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        internal Logger(string logFile, LoggerSetup loggerSetup, string configFile)
        {
            this._logFile = logFile;
            this._loggerSetup = loggerSetup;

            this._queueWaitHandle = new AutoResetEvent(false);
            this._queue = new Queue<string>();
            this._consumerThread = new Thread(this.ConsumerThread);
            this._consumerThread.IsBackground = true;
            this._consumerThread.Start();

            if (!this.IsNullOrWhiteSpace(configFile))
            {
                this._configFile = LogConfigManager.GetFileFullPath(configFile);

                try
                {
                    this._configFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(this._configFile), Path.GetFileName(this._configFile));
                    this._configFileWatcher.EnableRaisingEvents = true;
                    this._configFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    this._configFileWatcher.Changed += (s, e) => this._isConfigFileChanged = true;
                    this._configFileWatcher.Created += (s, e) => this._isConfigFileChanged = true;
                    this._configFileWatcher.Deleted += (s, e) => this._isConfigFileChanged = true;
                    this._configFileWatcher.Renamed += (s, e) => this._isConfigFileChanged = true;
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }

            this.InitFileAppender();

            this.WatchConfigFileChanged();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class.
        /// </summary>
        /// <param name="logFile">Log file.</param>
        internal Logger(string logFile)
            : this(logFile, LoggerSetup.DefaultSetup, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class.
        /// </summary>
        /// <param name="logConfig">LogConfig instance.</param>
        /// <param name="configFile">Config file.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        internal Logger(LogConfig logConfig, string configFile = null)
            : this(logConfig.LogFile, logConfig.LoggerSetup, configFile)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Logger" /> class.
        /// </summary>
        ~Logger()
        {
            this.DisposeFileAppender();

            if (this._configFileWatcher != null)
            {
                this._configFileWatcher.Dispose();
            }

            if (this._consumerThread != null)
            {
                try
                {
                    this._consumerThread.Abort();
                }
                catch
                {
                }
            }

            if (this._queueWaitHandle != null)
            {
                this._queueWaitHandle.Close();
            }
        }

        /// <summary>
        /// Writes the diagnostic message at INFO level.
        /// </summary>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(params object[] objs)
        {
            this.Log(1, LogLevel.INFO, null, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at INFO level.
        /// </summary>
        /// <param name="message">Diagnostic message to log.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(string message, params object[] objs)
        {
            this.Log(1, LogLevel.INFO, message, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(LogLevel logLevel, params object[] objs)
        {
            this.Log(1, logLevel, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="message">Diagnostic message to log.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(LogLevel logLevel, string message, params object[] objs)
        {
            this.Log(1, logLevel, message, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(int skipFrames, LogLevel logLevel, params object[] objs)
        {
            this.WatchConfigFileChanged();

            string logMessage = null;
#if DEBUG
            logMessage = LogLayout.Render(skipFrames, this._loggerSetup.DateTimeFormat, logLevel, this._loggerSetup.UseBracket, this._loggerSetup.EnableStackInfo, (string)null, objs);
            Debug.WriteLine(logMessage);
#endif
            if (logLevel >= this._loggerSetup.Level)
            {
                if ((this._loggerSetup.WriteToConsole && Environment.UserInteractive) || (this._loggerSetup.WriteToFile && this._fileAppender != null))
                {
                    try
                    {
#if !DEBUG
                        logMessage = LogLayout.Render(skipFrames, this._loggerSetup.DateTimeFormat, logLevel, this._loggerSetup.UseBracket, this._loggerSetup.EnableStackInfo, (string)null, objs);
#endif
                        if (this._loggerSetup.WriteToConsole && Environment.UserInteractive)
                        {
                            ColoredConsoleAppender.Write(logLevel, logMessage);
                        }

                        if (this._loggerSetup.WriteToFile && this._fileAppender != null)
                        {
                            lock (this._queueSyncRoot)
                            {
                                this._queue.Enqueue(logMessage);
                                this._queueWaitHandle.Set();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="message">Diagnostic message to log.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(int skipFrames, LogLevel logLevel, string message, params object[] objs)
        {
            this.WatchConfigFileChanged();

            string logMessage = null;
#if DEBUG
            logMessage = LogLayout.Render(skipFrames, this._loggerSetup.DateTimeFormat, logLevel, this._loggerSetup.UseBracket, this._loggerSetup.EnableStackInfo, message, objs);
            Debug.WriteLine(logMessage);
#endif
            if (logLevel >= this._loggerSetup.Level)
            {
                if ((this._loggerSetup.WriteToConsole && Environment.UserInteractive) || (this._loggerSetup.WriteToFile && this._fileAppender != null))
                {
                    try
                    {
#if !DEBUG
                        logMessage = LogLayout.Render(skipFrames, this._loggerSetup.DateTimeFormat, logLevel, this._loggerSetup.UseBracket, this._loggerSetup.EnableStackInfo, message, objs);
#endif
                        if (this._loggerSetup.WriteToConsole && Environment.UserInteractive)
                        {
                            ColoredConsoleAppender.Write(logLevel, logMessage);
                        }

                        if (this._loggerSetup.WriteToFile && this._fileAppender != null)
                        {
                            lock (this._queueSyncRoot)
                            {
                                this._queue.Enqueue(logMessage);
                                this._queueWaitHandle.Set();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
            }
        }

        /// <summary>
        /// Watches config file changed.
        /// </summary>
        private void WatchConfigFileChanged()
        {
            if (this._isConfigFileChanged)
            {
                this._loggerSetup = LogConfigManager.GetLoggerSetup(this._configFile);

                this.InitFileAppender();

                this._isConfigFileChanged = false;
            }
        }

        /// <summary>
        /// Initializes file appender.
        /// </summary>
        private void InitFileAppender()
        {
            this.DisposeFileAppender();

            if (this._loggerSetup.WriteToFile)
            {
                try
                {
                    this._fileAppender = new MutexMultiProcessFileAppender(this._logFile, this._loggerSetup);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Dispose file appender.
        /// </summary>
        private void DisposeFileAppender()
        {
            if (this._fileAppender != null)
            {
                this._fileAppender.Dispose();
                this._fileAppender = null;
            }
        }

        /// <summary>
        /// The consumer thread.
        /// </summary>
        private void ConsumerThread()
        {
            while (true)
            {
                string[] nextItems = null;

                bool itemExists;

                lock (this._queueSyncRoot)
                {
                    itemExists = this._queue.Count > 0;

                    if (itemExists)
                    {
                        nextItems = this._queue.ToArray();
                        this._queue.Clear();
                    }
                }

                if (itemExists
                    && this._loggerSetup.WriteToFile
                    && this._fileAppender != null
                    && nextItems != null
                    && nextItems.Length > 0)
                {
                    try
                    {
                        this._fileAppender.Write(nextItems);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
                else
                {
                    this._queueWaitHandle.WaitOne();
                }
            }
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        private bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
