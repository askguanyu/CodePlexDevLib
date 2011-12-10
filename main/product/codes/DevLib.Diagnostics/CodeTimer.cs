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
            DevLib.Diagnostics.CodeTimer.Time(1, "Initialize CodeTimer...", () => { });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iteration"></param>
        public static void CodeTime(this Action action, int iteration)
        {
            DevLib.Diagnostics.CodeTimer.Time(iteration, action);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iteration"></param>
        /// <param name="name"></param>
        public static void CodeTime(this Action action, int iteration, string name)
        {
            DevLib.Diagnostics.CodeTimer.Time(iteration, name, action);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iteration"></param>
        /// <param name="name"></param>
        /// <param name="outputAction"></param>
        public static void CodeTime(this Action action, int iteration, string name, Action<string> outputAction)
        {
            DevLib.Diagnostics.CodeTimer.Time(iteration, name, action, outputAction);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="action">E.g. () => { Console.WriteLine("Hello"); }</param>
        public static void Time(int iteration, Action action)
        {
            DevLib.Diagnostics.CodeTimer.Time(iteration, action.Method.ToString(), action, Console.WriteLine);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="name"></param>
        /// <param name="action">E.g. () => { Console.WriteLine("Hello"); }</param>
        public static void Time(int iteration, string name, Action action)
        {
            DevLib.Diagnostics.CodeTimer.Time(iteration, name, action, Console.WriteLine);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="name"></param>
        /// <param name="action">E.g. () => { Console.WriteLine("Hello"); }</param>
        /// <param name="outputAction"></param>
        public static void Time(int iteration, string name, Action action, Action<string> outputAction)
        {
            if ((iteration < 1) || String.IsNullOrEmpty(name) || (action == null))
            {
                return;
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            outputAction(string.Format("Begin:  {0}", name));

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

            for (int i = 0; i < iteration; i++)
            {
                action();
            }

            ulong cpuCycles = GetCycleCount() - cycleCount;
            long threadTime = GetCurrentThreadTimes() - threadTimeCount;

            watch.Stop();

            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i) - gcCounts[i];
            }

            // Console output recorded times
            Console.ForegroundColor = ConsoleColor.Yellow;
            outputAction(string.Format("End:    {0}", name));

            Console.ForegroundColor = ConsoleColor.Gray;
            outputAction(string.Format("{0,-17}{1,-18}{2,-17}{3,-2}/{4,-2}/{5,-2}", "Stopwatch", "ThreadTime", "CpuCycles", "G0", "G1", "G2"));

            Console.ForegroundColor = ConsoleColor.Green;
            outputAction(string.Format("{0,7:N0}ms{1,16:N0}ms{2,17:N0}{3,10}{4,3}{5,3}", watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, gcCounts[0], gcCounts[1], gcCounts[2]));

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
