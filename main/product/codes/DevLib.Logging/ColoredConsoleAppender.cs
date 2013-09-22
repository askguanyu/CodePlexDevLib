//-----------------------------------------------------------------------
// <copyright file="ColoredConsoleAppender.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class ColoredConsoleAppender.
    /// </summary>
    internal static class ColoredConsoleAppender
    {
        /// <summary>
        /// Field ConsoleSyncRoot.
        /// </summary>
        private static readonly object ConsoleSyncRoot = new object();

        /// <summary>
        /// Field ConsoleColorDictionary.
        /// </summary>
        private static readonly Dictionary<LogLevel, ConsoleColor> ConsoleColorDictionary = new Dictionary<LogLevel, ConsoleColor>();

        /// <summary>
        /// Initializes static members of the <see cref="ColoredConsoleAppender" /> class.
        /// </summary>
        static ColoredConsoleAppender()
        {
            ConsoleColorDictionary[LogLevel.DBUG] = ConsoleColor.DarkCyan;
            ConsoleColorDictionary[LogLevel.INFO] = ConsoleColor.Cyan;
            ConsoleColorDictionary[LogLevel.EXCP] = ConsoleColor.DarkYellow;
            ConsoleColorDictionary[LogLevel.WARN] = ConsoleColor.Yellow;
            ConsoleColorDictionary[LogLevel.ERRO] = ConsoleColor.Red;
            ConsoleColorDictionary[LogLevel.FAIL] = ConsoleColor.Magenta;
        }

        /// <summary>
        /// Writes the specified string value with foreground color, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="message">Message to write.</param>
        public static void Write(LogLevel logLevel, string message)
        {
            lock (ConsoleSyncRoot)
            {
                ConsoleColor originalForeColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColorDictionary[logLevel];

                Console.WriteLine(message);

                Console.ForegroundColor = originalForeColor;
            }
        }
    }
}
