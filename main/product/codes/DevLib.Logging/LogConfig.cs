//-----------------------------------------------------------------------
// <copyright file="LogConfig.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;

    /// <summary>
    /// Class LogConfig.
    /// </summary>
    [Serializable]
    public class LogConfig
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="LogConfig" /> class.
        /// </summary>
        public LogConfig()
        {
            this.LogFile = LogManager.DefaultLogFile;
            this.LoggerSetup = LogManager.DefaultLoggerSetup;
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
    }
}
