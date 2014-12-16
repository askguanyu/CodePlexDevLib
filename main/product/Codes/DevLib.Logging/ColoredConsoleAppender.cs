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
        private static readonly Dictionary<LogLevel, ConsoleColor> ConsoleColorDictionary;

        /// <summary>
        /// Initializes static members of the <see cref="ColoredConsoleAppender" /> class.
        /// </summary>
        static ColoredConsoleAppender()
        {
            ConsoleColorDictionary = new Dictionary<LogLevel, ConsoleColor>(6);

            ConsoleColorDictionary.Add(LogLevel.DBUG, ConsoleColor.DarkCyan);
            ConsoleColorDictionary.Add(LogLevel.INFO, ConsoleColor.Cyan);
            ConsoleColorDictionary.Add(LogLevel.EXCP, ConsoleColor.DarkYellow);
            ConsoleColorDictionary.Add(LogLevel.WARN, ConsoleColor.Yellow);
            ConsoleColorDictionary.Add(LogLevel.ERRO, ConsoleColor.Red);
            ConsoleColorDictionary.Add(LogLevel.FAIL, ConsoleColor.Magenta);
        }

        /// <summary>
        /// Writes the specified string value with foreground color, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="level">The Log level.</param>
        /// <param name="message">The Message to write.</param>
        public static void Write(LogLevel level, string message)
        {
            lock (ConsoleSyncRoot)
            {
                ConsoleColor originalForeColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColorDictionary[level];

                Console.WriteLine(message);

                Console.ForegroundColor = originalForeColor;
            }
        }
    }
}
