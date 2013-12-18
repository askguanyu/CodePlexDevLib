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
    /// Provides the base implementation of the retry mechanism for unreliable actions.
    /// </summary>
    public static class RetryAction
    {
        /// <summary>
        /// Execute code snippets with retry mechanism.
        /// </summary>
        /// <param name="action">Code snippets that will be invoked whenever a retry condition is encountered.</param>
        /// <param name="retryCount">The number of subsequent retry attempts, not including the very first execution.</param>
        /// <param name="retryInterval">The time interval between retries in milliseconds.</param>
        public static void Execute(Action<int> action, int retryCount = 1, int retryInterval = 0)
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

            bool succeeded = false;

            int count = 0;

            while (count <= retryCount && !succeeded)
            {
                try
                {
                    action(count);
                    succeeded = true;
                }
                catch
                {
                    count++;
                    Thread.Sleep(retryInterval);
                }
            }
        }
    }
}
