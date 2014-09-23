//-----------------------------------------------------------------------
// <copyright file="LogLevel.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    /// <summary>
    /// Enum LogLevel.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Represents debug message level. Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development.
        /// </summary>
        DBUG,

        /// <summary>
        /// Represents information message level. Which are normally enabled in production environment.
        /// </summary>
        INFO,

        /// <summary>
        /// Represents warning message level. Typically for non-critical issues, which can be recovered or which are temporary failures.
        /// </summary>
        WARN,

        /// <summary>
        /// Represents exception message level.
        /// </summary>
        EXCP,

        /// <summary>
        /// Represents error message level.
        /// </summary>
        ERRO,

        /// <summary>
        /// Represents fatal or critical message level.
        /// </summary>
        FAIL,
    }
}
