//-----------------------------------------------------------------------
// <copyright file="ExceptionHandler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Exception Handler.
    /// </summary>
    internal static class ExceptionHandler
    {
        /// <summary>
        /// Log Exception.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public static void Log(Exception exception)
        {
            if (exception != null)
            {
                string message = string.Format("[{0}] [EXCEPTION] [{1}] [{2}]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffffUTCzzz"), GetCallStackInfo(new StackFrame(1)), exception.ToString());
                Debug.WriteLine(message);
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Builds a readable representation of the stack frame.
        /// </summary>
        /// <param name="stackFrame">The frame that the System.Diagnostics.StackTrace object should contain, representing the function calls in the stack trace.</param>
        /// <returns>A readable representation of the stack frame.</returns>
        public static string GetCallStackInfo(StackFrame stackFrame)
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
