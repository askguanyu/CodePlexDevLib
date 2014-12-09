//-----------------------------------------------------------------------
// <copyright file="LoggerSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Class LoggerSetup.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter<LoggerSetup>))]
    public class LoggerSetup
    {
        /// <summary>
        /// Field _rollingFileSizeLimit.
        /// </summary>
        private long _rollingFileSizeLimit;

        /// <summary>
        /// Field _rollingFileSizeMBLimit.
        /// </summary>
        private long _rollingFileSizeMBLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerSetup" /> class.
        /// </summary>
        public LoggerSetup()
        {
            this.DateTimeFormat = string.Empty;
            this.Level = LogLevel.DBUG;
            this.WriteToConsole = true;
            this.WriteToFile = true;
            this.UseBracket = true;
            this.EnableStackInfo = true;
            this.RollingFileSizeMBLimit = 10;
            this.RollingFileCountLimit = 10;
            this.RollingByDate = false;
        }

        /// <summary>
        /// Gets or sets the standard or custom date and time format string.
        /// </summary>
        public string DateTimeFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating log level hierarchy write to log file.
        /// </summary>
        public LogLevel Level
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether write log message to console.
        /// </summary>
        public bool WriteToConsole
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether write log message to log file.
        /// </summary>
        public bool WriteToFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use square brackets ([ ]) around log message.
        /// </summary>
        public bool UseBracket
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether enable stack frame information.
        /// </summary>
        public bool EnableStackInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets rolling log file size limit in MB. If less than or equal to zero, there is no limit.
        /// </summary>
        public long RollingFileSizeMBLimit
        {
            get
            {
                return this._rollingFileSizeMBLimit;
            }

            set
            {
                if (value > 0)
                {
                    this._rollingFileSizeMBLimit = value;

                    try
                    {
                        this._rollingFileSizeLimit = value * 1024 * 1024;
                    }
                    catch
                    {
                        this._rollingFileSizeLimit = -1;
                    }
                }
                else
                {
                    this._rollingFileSizeMBLimit = -1;
                    this._rollingFileSizeLimit = -1;
                }
            }
        }

        /// <summary>
        /// Gets or sets rolling log file count limit. If less than zero, there is no limit. If equal to zero, there is no rolling log file.
        /// </summary>
        public long RollingFileCountLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether split log file by date.
        /// </summary>
        public bool RollingByDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets rolling log file size limit in byte.
        /// </summary>
        internal long RollingFileSizeLimit
        {
            get
            {
                return this._rollingFileSizeLimit;
            }
        }
    }
}
