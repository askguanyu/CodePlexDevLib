//-----------------------------------------------------------------------
// <copyright file="ExceptionHandler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Settings
{
    using System;
    using System.Diagnostics;

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
                string message = string.Format("[{0}] [EXCEPTION] [{1}] [{2}]", DateTime.Now.ToString(), exception.Source, exception.ToString());
                Debug.WriteLine(message);
                Console.WriteLine(message);
            }
        }
    }
}
