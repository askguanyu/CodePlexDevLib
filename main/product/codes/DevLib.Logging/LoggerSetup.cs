//-----------------------------------------------------------------------
// <copyright file="LoggerSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;

    /// <summary>
    /// Class LoggerSetup.
    /// </summary>
    [Serializable]
    public class LoggerSetup
    {
        /// <summary>
        /// Field _rollingFileSizeLimit.
        /// </summary>
        private long _rollingFileSizeLimit;

        /// <summary>
        /// Field _rollingFileSizeLimitMB.
        /// </summary>
        private long _rollingFileSizeLimitMB;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerSetup" /> class.
        /// </summary>
        public LoggerSetup()
        {
            this.WriteToConsole = true;
            this.WriteToFile = true;
            this.UseBracket = true;
            this.RollingFileSizeLimitMB = 10;
            this.RollingByDate = false;
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
        /// Gets or sets rolling log file size limit in MB. If less than or equal to zero, there is no limit.
        /// </summary>
        public long RollingFileSizeLimitMB
        {
            get
            {
                return this._rollingFileSizeLimitMB;
            }

            set
            {
                if (value > 0)
                {
                    this._rollingFileSizeLimitMB = value;

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
                    this._rollingFileSizeLimitMB = -1;
                    this._rollingFileSizeLimit = -1;
                }
            }
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
