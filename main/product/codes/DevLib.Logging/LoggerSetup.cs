﻿//-----------------------------------------------------------------------
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
        /// Field _rollingFileSizeLimitKB.
        /// </summary>
        private long _rollingFileSizeLimitKB;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerSetup" /> class.
        /// </summary>
        public LoggerSetup()
        {
            this.RollingFileSizeLimitKB = 10240;
            this.RollingByDate = false;
            this.UseBracket = true;
        }

        /// <summary>
        /// Gets or sets rolling log file size limit in KB. If less than or equal to zero, there is no limit.
        /// </summary>
        public long RollingFileSizeLimitKB
        {
            get
            {
                return this._rollingFileSizeLimitKB;
            }

            set
            {
                if (value > 0)
                {
                    this._rollingFileSizeLimitKB = value;

                    try
                    {
                        this._rollingFileSizeLimit = value * 1024;
                    }
                    catch
                    {
                        this._rollingFileSizeLimit = -1;
                    }
                }
                else
                {
                    this._rollingFileSizeLimitKB = -1;
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
        /// Gets or sets a value indicating whether use square brackets ([ ]) around log message.
        /// </summary>
        public bool UseBracket
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