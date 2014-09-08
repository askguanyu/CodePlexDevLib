//-----------------------------------------------------------------------
// <copyright file="RetryAction.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Threading;

    /// <summary>
    /// Encapsulates a method that takes two parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">The first type of the parameter of the method that this delegate encapsulates.</typeparam>
    /// <typeparam name="T2">The second type of the parameter of the method that this delegate encapsulates.</typeparam>
    /// <param name="exception">Exception object to catch.</param>
    /// <param name="retryCount">Retry count number.</param>
    public delegate void CatchAction<T1, T2>(T1 exception, T2 retryCount);

    /// <summary>
    /// Provides the base implementation of the retry mechanism for unreliable actions.
    /// </summary>
    public static class RetryAction
    {
        /// <summary>
        /// Execute code snippets with retry mechanism.
        /// </summary>
        /// <param name="action">Code snippets that will be invoked whenever a retry condition is encountered.</param>
        /// <param name="catchAction">Code snippets that will be invoked whenever an exception is caught.
        /// <example>Default: <code>Console.WriteLine</code></example>
        /// </param>
        /// <param name="retryCount">The number of subsequent retry attempts, not including the very first execution.</param>
        /// <param name="retryInterval">The time interval between retries in milliseconds.</param>
        public static void Execute(Action<int> action, CatchAction<Exception, int> catchAction = null, int retryCount = 1, int retryInterval = 0)
        {
            if (action == null)
            {
                return;
            }

            if (retryCount < 0)
            {
                retryCount = 0;
            }

            if (retryInterval < 0)
            {
                retryInterval = 0;
            }

            if (catchAction == null)
            {
                catchAction = delegate(Exception e, int i) { Console.WriteLine("Count {0} failed: {1}", i.ToString(), e.ToString()); };
            }

            bool succeeded = false;

            int count = 0;

            while (count <= retryCount && !succeeded)
            {
                try
                {
                    action(count);
                    succeeded = true;
                }
                catch (Exception e)
                {
                    try
                    {
                        catchAction(e, count);
                    }
                    catch
                    {
                    }

                    count++;
                    Thread.Sleep(retryInterval);
                }
            }
        }
    }
}
