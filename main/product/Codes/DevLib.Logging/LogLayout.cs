//-----------------------------------------------------------------------
// <copyright file="LogLayout.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class LogLayout.
    /// </summary>
    internal static class LogLayout
    {
        /// <summary>
        /// Render parameters into a string.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="useBracket">true to use square brackets ([ ]) around log message; otherwise, false.</param>
        /// <param name="enableStackInfo">true to enable stack information; otherwise, false.</param>
        /// <param name="message">Diagnostic message to log.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        /// <returns>The rendered layout string.</returns>
        public static string Render(int skipFrames, LogLevel logLevel, bool useBracket, bool enableStackInfo, string message, object[] objs)
        {
            StringBuilder result = new StringBuilder();

            result.Append(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUzzz", CultureInfo.InvariantCulture));
            result.Append("|");
            result.Append(logLevel.ToString());
            result.Append("|");
            result.Append(Environment.UserName);
            result.Append("|");
            result.Append(Thread.CurrentThread.ManagedThreadId.ToString("000"));
            result.Append("|");

            if (!string.IsNullOrEmpty(message))
            {
                if (useBracket)
                {
                    result.Append(" [");
                    result.Append(message);
                    result.Append("]");
                }
                else
                {
                    result.Append(" ");
                    result.Append(message);
                }
            }

            if (objs != null && objs.Length > 0)
            {
                if (useBracket)
                {
                    foreach (var item in objs)
                    {
                        result.Append(" [");
                        result.Append(item == null ? string.Empty : item.ToString());
                        result.Append("]");
                    }
                }
                else
                {
                    foreach (var item in objs)
                    {
                        result.Append(" ");
                        result.Append(item == null ? string.Empty : item.ToString());
                    }
                }
            }

            if (enableStackInfo)
            {
                result.Append(" |");
                result.Append(InternalLogger.GetStackFrameInfo(skipFrames < 1 ? 2 : skipFrames + 2));
            }

            result.Append(Environment.NewLine);

            return result.ToString();
        }
    }
}
