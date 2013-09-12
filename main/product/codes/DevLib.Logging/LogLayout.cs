//-----------------------------------------------------------------------
// <copyright file="LogLayout.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Text;

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

            result.AppendFormat("[{0}]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUTCzzz"));
            result.AppendFormat(" [{0}]", logLevel.ToString());
            result.AppendFormat(" [{0}]", Environment.UserName);

            if (objs != null && objs.Length > 0)
            {
                if (useBracket)
                {
                    foreach (var item in objs)
                    {
                        result.AppendFormat(" [{0}]", item.ToString());
                    }
                }
                else
                {
                    foreach (var item in objs)
                    {
                        result.AppendFormat(" {0}", item.ToString());
                    }
                }
            }

            result.AppendFormat(" [{0}]", ExceptionHandler.GetStackFrameInfo(skipFrames < 0 ? 3 : skipFrames + 3));

            result.Append(Environment.NewLine);

            return result.ToString();
        }
    }
}
