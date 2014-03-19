//-----------------------------------------------------------------------
// <copyright file="InternalLogger.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Internal logger.
    /// </summary>
    internal static class InternalLogger
    {
        /// <summary>
        /// Field LogFile.
        /// </summary>
        private static readonly string LogFile = Path.GetFullPath(Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".log"));

        /// <summary>
        /// Field LogFileBackup.
        /// </summary>
        private static readonly string LogFileBackup = Path.ChangeExtension(LogFile, ".log.bak");

        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Method Log.
        /// </summary>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public static void Log(params object[] objs)
        {
#if DEBUG
            if (objs != null)
            {
                lock (SyncRoot)
                {
                    if (objs != null)
                    {
                        try
                        {
                            string message = RenderLog(objs);
                            Debug.WriteLine(message);
                            Console.WriteLine(message);
                            AppendToFile(message);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString());
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
#endif
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

        /// <summary>
        /// Render parameters into a string.
        /// </summary>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        /// <returns>The rendered layout string.</returns>
        private static string RenderLog(object[] objs)
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("[{0}]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUTCzzz"));
            result.AppendFormat(" [{0}]", "INTL");
            result.AppendFormat(" [{0}]", Environment.UserName);
            result.AppendFormat(" [{0,3}]", Thread.CurrentThread.ManagedThreadId);

            if (objs != null && objs.Length > 0)
            {
                foreach (var item in objs)
                {
                    result.AppendFormat(" [{0}]", item == null ? string.Empty : item.ToString());
                }
            }

            result.AppendFormat(" [{0}]", GetStackFrameInfo(2));
            result.Append(Environment.NewLine);
            return result.ToString();
        }

        /// <summary>
        /// Append log message to the file.
        /// </summary>
        /// <param name="message">Log message to append.</param>
        private static void AppendToFile(string message)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = File.Open(LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                if (fileStream.Length > 10485760)
                {
                    try
                    {
                        File.Copy(LogFile, LogFileBackup, true);
                    }
                    catch
                    {
                    }

                    fileStream.SetLength(0);
                }

                fileStream.Seek(0, SeekOrigin.End);
                byte[] bytes = Encoding.Unicode.GetBytes(message);
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
    }
}
