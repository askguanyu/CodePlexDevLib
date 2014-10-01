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
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        /// <returns>The rendered layout string.</returns>
        public static string Render(int skipFrames, LogLevel logLevel, bool useBracket, object[] objs)
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("{0}|", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUzzz", CultureInfo.InvariantCulture));
            result.AppendFormat("{0}|", logLevel.ToString());
            result.AppendFormat("{0}|", Environment.UserName);
            result.AppendFormat("{0}|", Thread.CurrentThread.ManagedThreadId.ToString("000"));

            if (objs != null && objs.Length > 0)
            {
                if (useBracket)
                {
                    foreach (var item in objs)
                    {
                        result.AppendFormat(" [{0}]", item == null ? string.Empty : item.ToString());
                    }
                }
                else
                {
                    foreach (var item in objs)
                    {
                        result.AppendFormat(" {0}", item == null ? string.Empty : item.ToString());
                    }
                }
            }

            result.AppendFormat(" |{0}", InternalLogger.GetStackFrameInfo(skipFrames < 1 ? 2 : skipFrames + 2));

            result.Append(Environment.NewLine);

            return result.ToString();
        }
    }
}
