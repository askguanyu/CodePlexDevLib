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
        /// Represents debug message level.
        /// </summary>
        DBUG,

        /// <summary>
        /// Represents information message level.
        /// </summary>
        INFO,

        /// <summary>
        /// Represents exception message level.
        /// </summary>
        EXCP,

        /// <summary>
        /// Represents warning message level.
        /// </summary>
        WARN,

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
