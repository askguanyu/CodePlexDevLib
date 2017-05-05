//-----------------------------------------------------------------------
// <copyright file="DiagnosticsUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Diagnostics Utilities.
    /// </summary>
    public static class DiagnosticsUtilities
    {
        /// <summary>
        /// WriteLine exception information to console.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public static void ConsoleOutputException(Exception exception)
        {
            if (exception != null)
            {
                string message = string.Format(
                    "{0}|{1}|{2}|{3}| [{4}] |{5}",
                    DateTime.Now.ToString("o", CultureInfo.InvariantCulture),
                    "EXCP",
                    Environment.UserName,
                    Thread.CurrentThread.ManagedThreadId.ToString("000"),
                    exception.ToString(),
                    GetStackFrameInfo(1));

                Debug.WriteLine(message);
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Builds a readable representation of the method in which the frame is executing.
        /// </summary>
        /// <param name="stackFrame">Instance of <see cref="T:System.Diagnostics.StackFrame" />, which represents a function call on the call stack for the current thread.</param>
        /// <returns>A readable representation of the method in which the frame is executing.</returns>
        public static string GetStackFrameMethodInfo(StackFrame stackFrame)
        {
            if (stackFrame == null)
            {
                return string.Empty;
            }

            MethodBase methodBase = stackFrame.GetMethod();

            if (methodBase != null)
            {
                StringBuilder result = new StringBuilder(255);

                Type declaringType = methodBase.DeclaringType;

                if (declaringType != null)
                {
                    result.Append(declaringType.FullName.Replace('+', '.'));
                    result.Append(".");
                }

                result.Append(methodBase.Name);

                if (methodBase is MethodInfo && ((MethodInfo)methodBase).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo)methodBase).GetGenericArguments();

                    result.Append("[");

                    int i = 0;

                    bool flag = true;

                    while (i < genericArguments.Length)
                    {
                        if (!flag)
                        {
                            result.Append(",");
                        }
                        else
                        {
                            flag = false;
                        }

                        result.Append(genericArguments[i].Name);

                        i++;
                    }

                    result.Append("]");
                }

                result.Append("(");

                ParameterInfo[] parameters = methodBase.GetParameters();

                bool flag2 = true;

                for (int j = 0; j < parameters.Length; j++)
                {
                    if (!flag2)
                    {
                        result.Append(", ");
                    }
                    else
                    {
                        flag2 = false;
                    }

                    string parameterTypeName = "<UnknownType>";

                    if (parameters[j].ParameterType != null)
                    {
                        parameterTypeName = parameters[j].ParameterType.Name;
                    }

                    result.Append(parameterTypeName + " " + parameters[j].Name);
                }

                result.Append(")");

                return result.ToString();
            }
            else
            {
                return string.Empty;
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
                StringBuilder result = new StringBuilder();

                result.Append(method.Name);

                if (method is MethodInfo && ((MethodInfo)method).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();

                    result.Append("<");

                    int i = 0;

                    bool flag = true;

                    while (i < genericArguments.Length)
                    {
                        if (!flag)
                        {
                            result.Append(",");
                        }
                        else
                        {
                            flag = false;
                        }

                        result.Append(genericArguments[i].Name);

                        i++;
                    }

                    result.Append(">");
                }

                result.Append(" in ");
                result.Append(Path.GetFileName(stackFrame.GetFileName()) ?? "<unknown>");
                result.Append(":");
                result.Append(stackFrame.GetFileLineNumber());

                return result.ToString();
            }
            else
            {
                return "<null>";
            }
        }
    }
}
