//-----------------------------------------------------------------------
// <copyright file="MemorySnapshot.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Xml;
    using DevLib.Diagnostics.NativeAPI;

    /// <summary>
    /// Represents a snapshot in time of the memory consumed by a specified OS process.
    /// MemorySnapshot objects can be instantiated from a running process or from a file.
    /// <p>MemorySnapshot objects are used for detection of <a href="http://en.wikipedia.org/wiki/Memory_leak">memory leaks</a>.</p>
    /// </summary>
    /// <example>
    /// The following example demonstrates taking two memory snapshots of Notepad and comparing them for leaks.
    /// <code>
    /// Process p = Process.Start("notepad.exe");
    /// p.WaitForInputIdle(5000);
    /// Thread.Sleep(3000);
    /// MemorySnapshot s1 = MemorySnapshot.FromProcess(p.Id);
    ///
    /// // Perform operations that may cause a leak...
    ///
    /// MemorySnapshot s2 = MemorySnapshot.FromProcess(p.Id);
    ///
    /// MemorySnapshot diff = s2.CompareTo(s1);
    /// if (diff.GdiObjectCount != 0)
    /// {
    ///     s1.ToFile(@"\s1.xml");
    ///     s2.ToFile(@"\s2.xml");
    ///     Console.WriteLine("Possible GDI handle leak! Review the saved memory snapshots.");
    /// }
    ///
    /// p.CloseMainWindow();
    /// p.Close();
    /// </code>
    /// </example>
    /// <remarks>
    /// <p>For more information on memory leak detection in native code, refer to the <a href="http://msdn.microsoft.com/en-us/library/x98tx3cf.aspx">
    /// Memory Leak Detection and Isolation</a> article. The table below provides a relationship between the metrics reported by the different tools:
    /// </p>
    /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
    /// <tr>
    ///   <td><strong>TestApi</strong></td>
    ///   <td><strong><a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></strong></td>
    ///   <td><strong><a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></strong></td>
    ///   <td><strong>Task Manager (Windows 7)</strong></td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="GdiObjectCount"/></td>
    ///   <td> - </td>
    ///   <td>Handles : GDI Objects</td>
    ///   <td>GDI Handles</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="HandleCount"/></td>
    ///   <td>HandleCount</td>
    ///   <td>Handles : Handles</td>
    ///   <td>Handles</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="PageFileBytes"/></td>
    ///   <td>PageFileBytes</td>
    ///   <td> - </td>
    ///   <td> - </td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="PageFilePeakBytes"/></td>
    ///   <td>PageFileBytesPeak</td>
    ///   <td> - </td>
    ///   <td> - </td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="PoolNonpagedBytes"/></td>
    ///   <td>Pool Nonpaged Bytes</td>
    ///   <td> - </td>
    ///   <td>NonPaged Pool</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="PoolPagedBytes"/></td>
    ///   <td>Pool Paged Bytes</td>
    ///   <td> - </td>
    ///   <td>Paged Pool</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="ThreadCount"/></td>
    ///   <td> - </td>
    ///   <td>Threads</td>
    ///   <td>Threads</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="Timestamp"/></td>
    ///   <td> - </td>
    ///   <td> - </td>
    ///   <td> - </td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="UserObjectCount"/></td>
    ///   <td> - </td>
    ///   <td>Handles : USER Objects</td>
    ///   <td>USER Handles</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="VirtualMemoryBytes"/></td>
    ///   <td>VirtualBytes</td>
    ///   <td>Virtual Memory : Virtual Size</td>
    ///   <td> - </td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="VirtualMemoryPrivateBytes"/></td>
    ///   <td>PrivateBytes</td>
    ///   <td>Virtual Memory : Private Bytes</td>
    ///   <td>Commit Size</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="WorkingSetBytes"/></td>
    ///   <td>WorkingSet</td>
    ///   <td>Physical Memory : WorkingSet</td>
    ///   <td>Working Set (Memory)</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="WorkingSetPeakBytes"/></td>
    ///   <td>WorkingSetPeak</td>
    ///   <td>Physical Memory : Peak Working Set</td>
    ///   <td>Peak Working Set (Memory)</td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="WorkingSetPrivateBytes"/></td>
    ///   <td>WorkingSetPrivate</td>
    ///   <td>Physical Memory : Working Set : WS Private</td>
    ///   <td>Memory (Private Working Set)</td>
    /// </tr>
    /// </table>
    /// </remarks>
    public class MemorySnapshot
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="MemorySnapshot" /> class from being created.
        /// </summary>
        private MemorySnapshot()
        {
        }

        /// <summary>
        /// Gets the number of handles to GDI objects in use by the process. For more information see the
        /// <a href="http://msdn.microsoft.com/en-us/library/ms683192(VS.85).aspx">GetGuiResources</a> function.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Handles : GDI Objects</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>GDI Handles</td></tr>
        /// </table>
        /// </remarks>
        public long GdiObjectCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total number of handles currently open by the process. This number is equal to the sum of the handles currently open by each thread in this process.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>HandleCount</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Handles : Handles</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Handles</td></tr>
        /// </table>
        /// </remarks>
        public long HandleCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current amount of virtual memory that this process has reserved for use in the paging file or files. Those pages may or may not be in memory.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>PageFileBytes</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>n/a</td></tr>
        /// </table>
        /// For more information, see the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">
        /// PROCESS_MEMORY_COUNTERS_EX</a> structure.
        /// </remarks>
        public IntPtr PageFileBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum amount of virtual memory that this process has reserved for use in the paging file or files.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>PageFileBytesPeak</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>n/a</td></tr>
        /// </table>
        /// For more information, see the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">
        /// PROCESS_MEMORY_COUNTERS_EX</a> structure.
        /// </remarks>
        public IntPtr PageFilePeakBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the <i>nonpaged pool</i>, an area of system memory (physical memory used by the operating system) for objects that cannot be written to disk but must remain in physical memory as long as they are allocated.
        /// For more information, see the <a href="http://msdn.microsoft.com/en-us/library/ms683219(VS.85).aspx">GetProcessMemoryInfo</a>
        /// function and the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">PROCESS_MEMORY_COUNTERS_EX</a> structure.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>Pool Nonpaged Bytes</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>NonPaged Pool</td></tr>
        /// </table>
        /// </remarks>
        public IntPtr PoolNonpagedBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the <i>paged pool</i>, an area of system memory (physical memory used by the operating system) for objects that can be written to disk when they are not being used.
        /// For more information, see the <a href="http://msdn.microsoft.com/en-us/library/ms683219(VS.85).aspx">GetProcessMemoryInfo</a> function
        /// and the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">PROCESS_MEMORY_COUNTERS_EX</a> structure.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>Pool Paged Bytes</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Paged Pool</td></tr>
        /// </table>
        /// </remarks>
        public IntPtr PoolPagedBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of threads currently active in the process.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Threads</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Threads</td></tr>
        /// </table>
        /// </remarks>
        public long ThreadCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time when the memory snapshot was taken.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time difference between two memory snapshots.
        /// </summary>
        public TimeSpan TimeDiff
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of handles to USER objects in use by the process. For more information see the
        /// <a href="http://msdn.microsoft.com/en-us/library/ms683192(VS.85).aspx">GetGuiResources</a> function.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>n/a</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Handles : USER Objects</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>USER Handles</td></tr>
        /// </table>
        /// </remarks>
        public long UserObjectCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current size of the virtual address space that the process is using.
        /// Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. Virtual space is finite, and the process can limit its ability to load libraries.
        /// For more information see the <a href="http://msdn.microsoft.com/en-us/library/aa366589(VS.85).aspx">GlobalMemoryStatusEx</a> function
        /// and the <a href="http://msdn.microsoft.com/en-us/library/aa366770(VS.85).aspx">MEMORYSTATUSEX</a> structure.
        /// This metric is calculated as MEMORYSTATUSEX.ullTotalVirtual MEMORYSTATUSEX.ullAvailVirtual.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>VirtualBytes</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Virtual Memory : Virtual Size</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>n/a</td></tr>
        /// </table>
        /// </remarks>
        public long VirtualMemoryBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current size of memory that this process has allocated that cannot be shared with other processes.
        /// For more information see the <a href="http://msdn.microsoft.com/en-us/library/ms683219(VS.85).aspx">GetProcessMemoryInfo</a> function
        /// and the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">PROCESS_MEMORY_COUNTERS_EX</a> structure (this metric corresponds to the <b>PrivateUsage</b> field in the structure).
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>PrivateBytes</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Virtual Memory : Private Bytes</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Commit Size</td></tr>
        /// </table>
        /// </remarks>
        public IntPtr VirtualMemoryPrivateBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current size of the <i>working set</i> of the process.
        /// The working set is the set of memory pages recently touched by the threads in the process.
        /// If free memory in the computer is above a threshold, pages are left in the working set of a process even if they are not in use.
        /// When free memory falls below a threshold, pages are trimmed from working sets.
        /// If they are needed they will then be soft-faulted back into the working set before leaving main memory.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>WorkingSet</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Physical Memory : Working Set</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Working Set (Memory)</td></tr>
        /// </table>
        /// </remarks>
        public long WorkingSetBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum size, in bytes, of the working set of the process at any one time.
        /// For more information see the <a href="http://msdn.microsoft.com/en-us/library/ms683219(VS.85).aspx">GetProcessMemoryInfo</a> function
        /// and the <a href="http://msdn.microsoft.com/en-us/library/ms684874(VS.85).aspx">PROCESS_MEMORY_COUNTERS_EX</a> structure.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>WorkingSetPeak</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Physical Memory : Peak Working Set</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Peak Working Set (Memory)</td></tr>
        /// </table>
        /// </remarks>
        public long WorkingSetPeakBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the working set that is only used for the process and not shared nor shareable by other processes.
        /// For more information see the <a href="http://msdn.microsoft.com/en-us/library/aa965225%28VS.85%29.aspx">Memory Performance Information</a> article.
        /// </summary>
        /// <remarks>
        /// <p> This metric is reported as follows by the other tools: </p>
        /// <table style="font-size:xx-small" border="1" bordercolor="#CCCCCC" cellpadding="1" cellspacing="0">
        /// <tr><td><strong>Tool</strong></td><td><strong>Metric</strong></td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://msdn.microsoft.com/en-us/library/aa373083(VS.85).aspx">Performance Counters</a></td>
        ///   <td>WorkingSetPrivate</td></tr>
        /// <tr>
        ///   <td>
        ///   <a href="http://technet.microsoft.com/en-us/sysinternals/bb896653.aspx">Process Explorer</a></td>
        ///   <td>Physical Memory : Working Set : WS Private</td></tr>
        /// <tr>
        ///   <td>Task Manager (Windows 7)</td>
        ///   <td>Memory (Private Working Set)</td></tr>
        /// </table>
        /// </remarks>
        public long WorkingSetPrivateBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a MemorySnapshot instance for the specified OS process.
        /// </summary>
        /// <param name="processId">The ID of the process for which to generate the memory snapshot.</param>
        /// <returns>A MemorySnapshot instance containing memory information for the specified process, at the time of the snapshot.</returns>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public static MemorySnapshot FromProcess(int processId)
        {
            MemorySnapshot memorySnapshot = new MemorySnapshot();

            Process process = Process.GetProcessById(processId);
            process.Refresh();

            PROCESS_MEMORY_COUNTERS_EX counters = MemoryInterop.GetCounters(process.Handle);

            memorySnapshot.GdiObjectCount = NativeMethods.GetGuiResources(process.Handle, NativeMethods.GR_GDIOBJECTS);
            memorySnapshot.HandleCount = process.HandleCount;
            memorySnapshot.PageFileBytes = counters.PagefileUsage;
            memorySnapshot.PageFilePeakBytes = counters.PeakPagefileUsage;
            memorySnapshot.PoolNonpagedBytes = counters.QuotaNonPagedPoolUsage;
            memorySnapshot.PoolPagedBytes = counters.QuotaPagedPoolUsage;
            memorySnapshot.ThreadCount = process.Threads.Count;
            memorySnapshot.UserObjectCount = NativeMethods.GetGuiResources(process.Handle, NativeMethods.GR_USEROBJECTS);
            memorySnapshot.VirtualMemoryBytes = process.VirtualMemorySize64;
            memorySnapshot.VirtualMemoryPrivateBytes = counters.PrivateUsage;
            memorySnapshot.WorkingSetBytes = process.WorkingSet64;
            memorySnapshot.WorkingSetPeakBytes = process.PeakWorkingSet64;
            memorySnapshot.WorkingSetPrivateBytes = MemoryInterop.GetPrivateWorkingSet(process);
            memorySnapshot.Timestamp = DateTime.Now;

            return memorySnapshot;
        }

        /// <summary>
        /// Creates a MemorySnapshot instance from data in the specified file.
        /// </summary>
        /// <param name="filename">The path to the memory snapshot file.</param>
        /// <returns>A MemorySnapshot instance containing memory information recorded in the specified file.</returns>
        public static MemorySnapshot FromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("MemorySnapshot.FromFile(): The specified file does not exist.", filename);
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filename);
                XmlNode rootNode = xmlDocument.DocumentElement;
                return Deserialize(rootNode);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Writes the current MemorySnapshot to a file.
        /// </summary>
        /// <param name="filename">The path to the output file.</param>
        public void ToFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("MemorySnapshot.ToFile(): the specified file path \"" + filename + "\" is null or empty.");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlNode rootNode = this.Serialize(xmlDocument);
                xmlDocument.AppendChild(rootNode);
                xmlDocument.Save(fullPath);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Compares the current MemorySnapshot instance to the specified MemorySnapshot to produce a difference.
        /// </summary>
        /// <param name="memorySnapshot">The MemorySnapshot to be compared to.</param>
        /// <returns>A new MemorySnapshot object representing the difference between the two memory snapshots (i.e. the result of the comparison).</returns>
        public MemorySnapshot CompareTo(MemorySnapshot memorySnapshot)
        {
            MemorySnapshot diff = new MemorySnapshot();

            diff.GdiObjectCount = this.GdiObjectCount - memorySnapshot.GdiObjectCount;
            diff.HandleCount = this.HandleCount - memorySnapshot.HandleCount;
            diff.PageFileBytes = new IntPtr(this.PageFileBytes.ToInt64() - memorySnapshot.PageFileBytes.ToInt64());
            diff.PageFilePeakBytes = new IntPtr(this.PageFilePeakBytes.ToInt64() - memorySnapshot.PageFilePeakBytes.ToInt64());
            diff.PoolNonpagedBytes = new IntPtr(this.PoolNonpagedBytes.ToInt64() - memorySnapshot.PoolNonpagedBytes.ToInt64());
            diff.PoolPagedBytes = new IntPtr(this.PoolPagedBytes.ToInt64() - memorySnapshot.PoolPagedBytes.ToInt64());
            diff.ThreadCount = this.ThreadCount - memorySnapshot.ThreadCount;
            diff.UserObjectCount = this.UserObjectCount - memorySnapshot.UserObjectCount;
            diff.VirtualMemoryBytes = this.VirtualMemoryBytes - memorySnapshot.VirtualMemoryBytes;
            diff.VirtualMemoryPrivateBytes = new IntPtr(this.VirtualMemoryPrivateBytes.ToInt64() - memorySnapshot.VirtualMemoryPrivateBytes.ToInt64());
            diff.WorkingSetBytes = this.WorkingSetBytes - memorySnapshot.WorkingSetBytes;
            diff.WorkingSetPeakBytes = this.WorkingSetPeakBytes - memorySnapshot.WorkingSetPeakBytes;
            diff.WorkingSetPrivateBytes = this.WorkingSetPrivateBytes - memorySnapshot.WorkingSetPrivateBytes;
            diff.TimeDiff = this.Timestamp - memorySnapshot.Timestamp;
            diff.Timestamp = this.Timestamp;

            return diff;
        }

        /// <summary>
        /// De-Serializes a MemorySnapshot instance from xml format.
        /// </summary>
        /// <param name="rootNode">The Xml Node from which the MemorySnapshot is to be de-serialized.</param>
        /// <returns>MemorySnapshot instance.</returns>
        internal static MemorySnapshot Deserialize(XmlNode rootNode)
        {
            MemorySnapshot result = new MemorySnapshot();

            result.GdiObjectCount = (long)DeserializeNode(rootNode, "GdiObjectCount");
            result.HandleCount = (long)DeserializeNode(rootNode, "HandleCount");
            result.PageFileBytes = DeserializeNode(rootNode, "PageFileBytes");
            result.PageFilePeakBytes = DeserializeNode(rootNode, "PageFilePeakBytes");
            result.PoolNonpagedBytes = DeserializeNode(rootNode, "PoolNonpagedBytes");
            result.PoolPagedBytes = DeserializeNode(rootNode, "PoolPagedBytes");
            result.ThreadCount = (long)DeserializeNode(rootNode, "ThreadCount");
            result.UserObjectCount = (long)DeserializeNode(rootNode, "UserObjectCount");
            result.VirtualMemoryBytes = (long)DeserializeNode(rootNode, "VirtualMemoryBytes");
            result.VirtualMemoryPrivateBytes = DeserializeNode(rootNode, "VirtualMemoryPrivateBytes");
            result.WorkingSetBytes = (long)DeserializeNode(rootNode, "WorkingSetBytes");
            result.WorkingSetPeakBytes = (long)DeserializeNode(rootNode, "WorkingSetPeakBytes");
            result.WorkingSetPrivateBytes = (long)DeserializeNode(rootNode, "WorkingSetPrivateBytes");

            XmlNode timestampNode = rootNode.SelectSingleNode("Timestamp");

            if (timestampNode == null)
            {
                throw new XmlException("MemorySnapshot file is missing value: Timestamp");
            }

            XmlAttribute timestampAttribute = timestampNode.Attributes["Value"];
            result.Timestamp = DateTime.ParseExact(timestampAttribute.InnerText, "yyyy-MM-ddTHH:mm:ss.fffffffUzzz", CultureInfo.InvariantCulture);

            XmlNode timeDiffNode = rootNode.SelectSingleNode("TimeDiff");

            if (timeDiffNode != null)
            {
                XmlAttribute timeDiffAttribute = timeDiffNode.Attributes["Value"];
                result.TimeDiff = TimeSpan.Parse(timestampAttribute.InnerText);
            }

            return result;
        }

        /// <summary>
        /// Serializes a MemorySnapshot instance to xml format.
        /// </summary>
        /// <param name="xmlDocument">The XmlDocument to which the MemorySnapshot is to be serialized.</param>
        /// <returns>XmlNode represents MemorySnapshot instance.</returns>
        internal XmlNode Serialize(XmlDocument xmlDocument)
        {
            XmlNode rootNode = xmlDocument.CreateElement("MemorySnapshot");

            this.SerializeNodes(xmlDocument, rootNode, "GdiObjectCount", this.GdiObjectCount);
            this.SerializeNodes(xmlDocument, rootNode, "HandleCount", this.HandleCount);
            this.SerializeNodes(xmlDocument, rootNode, "PageFileBytes", this.PageFileBytes);
            this.SerializeNodes(xmlDocument, rootNode, "PageFilePeakBytes", this.PageFilePeakBytes);
            this.SerializeNodes(xmlDocument, rootNode, "PoolNonpagedBytes", this.PoolNonpagedBytes);
            this.SerializeNodes(xmlDocument, rootNode, "PoolPagedBytes", this.PoolPagedBytes);
            this.SerializeNodes(xmlDocument, rootNode, "ThreadCount", this.ThreadCount);
            this.SerializeNodes(xmlDocument, rootNode, "UserObjectCount", this.UserObjectCount);
            this.SerializeNodes(xmlDocument, rootNode, "VirtualMemoryBytes", this.VirtualMemoryBytes);
            this.SerializeNodes(xmlDocument, rootNode, "VirtualMemoryPrivateBytes", this.VirtualMemoryPrivateBytes);
            this.SerializeNodes(xmlDocument, rootNode, "WorkingSetBytes", this.WorkingSetBytes);
            this.SerializeNodes(xmlDocument, rootNode, "WorkingSetPeakBytes", this.WorkingSetPeakBytes);
            this.SerializeNodes(xmlDocument, rootNode, "WorkingSetPrivateBytes", this.WorkingSetPrivateBytes);

            XmlNode timestampNode = xmlDocument.CreateElement("Timestamp");
            XmlAttribute timestampAttribute = xmlDocument.CreateAttribute("Value");
            timestampAttribute.InnerText = this.Timestamp.ToString("o", CultureInfo.InvariantCulture);
            timestampNode.Attributes.Append(timestampAttribute);
            rootNode.AppendChild(timestampNode);

            XmlNode timeDiffNode = xmlDocument.CreateElement("TimeDiff");
            XmlAttribute timeDiffAttribute = xmlDocument.CreateAttribute("Value");
            timeDiffAttribute.InnerText = this.TimeDiff.ToString();
            timeDiffNode.Attributes.Append(timeDiffAttribute);
            rootNode.AppendChild(timeDiffNode);

            return rootNode;
        }

        /// <summary>
        /// Deserialize xml node.
        /// </summary>
        /// <param name="rootNode">Root node to deserialize.</param>
        /// <param name="nodeName">Node name to deserialize.</param>
        /// <returns>IntPtr instance.</returns>
        private static IntPtr DeserializeNode(XmlNode rootNode, string nodeName)
        {
            XmlNode xmlNode = rootNode.SelectSingleNode(nodeName);

            if (xmlNode == null)
            {
                throw new XmlException("MemorySnapshot file is missing value: " + nodeName);
            }

            XmlAttribute attribute = xmlNode.Attributes["Value"];

            return new IntPtr(Convert.ToInt64(attribute.InnerText, NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Serialize object to xml document.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="xmlDocument">XmlDocument to serialize.</param>
        /// <param name="rootNode">Root node to serialize.</param>
        /// <param name="nodeName">Node name to serialize.</param>
        /// <param name="value">Object to serialize.</param>
        private void SerializeNodes<T>(XmlDocument xmlDocument, XmlNode rootNode, string nodeName, T value)
        {
            XmlNode newNode = xmlDocument.CreateElement(nodeName);
            XmlAttribute attribute = xmlDocument.CreateAttribute("Value");
            attribute.InnerText = value.ToString();
            newNode.Attributes.Append(attribute);
            rootNode.AppendChild(newNode);
        }
    }
}
