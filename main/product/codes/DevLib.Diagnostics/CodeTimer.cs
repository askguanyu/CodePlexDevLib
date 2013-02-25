//-----------------------------------------------------------------------
// <copyright file="CodeTimer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Code snippets performance timer.
    /// </summary>
    [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
    public static class CodeTimer
    {
        /// <summary>
        /// Static Field _random.
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
        /// Initialize code snippets performance timer.
        /// </summary>
        public static void Initialize()
        {
            DevLib.Diagnostics.CodeTimer.Time(delegate { }, 1, string.Empty, delegate { });
        }

        /// <summary>
        /// Run code snippets and give a performance test result.
        /// </summary>
        /// <param name="action">Code snippets to run.
        /// <example>E.g. <code>delegate { Console.WriteLine("Hello"); }</code></example>
        /// </param>
        /// <param name="iteration">Repeat times.</param>
        /// <param name="name">The name of current performance.</param>
        /// <param name="outputAction">The action to handle the performance test result string.
        /// <example>Default: <code>Console.WriteLine</code></example>
        /// </param>
        /// <returns>CodeTimer result.</returns>
        public static CodeTimerResult Time(ActionDelegate action, int iteration = 1, string name = null, Action<string> outputAction = null)
        {
            if ((action == null) || (iteration < 1))
            {
                return null;
            }

            if (name == null)
            {
                name = action.Method.ToString();
            }

            if (outputAction == null)
            {
                outputAction = Console.WriteLine;
            }

            string titleName = name.PadRight(53, '-');

            // Backup current thread priority
            ProcessPriorityClass originalPriorityClass = Process.GetCurrentProcess().PriorityClass;
            ThreadPriority originalThreadPriority = Thread.CurrentThread.Priority;

            // Backup current console color
            ConsoleColor originalForeColor = Console.ForegroundColor;
            ConsoleColor consoleRandomColor = (ConsoleColor)_random.Next(1, 15);

            Console.ForegroundColor = consoleRandomColor;
            string beginTitle = string.Format(@"/--Time Begin-->{0}--\", titleName);
            outputAction(beginTitle);
            Debug.WriteLine(beginTitle);
            Console.ForegroundColor = originalForeColor;

            // Record the latest GC counts
            int gcArrayLength = GC.MaxGeneration + 1;

            int[] gcCountArray = new int[gcArrayLength];

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            for (int i = 0; i < gcArrayLength; i++)
            {
                gcCountArray[i] = GC.CollectionCount(i);
            }

            // Run action, record timespan
            Stopwatch watch = new Stopwatch();

            ulong cycleCount = GetCycleCount();
            long threadTimeCount = GetCurrentThreadTime();

            watch.Start();

            for (int i = 0; i < iteration; i++)
            {
                action();
            }

            watch.Stop();

            long threadTime = GetCurrentThreadTime() - threadTimeCount;
            ulong cpuCycles = GetCycleCount() - cycleCount;

            for (int i = 0; i < gcArrayLength; i++)
            {
                gcCountArray[i] = GC.CollectionCount(i) - gcCountArray[i];
            }

            string[] gcTitleArray = new string[gcArrayLength];
            string[] gcResultArray = new string[gcArrayLength];
            for (int i = 0; i < gcArrayLength; i++)
            {
                gcTitleArray[i] = string.Format("G{0}", i);
                gcResultArray[i] = gcCountArray[i].ToString();
            }

            Console.WriteLine();

            // Console output recorded times
            Console.ForegroundColor = ConsoleColor.White;
            string resultTitle = string.Format("{0,18}{1,18}{2,18}{3,18}", "[Stopwatch]", "[ThreadTime]", "[CPUCycles]", string.Format("[{0}]", string.Join("/", gcTitleArray)));
            outputAction(resultTitle);
            Debug.WriteLine(resultTitle);

            Console.ForegroundColor = ConsoleColor.Green;
            string resultTime = string.Format("{0,16:N0}ms{1,16:N0}ms{2,18:N0}{3,18}", watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, string.Join("/", gcResultArray));
            outputAction(resultTime);
            Debug.WriteLine(resultTime);

            Console.ForegroundColor = consoleRandomColor;
            string endTitle = string.Format(@"\--Time End---->{0}--/", titleName);
            outputAction(endTitle);
            Debug.WriteLine(endTitle);

            // Restore console color
            Console.ForegroundColor = originalForeColor;

            // Restore thread priority
            Process.GetCurrentProcess().PriorityClass = originalPriorityClass;
            Thread.CurrentThread.Priority = originalThreadPriority;

            Console.WriteLine();

            CodeTimerResult result = new CodeTimerResult(watch.ElapsedMilliseconds, threadTime / 10000, cpuCycles, gcCountArray);
            return result;
        }

        #region Native Methods Wrap
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Reviewed.")]
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Reviewed.")]
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentThread();

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Reviewed.")]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetThreadTimes(IntPtr threadHandle, out long creationTime, out long exitTime, out long kernelTime, out long userTime);

        /// <summary>
        /// Static Method GetCycleCount.
        /// </summary>
        /// <returns>Cycle count.</returns>
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
        /// Static Method GetCurrentThreadTime.
        /// </summary>
        /// <returns>Thread time.</returns>
        private static long GetCurrentThreadTime()
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

    /// <summary>
    /// Code snippets performance test result.
    /// </summary>
    public class CodeTimerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeTimerResult" /> class.
        /// </summary>
        /// <param name="stopwatchElapsedMilliseconds">Stopwatch timespan in milliseconds.</param>
        /// <param name="threadTimeElapsedMilliseconds">ThreadTime timespan in milliseconds.</param>
        /// <param name="cpuCycles">CPU cycles.</param>
        /// <param name="gcCountArray">GC Count Array.</param>
        public CodeTimerResult(long stopwatchElapsedMilliseconds, long threadTimeElapsedMilliseconds, ulong cpuCycles, int[] gcCountArray)
        {
            this.StopwatchElapsedMilliseconds = stopwatchElapsedMilliseconds;
            this.ThreadTimeElapsedMilliseconds = threadTimeElapsedMilliseconds;
            this.CPUCycles = cpuCycles;
            this.GCCountArray = gcCountArray;
        }

        /// <summary>
        /// Gets stopwatch timespan in milliseconds.
        /// </summary>
        public long StopwatchElapsedMilliseconds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets threadTime timespan in milliseconds.
        /// </summary>
        public long ThreadTimeElapsedMilliseconds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets CPU cycles.
        /// </summary>
        public ulong CPUCycles
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets GC Count Array.
        /// </summary>
        public int[] GCCountArray
        {
            get;
            private set;
        }
    }
}
