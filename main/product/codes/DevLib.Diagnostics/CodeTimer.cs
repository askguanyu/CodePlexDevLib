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
    using System.Threading;

    /// <summary>
    /// Code performence timer
    /// </summary>
    public static class CodeTimer
    {
        /// <summary>
        ///
        /// </summary>
        public static void Initialize()
        {
            Time("Initialize CodeTimer...", 1, () => { });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iteration"></param>
        /// <param name="action">E.g. () => { Console.WriteLine("Hello"); }</param>
        public static void Time(string name, int iteration, Action action)
        {
            DevLib.Diagnostics.CodeTimer.Time(name, iteration, action, Console.WriteLine);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iteration"></param>
        /// <param name="action">E.g. () => { Console.WriteLine("Hello"); }</param>
        /// <param name="outputAction"></param>
        public static void Time(string name, int iteration, Action action, Action<string> outputAction)
        {
            if (String.IsNullOrEmpty(name) || action == null) return;

            // Backup current thread priority
            var originalPriorityClass = Process.GetCurrentProcess().PriorityClass;
            var originalThreadPriority = Thread.CurrentThread.Priority;

            // Format console color, print name
            ConsoleColor originalForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            outputAction(name);
            Console.ForegroundColor = ConsoleColor.Gray;
            outputAction(string.Format("{0,-17}{1,-18}{2,-17}{3,-2}/{4,-2}/{5,-2}", "Stopwatch", "ThreadTime", "CpuCycles", "G0", "G1", "G2"));

            // Record the latest GC counts
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // Run action, record timespan
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ulong cycleCount = GetCycleCount();
            long threadTimeCount = GetCurrentThreadTimes();

            for (int i = 0; i < iteration; i++) action();

            ulong cpuCycles = GetCycleCount() - cycleCount;
            long threadTime = GetCurrentThreadTimes() - threadTimeCount;

            watch.Stop();

            int[] gen = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gen[i] = GC.CollectionCount(i) - gcCounts[i];
            }

            // Console output recorded times
            Console.ForegroundColor = ConsoleColor.Green;
            outputAction(string.Format("{0,7:N0}ms{1,16:N0}ms{2,17:N0}{3,10}{4,3}{5,3}", watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, gen[0], gen[1], gen[2]));

            // Restore console color
            Console.ForegroundColor = originalForeColor;

            // Restore thread priority
            Process.GetCurrentProcess().PriorityClass = originalPriorityClass;
            Thread.CurrentThread.Priority = originalThreadPriority;

            Console.WriteLine();
        }

        #region NativeMethods
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr threadHandle, out long creationTime, out long exitTime, out long kernelTime, out long userTime);

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private static ulong GetCycleCount()
        {
            ulong cycleCount = 0;
            QueryThreadCycleTime(GetCurrentThread(), ref cycleCount);
            return cycleCount;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private static long GetCurrentThreadTimes()
        {
            long temp;
            long kernelTime, userTimer;
            GetThreadTimes(GetCurrentThread(), out temp, out temp, out kernelTime, out userTimer);
            return kernelTime + userTimer;
        }
        #endregion
    }
}
