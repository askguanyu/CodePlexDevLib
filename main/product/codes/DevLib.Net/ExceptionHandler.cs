//-----------------------------------------------------------------------
// <copyright file="ExceptionHandler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net
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
                string message = string.Format("[{0}] [EXCEPTION] [{1}] [{2}]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUTCzzz"), GetStackFrameMethodInfo(new StackFrame(1)), exception.ToString());

                Debug.WriteLine(message);
#if DEBUG
                Console.WriteLine(message);

                lock (SyncRoot)
                {
                    try
                    {
                        File.AppendAllText("DevLib.Net.log", message + Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
#endif
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
                StringBuilder stringBuilder = new StringBuilder(255);

                Type declaringType = methodBase.DeclaringType;
                if (declaringType != null)
                {
                    stringBuilder.Append(declaringType.FullName.Replace('+', '.'));
                    stringBuilder.Append(".");
                }

                stringBuilder.Append(methodBase.Name);

                if (methodBase is MethodInfo && ((MethodInfo)methodBase).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo)methodBase).GetGenericArguments();
                    stringBuilder.Append("[");
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

                    stringBuilder.Append("]");
                }

                stringBuilder.Append("(");

                ParameterInfo[] parameters = methodBase.GetParameters();
                bool flag2 = true;
                for (int j = 0; j < parameters.Length; j++)
                {
                    if (!flag2)
                    {
                        stringBuilder.Append(", ");
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

                    stringBuilder.Append(parameterTypeName + " " + parameters[j].Name);
                }

                stringBuilder.Append(")");

                return stringBuilder.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
