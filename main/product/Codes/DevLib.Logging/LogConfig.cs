//-----------------------------------------------------------------------
// <copyright file="LogConfig.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Class LogConfig.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter<LogConfig>))]
    public class LogConfig
    {
        /// <summary>
        /// Field DefaultLogFile.
        /// </summary>
        public static readonly string DefaultLogFile = Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName + ".log");

        /// <summary>
        /// Field DefaultLogConfigFile.
        /// </summary>
        public static readonly string DefaultLogConfigFile = Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName + ".config");

        /// <summary>
        /// Field DefaultLoggerSetup.
        /// </summary>
        public static readonly LoggerSetup DefaultLoggerSetup = new LoggerSetup();

        /// <summary>
        ///  Initializes a new instance of the <see cref="LogConfig" /> class.
        /// </summary>
        public LogConfig()
        {
            this.LogFile = LogConfig.DefaultLogFile;
            this.LoggerSetup = LogConfig.DefaultLoggerSetup;
        }

        /// <summary>
        /// Gets or sets log file.
        /// </summary>
        public string LogFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets logger setup for log file.
        /// </summary>
        public LoggerSetup LoggerSetup
        {
            get;
            set;
        }

        /// <summary>
        /// Serves as a hash function for LogConfig instance.
        /// </summary>
        /// <returns>A hash code for the current LogConfig.</returns>
        public override int GetHashCode()
        {
            return string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                Path.GetFullPath(this.LogFile),
                this.LoggerSetup.DateTimeFormat ?? string.Empty,
                this.LoggerSetup.Level.ToString(),
                this.LoggerSetup.WriteToConsole.ToString(),
                this.LoggerSetup.WriteToFile.ToString(),
                this.LoggerSetup.UseBracket.ToString(),
                this.LoggerSetup.EnableStackInfo.ToString(),
                this.LoggerSetup.RollingFileSizeLimit.ToString(),
                this.LoggerSetup.RollingFileCountLimit.ToString(),
                this.LoggerSetup.RollingByDate.ToString()).ToUpperInvariant().GetHashCode();
        }
    }
}
