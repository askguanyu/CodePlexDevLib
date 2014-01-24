//-----------------------------------------------------------------------
// <copyright file="ExceptionHandler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Exception Handler.
    /// </summary>
    internal static class ExceptionHandler
    {
        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Log Exception.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public static void Log(Exception exception)
        {
            if (exception != null)
            {
                string message = string.Format(
                    "[{0}] [{1}] [{2}] [{3}] [{4}]",
                    DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUTCzzz"),
                    "EXCP",
                    Environment.UserName,
                    exception.ToString(),
                    GetStackFrameInfo(1));

                Debug.WriteLine(message);
#if DEBUG
                Console.WriteLine(message);

                lock (SyncRoot)
                {
                    FileStream fileStream = null;

                    try
                    {
                        fileStream = File.Open("DevLib.Timers.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                        if (fileStream.Length > 10485760)
                        {
                            fileStream.SetLength(10485760);
                        }

                        fileStream.Seek(0, SeekOrigin.Begin);
                        byte[] bytes = Encoding.Unicode.GetBytes(message + Environment.NewLine);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Flush();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (fileStream != null)
                        {
                            fileStream.Dispose();
                            fileStream = null;
                        }
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Builds a readable representation of the stack trace.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <returns>A readable representation of the stack trace.</returns>
        public static string GetStackFrameInfo(int skipFrames)
        {
            StackFrame stackFrame = new StackFrame(skipFrames < 1 ? 1 : skipFrames + 1, true);

            MethodBase method = stackFrame.GetMethod();

            if (method != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(method.Name);

                if (method is MethodInfo && ((MethodInfo)method).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();

                    stringBuilder.Append("<");

                    int i = 0;

                    bool flag = true;

                    while (i < genericArguments.Length)
                    {
                        if (!flag)
                        {
                            stringBuilder.Append(",");
                        }
                        else
                        {
                            flag = false;
                        }

                        stringBuilder.Append(genericArguments[i].Name);

                        i++;
                    }

                    stringBuilder.Append(">");
                }

                stringBuilder.Append(" in ");

                stringBuilder.Append(Path.GetFileName(stackFrame.GetFileName()) ?? "<unknown>");

                stringBuilder.Append(":");

                stringBuilder.Append(stackFrame.GetFileLineNumber());

                return stringBuilder.ToString();
            }
            else
            {
                return "<null>";
            }
        }
    }
}
