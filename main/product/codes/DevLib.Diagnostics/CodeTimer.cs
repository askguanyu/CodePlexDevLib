//-----------------------------------------------------------------------
// <copyright file="CodeTimer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Code snippets performence timer.
    /// </summary>
    [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
    public static class CodeTimer
    {
        /// <summary>
        ///
        /// </summary>
        private static Random _random = new Random();

        /*
        /// <summary>
        /// Run code snippets and give a performance test result. Extension Method for .Net 3.5 or above.
        /// </summary>
        /// <param name="action">Code snippets to run.</param>
        /// <param name="iteration">Repeat times.</param>
        /// <param name="name">The name of current performance.</param>
        /// <param name="outputAction">The action to handle the performance test result string.
        /// <example>Default: <code>Console.WriteLine</code></example>
        /// </param>
        public static void CodeTime(this Action action, int iteration = 1, string name = null, Action<string> outputAction = null)
        {
            DevLib.Diagnostics.CodeTimer.Time(action, iteration, name, outputAction);
        }
        */

        /// <summary>
        /// Encapsulates a method that has no parameters and does not return a value.
        /// </summary>
        public delegate void ActionDelegate();

        /// <summary>
        /// Initialize code snippets performence timer.
        /// </summary>
        public static void Initialize()
        {
            DevLib.Diagnostics.CodeTimer.Time(delegate() { }, 1, "Initialize CodeTimer...");
        }

        /// <summary>
        /// Run code snippets and give a performance test result.
        /// </summary>
        /// <param name="action">Code snippets to run.
        /// <example>E.g. <code>delegate() { Console.WriteLine("Hello"); }</code></example>
        /// </param>
        /// <param name="iteration">Repeat times.</param>
        /// <param name="name">The name of current performance.</param>
        /// <param name="outputAction">The action to handle the performance test result string.
        /// <example>Default: <code>Console.WriteLine</code></example>
        /// </param>
        public static void Time(ActionDelegate action, int iteration = 1, string name = null, Action<string> outputAction = null)
        {
            if ((action == null) || (iteration < 1))
            {
                return;
            }

            if (name == null)
            {
                name = action.Method.ToString();
            }

            if (outputAction == null)
            {
                outputAction = Console.WriteLine;
            }

            // Backup current thread priority
            var originalPriorityClass = Process.GetCurrentProcess().PriorityClass;
            var originalThreadPriority = Thread.CurrentThread.Priority;

            // Backup current console color
            ConsoleColor originalForeColor = Console.ForegroundColor;
            ConsoleColor consoleRandomColor = (ConsoleColor)_random.Next(1, 15);

            Console.ForegroundColor = consoleRandomColor;
            string beginTitle = string.Format("┌── Time Begin--> {0} ──┐", name);
            outputAction(beginTitle);
            Debug.WriteLine(beginTitle);
            Console.ForegroundColor = originalForeColor;

            // Record the latest GC counts
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            int[] gcCountArray = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCountArray[i] = GC.CollectionCount(i);
            }

            // Run action, record timespan
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ulong cycleCount = GetCycleCount();
            long threadTimeCount = GetCurrentThreadTimes();

            for (int i = 0; i < iteration; i++)
            {
                action();
            }

            ulong cpuCycles = GetCycleCount() - cycleCount;
            long threadTime = GetCurrentThreadTimes() - threadTimeCount;

            watch.Stop();

            string[] gcResultArray = { "NA", "NA", "NA" };

            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCountArray[i] = GC.CollectionCount(i) - gcCountArray[i];
                gcResultArray[i] = gcCountArray[i].ToString();
            }

            // Console output recorded times
            Console.ForegroundColor = ConsoleColor.White;
            string resultTitle = string.Format("{0,-17}{1,-18}{2,-17}{3,-2}/{4,-2}/{5,-2}", "Stopwatch", "ThreadTime", "CpuCycles", "G0", "G1", "G2");
            outputAction(resultTitle);
            Debug.WriteLine(resultTitle);

            Console.ForegroundColor = ConsoleColor.Green;

            string resultTime = string.Format("{0,7:N0}ms{1,16:N0}ms{2,17:N0}{3,10}{4,3}{5,3}", watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, gcResultArray[0], gcResultArray[1], gcResultArray[2]);
            outputAction(resultTime);
            Debug.WriteLine(resultTime);

            Console.ForegroundColor = consoleRandomColor;
            string endTitle = string.Format("└── Time End----> {0} ──┘", name);
            outputAction(endTitle);
            Debug.WriteLine(endTitle);

            // Restore console color
            Console.ForegroundColor = originalForeColor;

            // Restore thread priority
            Process.GetCurrentProcess().PriorityClass = originalPriorityClass;
            Thread.CurrentThread.Priority = originalThreadPriority;

            Console.WriteLine();
        }

        #region Native Methods Wrap
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr threadHandle, out long creationTime, out long exitTime, out long kernelTime, out long userTime);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private static ulong GetCycleCount()
        {
            ulong cycleCount = 0;
            try
            {
                QueryThreadCycleTime(GetCurrentThread(), ref cycleCount);
            }
            catch
            {
            }

            return cycleCount;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private static long GetCurrentThreadTimes()
        {
            long temp = 0;
            long kernelTime = 0;
            long userTimer = 0;
            try
            {
                GetThreadTimes(GetCurrentThread(), out temp, out temp, out kernelTime, out userTimer);
            }
            catch
            {
            }

            return kernelTime + userTimer;
        }
        #endregion
    }
}
