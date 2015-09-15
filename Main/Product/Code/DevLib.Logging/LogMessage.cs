//-----------------------------------------------------------------------
// <copyright file="LogMessage.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;

    /// <summary>
    /// Log message entity.
    /// </summary>
    [Serializable]
    internal class LogMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        public LogMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The log message.</param>
        public LogMessage(LogLevel level, string message)
        {
            this.Level = level;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel Level
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }
}
