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
    /// Code performence timer
    /// </summary>
    [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
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
            ConsoleColor consoleRandomColor = (ConsoleColor)new Random().Next(9, 15);

            Console.ForegroundColor = consoleRandomColor;
            outputAction(string.Format("--Time Begin--> {0}", name));
            Debug.WriteLine(string.Format("--Time Begin--> {0}", name));
            Console.WriteLine();
            Console.ForegroundColor = originalForeColor;

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
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            string resultTitle = string.Format("{0,-17}{1,-18}{2,-17}{3,-2}/{4,-2}/{5,-2}", "Stopwatch", "ThreadTime", "CpuCycles", "G0", "G1", "G2");
            outputAction(resultTitle);
            Debug.WriteLine(resultTitle);

            Console.ForegroundColor = ConsoleColor.Green;

            string gcCount0 = "NA";
            string gcCount1 = "NA";
            string gcCount2 = "NA";
            try
            {
                gcCount0 = gcCounts[0].ToString();
                gcCount1 = gcCounts[1].ToString();
                gcCount2 = gcCounts[2].ToString();
            }
            catch
            {
            }

            string resultTime = string.Format("{0,7:N0}ms{1,16:N0}ms{2,17:N0}{3,10}{4,3}{5,3}", watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, gcCount0, gcCount1, gcCount2);
            outputAction(resultTime);
            Debug.WriteLine(resultTime);

            Console.ForegroundColor = consoleRandomColor;
            outputAction(string.Format("--Time End----> {0}", name));
            Debug.WriteLine(string.Format("--Time End----> {0}", name));

            // Restore console color
            Console.ForegroundColor = originalForeColor;

            // Restore thread priority
            Process.GetCurrentProcess().PriorityClass = originalPriorityClass;
            Thread.CurrentThread.Priority = originalThreadPriority;

            Console.WriteLine();
        }

        #region NativeMethods
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
